using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;
using Freyja.NpcScheduling.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Freyja.NpcScheduling.Editor
{
    internal enum NpcScheduleDocumentKind { Catalog, Recipes, Template, Npc, Override }

    internal sealed class NpcScheduleDocument
    {
        public string AssetPath;
        public string FullPath;
        public string Text;
        public string Fingerprint;
        public string Id;
        public NpcScheduleDocumentKind Kind;
        public bool RuntimeLoaded;
        public bool ReadOnly;
        public string Error;
        public bool IsDemo;
        public string ValidationStatus = "incomplete";
        public string ValidationReason = "ainda não validado";
        public string UsageLabel => RuntimeLoaded ? "RUNTIME" : IsDemo ? "DEMO" : "BIBLIOTECA";
        public string Label => Id + "  ·  " + Kind + "  ·  " + UsageLabel + "  ·  " + ValidationStatus.ToUpperInvariant();
    }

    internal sealed class NpcScheduleValidation
    {
        public ResolvedScheduleData Data;
        public IDictionary<string, ActivityRecipe> Recipes;
        public readonly List<string> Diagnostics = new List<string>();
        public string Error;
        public bool IsValid => Data != null && Error == null;
    }

    internal sealed class NpcScheduleEditorRepository
    {
        internal const string Root = "Assets/_Project/NpcScheduling/Data";
        private readonly List<NpcScheduleDocument> documents = new List<NpcScheduleDocument>();
        public IReadOnlyList<NpcScheduleDocument> Documents => documents;
        public int Revision { get; private set; }
        public NpcScheduleDestinationSnapshot Destinations { get; private set; }
        public NpcScheduleValidation SourceValidation { get; private set; }

        public void Refresh()
        {
            Revision++;
            documents.Clear();
            HashSet<string> runtime = RuntimeAssetPaths();
            foreach (string guid in AssetDatabase.FindAssets("t:TextAsset", new[] { Root }))
            {
                string asset = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                if (!asset.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || asset.Contains("/test-fixtures/")) continue;
                NpcScheduleDocumentKind kind;
                if (asset == Root + "/activities.json") kind = NpcScheduleDocumentKind.Catalog;
                else if (asset == Root + "/execution/recipes.json") kind = NpcScheduleDocumentKind.Recipes;
                else if (asset.StartsWith(Root + "/templates/", StringComparison.Ordinal)) kind = NpcScheduleDocumentKind.Template;
                else if (asset.StartsWith(Root + "/npcs/", StringComparison.Ordinal)) kind = NpcScheduleDocumentKind.Npc;
                else if (asset.StartsWith(Root + "/overrides/", StringComparison.Ordinal)) kind = NpcScheduleDocumentKind.Override;
                else continue;
                string full = FullPath(asset);
                string text = File.ReadAllText(full, Encoding.UTF8);
                string id = Path.GetFileNameWithoutExtension(asset);
                try
                {
                    var parsed = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(text);
                    string key = kind == NpcScheduleDocumentKind.Npc ? "npcId" : "id";
                    id = (string)parsed?[key] ?? id;
                }
                catch { }
                bool demo = id.StartsWith("demo_", StringComparison.Ordinal) || asset.IndexOf("/demo", StringComparison.OrdinalIgnoreCase) >= 0;
                documents.Add(new NpcScheduleDocument
                {
                    AssetPath = asset, FullPath = full, Text = text, Fingerprint = Fingerprint(text),
                    Id = id, Kind = kind, RuntimeLoaded = runtime.Contains(asset), IsDemo = demo
                });
            }
            documents.Sort((a, b) => StringComparer.Ordinal.Compare(a.AssetPath, b.AssetPath));
            foreach (NpcScheduleDocument template in documents.Where(x => x.Kind == NpcScheduleDocumentKind.Template))
            {
                NpcScheduleDocument[] dependents = documents.Where(x => x.Kind == NpcScheduleDocumentKind.Npc && (string)Parse(x.Text)?["templateId"] == template.Id).ToArray();
                if (dependents.Length > 0 && dependents.All(x => x.IsDemo)) template.IsDemo = true;
            }
            Destinations = NpcScheduleDestinationSnapshot.Capture();
            SourceValidation = ValidateAll();
            UpdateDocumentStatuses();
        }

        public void RefreshDestinations()
        {
            Revision++;
            Destinations = NpcScheduleDestinationSnapshot.Capture();
            UpdateDocumentStatuses();
        }

        public NpcScheduleValidation ValidateAll(NpcScheduleDocument selected = null, string candidate = null)
        {
            var result = new NpcScheduleValidation();
            try
            {
                string activities = TextFor(NpcScheduleDocumentKind.Catalog, selected, candidate);
                string recipes = TextFor(NpcScheduleDocumentKind.Recipes, selected, candidate);
                var templates = new List<ScheduleTextDocument>();
                var npcs = new List<ScheduleTextDocument>();
                var overrides = new List<ScheduleTextDocument>();
                foreach (NpcScheduleDocument document in documents)
                {
                    string text = TextFor(document.Kind, selected, candidate, document);
                    var item = new ScheduleTextDocument(document.AssetPath, text);
                    if (document.Kind == NpcScheduleDocumentKind.Template) templates.Add(item);
                    else if (document.Kind == NpcScheduleDocumentKind.Npc) npcs.Add(item);
                    else if (document.Kind == NpcScheduleDocumentKind.Override) overrides.Add(item);
                }
                ScheduleDataLoadResult loaded = ScheduleDataLoader.Load(new ScheduleDataDocuments(activities, templates, npcs, overrides));
                result.Data = loaded.Data;
                result.Diagnostics.AddRange(loaded.Diagnostics);
                result.Recipes = ActivityRecipeLoader.Load(recipes, loaded.Data.Activities, "execution/recipes.json");
            }
            catch (Exception ex) { result.Error = ex.Message; }
            return result;
        }

        private void UpdateDocumentStatuses()
        {
            foreach (NpcScheduleDocument document in documents)
            {
                if (SourceValidation == null || !SourceValidation.IsValid)
                {
                    string message = SourceValidation?.Error ?? "dados da fonte indisponíveis";
                    bool belongsToDocument = message.IndexOf(document.AssetPath, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        message.IndexOf(Path.GetFileName(document.AssetPath), StringComparison.OrdinalIgnoreCase) >= 0;
                    document.ValidationStatus = belongsToDocument ? "inválido" : "incompleto";
                    document.ValidationReason = message;
                    continue;
                }

                string reason = IncompleteReason(document);
                document.ValidationStatus = reason == null ? "válido" : "incompleto";
                document.ValidationReason = reason ?? "loader e receitas aceitaram a fonte";
            }
        }

        private string IncompleteReason(NpcScheduleDocument document)
        {
            if (document.Kind == NpcScheduleDocumentKind.Catalog || document.Kind == NpcScheduleDocumentKind.Recipes) return null;
            var entries = new List<ScheduleEntry>();
            if (document.Kind == NpcScheduleDocumentKind.Npc)
            {
                if (SourceValidation.Data.NpcSchedules.TryGetValue(document.Id, out IList<ScheduleEntry> resolved)) entries.AddRange(resolved);
            }
            else if (document.Kind == NpcScheduleDocumentKind.Template)
            {
                foreach (NpcScheduleDocument npc in documents.Where(x => x.Kind == NpcScheduleDocumentKind.Npc))
                    if ((string)Parse(npc.Text)?["templateId"] == document.Id && SourceValidation.Data.NpcSchedules.TryGetValue(npc.Id, out IList<ScheduleEntry> resolved))
                        entries.AddRange(resolved);
            }
            else if (document.Kind == NpcScheduleDocumentKind.Override)
            {
                JObject root = Parse(document.Text);
                foreach (JObject entry in (root?["entries"] as Newtonsoft.Json.Linq.JArray ?? new Newtonsoft.Json.Linq.JArray()).OfType<JObject>())
                {
                    string activityId = (string)entry["activityId"];
                    string targetId = (string)entry["targetId"];
                    if (!SourceValidation.Recipes.ContainsKey(activityId)) return "atividade sem receita executável: " + activityId;
                    if (SourceValidation.Data.Activities.TryGetValue(activityId, out ActivityDefinition activity) &&
                        Destinations.StatusFor(activityId, activity.TargetKind, targetId) != "resolved")
                        return "destino " + (targetId ?? "ausente") + " não comprovado nesta cena";
                }
            }

            foreach (ScheduleEntry entry in entries)
            {
                if (!SourceValidation.Recipes.ContainsKey(entry.ActivityId)) return "atividade sem receita executável: " + entry.ActivityId;
                if (SourceValidation.Data.Activities.TryGetValue(entry.ActivityId, out ActivityDefinition activity) &&
                    Destinations.StatusFor(entry.ActivityId, activity.TargetKind, entry.TargetId) != "resolved")
                    return "destino " + (entry.TargetId ?? "ausente") + " não comprovado nesta cena";
            }
            return null;
        }

        private static Newtonsoft.Json.Linq.JObject Parse(string text)
        { try { return Newtonsoft.Json.Linq.JObject.Parse(text); } catch { return null; } }

        public bool TrySave(NpcScheduleDocument document, string draft, string originalFingerprint, out string error)
        {
            error = null;
            string full = FullPath(document.AssetPath);
            if (!full.StartsWith(FullPath(Root) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            { error = "O destino saiu da pasta canônica de agendas."; return false; }
            if (!File.Exists(full) || Fingerprint(File.ReadAllText(full, Encoding.UTF8)) != originalFingerprint)
            { error = "Conflito: o arquivo foi removido ou alterado por outra ferramenta. O rascunho foi preservado."; return false; }
            NpcScheduleValidation validation = ValidateAll(document, draft);
            if (!validation.IsValid) { error = validation.Error; return false; }

            string original = File.ReadAllText(full, Encoding.UTF8);
            string temp = full + ".npc-schedule-tmp";
            string backup = full + ".npc-schedule-bak";
            bool preserveBackup = false;
            try
            {
                File.WriteAllText(backup, original, new UTF8Encoding(false));
                File.WriteAllText(temp, draft, new UTF8Encoding(false));
                if (!File.Exists(full) || Fingerprint(File.ReadAllText(full, Encoding.UTF8)) != originalFingerprint)
                { error = "Conflito: o arquivo mudou durante a validação. O rascunho foi preservado."; return false; }
                if (File.Exists(temp)) File.Replace(temp, full, null);
                else throw new IOException("Arquivo temporário não foi criado.");
                AssetDatabase.ImportAsset(document.AssetPath, ImportAssetOptions.ForceUpdate);
                TextAsset imported = AssetDatabase.LoadAssetAtPath<TextAsset>(document.AssetPath);
                if (imported == null || imported.text != draft) throw new IOException("A reimportação não carregou exatamente o conteúdo salvo.");
                Refresh();
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(temp)) File.Delete(temp);
                    if (File.Exists(backup)) File.Replace(backup, full, null);
                    else File.WriteAllText(full, original, new UTF8Encoding(false));
                    AssetDatabase.ImportAsset(document.AssetPath, ImportAssetOptions.ForceUpdate);
                }
                catch (Exception rollback) { preserveBackup = true; error = "Falha ao salvar: " + ex.Message + ". Falha também ao restaurar a origem: " + rollback.Message + ". Cópia de recuperação preservada ao lado do arquivo."; return false; }
                error = "Falha ao salvar; o conteúdo original foi restaurado: " + ex.Message;
                return false;
            }
            finally { if (File.Exists(temp)) File.Delete(temp); if (!preserveBackup && File.Exists(backup)) File.Delete(backup); }
        }

        public bool TryCreateNpc(string assetPath, string json, out string error)
        {
            error = null;
            string full = FullPath(assetPath);
            if (!assetPath.StartsWith(Root + "/npcs/", StringComparison.Ordinal) || !full.StartsWith(FullPath(Root) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            { error = "A cópia precisa ficar na pasta canônica de NPCs."; return false; }
            if (File.Exists(full) || File.Exists(full + ".meta")) { error = "O destino já existe."; return false; }
            NpcScheduleDocument candidate = new NpcScheduleDocument { AssetPath = assetPath, FullPath = full, Text = json, Kind = NpcScheduleDocumentKind.Npc };
            try { candidate.Id = (string)Newtonsoft.Json.Linq.JObject.Parse(json)["npcId"]; }
            catch (Exception ex) { error = "JSON da cópia inválido: " + ex.Message; return false; }
            documents.Add(candidate);
            NpcScheduleValidation validation = ValidateAll();
            documents.Remove(candidate);
            if (!validation.IsValid) { error = validation.Error; return false; }

            string temp = full + ".npc-schedule-tmp";
            bool targetCreated = false;
            try
            {
                File.WriteAllText(temp, json, new UTF8Encoding(false));
                File.Move(temp, full);
                targetCreated = true;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                Refresh();
                if (!ValidateAll().IsValid) throw new InvalidDataException("A cópia importada falhou na revalidação.");
                return true;
            }
            catch (Exception ex)
            {
                if (File.Exists(temp)) File.Delete(temp);
                if (targetCreated && File.Exists(full)) File.Delete(full);
                if (targetCreated && (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null || File.Exists(full + ".meta"))) AssetDatabase.DeleteAsset(assetPath);
                error = "Falha ao criar cópia; o arquivo novo foi removido: " + ex.Message;
                Refresh();
                return false;
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }

        public NpcScheduleDocument FindByPath(string path) => documents.FirstOrDefault(x => x.AssetPath == path);

        private string TextFor(NpcScheduleDocumentKind kind, NpcScheduleDocument selected, string candidate, NpcScheduleDocument current = null)
        {
            if (selected != null && candidate != null && (current == null ? selected.Kind == kind : current.AssetPath == selected.AssetPath)) return candidate;
            NpcScheduleDocument doc = current ?? documents.FirstOrDefault(x => x.Kind == kind);
            if (doc == null) throw new InvalidDataException("Documento obrigatório ausente: " + kind);
            return doc.Text;
        }

        private static HashSet<string> RuntimeAssetPaths()
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            Scene active = SceneManager.GetActiveScene();
            foreach (NpcScheduleTextAssetProvider provider in Resources.FindObjectsOfTypeAll<NpcScheduleTextAssetProvider>())
            {
                if (provider == null || EditorUtility.IsPersistent(provider) || !provider.gameObject.scene.IsValid() || provider.gameObject.scene != active) continue;
                var serialized = new SerializedObject(provider);
                AddAsset(serialized.FindProperty("activityCatalog")?.objectReferenceValue, result);
                AddAsset(serialized.FindProperty("activityRecipes")?.objectReferenceValue, result);
                AddArray(serialized.FindProperty("templates"), result);
                AddArray(serialized.FindProperty("npcSchedules"), result);
                AddArray(serialized.FindProperty("overrides"), result);
            }
            return result;
        }

        private static void AddArray(SerializedProperty array, HashSet<string> result)
        {
            for (int i = 0; array != null && i < array.arraySize; i++) AddAsset(array.GetArrayElementAtIndex(i).objectReferenceValue, result);
        }

        private static void AddAsset(UnityEngine.Object value, HashSet<string> result)
        {
            if (value == null) return;
            string path = AssetDatabase.GetAssetPath(value).Replace('\\', '/');
            if (path.StartsWith(Root + "/", StringComparison.Ordinal)) result.Add(path);
        }

        internal static string Fingerprint(string value)
        {
            using (SHA256 sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }

        private static string FullPath(string assetPath) => Path.GetFullPath(Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)));
    }
}
