#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Freyja.Narrative.Editor
{
    public static class NarrativeCsvValidator
    {
        private const string Root = "Assets/_Project/Narrative";
        private const string YarnPath = Root + "/Content/Demo/Demo.yarn";
        private const string MasterPath = Root + "/Localization/strings.csv";
        private static readonly string[] Columns = { "line_id", "speaker_id", "node_id", "source_file", "kind", "context", "source_hash", "pt-BR", "en-US", "translation_status" };

        [MenuItem("Freyja/Narrative/Create runtime Yarn reference")]
        public static void CreateRuntimeReference()
        {
            var path = Root + "/Resources/Narrative/DemoYarnProjectReference.asset";
            if (AssetDatabase.LoadAssetAtPath<DemoYarnProjectReference>(path) != null) return;
            var project = AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>(Root + "/Content/Demo/Demo.yarnproject");
            if (project == null || project.Program == null)
                throw new InvalidOperationException("Demo Yarn Project não compilado/importado.");
            var reference = ScriptableObject.CreateInstance<DemoYarnProjectReference>();
            reference.SetProject(project);
            AssetDatabase.CreateAsset(reference, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Freyja/Narrative/Export or merge translation CSV")]
        public static void ExportOrMerge()
        {
            var parsed = ParseSource(File.ReadAllText(YarnPath));
            var oldRows = File.Exists(MasterPath) ? Csv.Read(File.ReadAllText(MasterPath)) : new List<List<string>>();
            if (oldRows.Count > 0) ValidateMasterSchema(oldRows);
            var oldDuplicates = DuplicateIds(oldRows);
            if (oldDuplicates.Count > 0) throw new FormatException("IDs duplicados no CSV mestre: " + string.Join(", ", oldDuplicates));
            var parsedDuplicates = DuplicateIds(new List<List<string>> { Columns.ToList() }.Concat(parsed).ToList());
            if (parsedDuplicates.Count > 0) throw new FormatException("IDs duplicados no Yarn: " + string.Join(", ", parsedDuplicates));
            var oldById = Index(oldRows);
            var merged = new List<List<string>> { Columns.ToList() };
            foreach (var source in parsed)
            {
                oldById.TryGetValue(source[0], out var previous);
                if (previous == null && source[0].StartsWith("line:", StringComparison.Ordinal))
                    oldById.TryGetValue(source[0].Substring("line:".Length), out previous);
                var translated = previous != null && previous.Count > 8 ? previous[8] : "";
                var priorHash = previous != null && previous.Count > 6 ? previous[6] : "";
                var unchangedStaleTranslation = previous != null && previous.Count > 9 && previous[9] == "stale"
                    && previous.Count > 8 && previous[8] == translated;
                var status = string.IsNullOrEmpty(translated) ? "missing"
                    : unchangedStaleTranslation ? "stale" : priorHash == source[6] ? "translated" : "stale";
                source[8] = translated;
                source[9] = !string.IsNullOrEmpty(translated) && !PlaceholdersMatch(source[7], translated)
                    ? "invalid_placeholders" : status;
                merged.Add(source);
            }
            var parsedIdSet = new HashSet<string>(parsed.SelectMany(p => p[0].StartsWith("line:", StringComparison.Ordinal)
                ? new[] { p[0], p[0].Substring("line:".Length) } : new[] { p[0] }), StringComparer.Ordinal);
            foreach (var orphan in oldById.Values.Where(x => !parsedIdSet.Contains(x[0])))
            {
                Debug.LogWarning($"ID órfão preservado no CSV: {orphan[0]}");
                orphan[9] = "orphan"; merged.Add(orphan);
            }
            File.WriteAllText(MasterPath, Csv.Write(merged), new UTF8Encoding(true));
            WriteNative(Root + "/Localization/pt-BR.csv", merged, 7);
            WriteNative(Root + "/Localization/en-US.csv", merged, 8);
            AssetDatabase.Refresh();
            Validate();
        }

        [MenuItem("Freyja/Narrative/Validate demo CSV")]
        public static void Validate()
        {
            var rows = Csv.Read(File.ReadAllText(MasterPath));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            int errors = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Count != Columns.Length || string.IsNullOrWhiteSpace(row[0]) || !row[0].StartsWith("line:demo.", StringComparison.Ordinal))
                { Debug.LogError($"CSV linha {i + 1}: colunas/ID inválidos."); errors++; continue; }
                if (!ids.Add(row[0])) { Debug.LogError($"CSV linha {i + 1}: ID duplicado {row[0]}."); errors++; }
                if (!string.IsNullOrEmpty(row[8]) && !PlaceholdersMatch(row[7], row[8]))
                { Debug.LogError($"CSV linha {i + 1}: placeholders incompatíveis em {row[0]}."); errors++; }
            }
            Debug.Log(errors == 0 ? $"CSV narrativo válido: {ids.Count} IDs." : $"CSV narrativo: {errors} erros.");
        }

        public static string RoundTripCsvForTests(string source) => Csv.Write(Csv.Read(source));

        public static string MergeCsvForTests(string current, string previous)
        {
            var currentRows = Csv.Read(current);
            var previousRows = Csv.Read(previous);
            ValidateMasterSchema(currentRows);
            if (previousRows.Count > 0) ValidateMasterSchema(previousRows);
            var duplicates = DuplicateIds(previousRows);
            if (duplicates.Count > 0) throw new FormatException("IDs duplicados no CSV mestre: " + string.Join(", ", duplicates));
            var oldById = Index(previousRows);
            var merged = new List<List<string>> { Columns.ToList() };
            foreach (var source in currentRows.Skip(1))
            {
                if (source.Count != Columns.Length) throw new FormatException("Linha atual fora do esquema consolidado.");
                oldById.TryGetValue(source[0], out var previousRow);
                if (previousRow == null && source[0].StartsWith("line:", StringComparison.Ordinal))
                    oldById.TryGetValue(source[0].Substring(5), out previousRow);
                var translated = previousRow != null && previousRow.Count > 8 ? previousRow[8] : "";
                var unchangedStaleTranslation = previousRow != null && previousRow.Count > 9 && previousRow[9] == "stale"
                    && previousRow.Count > 8 && previousRow[8] == translated;
                var status = string.IsNullOrEmpty(translated) ? "missing"
                    : unchangedStaleTranslation ? "stale" : previousRow[6] == source[6] ? "translated" : "stale";
                if (!string.IsNullOrEmpty(translated) && !PlaceholdersMatch(source[7], translated)) status = "invalid_placeholders";
                var copy = source.ToList(); copy[8] = translated; copy[9] = status; merged.Add(copy);
            }
            var ids = new HashSet<string>(merged.Skip(1).Select(row => row[0]), StringComparer.Ordinal);
            foreach (var orphan in oldById.Values.Where(row => !ids.Contains(row[0])))
            { var copy = orphan.ToList(); copy[9] = "orphan"; merged.Add(copy); }
            return Csv.Write(merged);
        }

        private static List<List<string>> ParseSource(string source)
        {
            var result = new List<List<string>>();
            string node = "";
            foreach (var raw in source.Split('\n'))
            {
                var title = Regex.Match(raw, @"^title:\s*(\S+)");
                if (title.Success) { node = title.Groups[1].Value; continue; }
                var id = Regex.Match(raw, @"#line:([\w.-]+)");
                if (!id.Success) continue;
                var text = raw.Substring(0, id.Index).Trim();
                var kind = text.StartsWith("->", StringComparison.Ordinal) ? "option" : "line";
                if (kind == "option") text = text.Substring(2).Trim();
                string speaker = "";
                int colon = text.IndexOf(':');
                if (colon > 0) { speaker = text.Substring(0, colon).Trim(); text = text.Substring(colon + 1).Trim(); }
                var context = $"node={node};kind={kind}";
                var hashBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
                var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                result.Add(new List<string> { "line:" + id.Groups[1].Value, speaker, node, "Demo.yarn", kind, context, hash, text, "", "missing" });
            }
            return result;
        }
        private static Dictionary<string, List<string>> Index(List<List<string>> rows)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var row in rows.Skip(1)) if (row.Count > 0 && !string.IsNullOrEmpty(row[0])) result[row[0]] = row;
            return result;
        }
        private static List<string> DuplicateIds(List<List<string>> rows)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var duplicates = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows.Skip(1))
                if (row.Count > 0 && !string.IsNullOrEmpty(row[0]) && !seen.Add(row[0])) duplicates.Add(row[0]);
            return duplicates.ToList();
        }
        private static void ValidateMasterSchema(List<List<string>> rows)
        {
            if (rows.Count == 0 || !rows[0].SequenceEqual(Columns, StringComparer.Ordinal))
                throw new FormatException("Cabeçalho CSV mestre incompatível com o esquema narrativo.");
            for (int i = 1; i < rows.Count; i++)
                if (rows[i].Count != Columns.Length)
                    throw new FormatException($"Linha CSV {i + 1} tem {rows[i].Count} colunas; esperado {Columns.Length}.");
        }
        private static bool PlaceholdersMatch(string source, string translation)
        {
            var rx = new Regex(@"\{[A-Za-z_][A-Za-z0-9_]*\}");
            return rx.Matches(source).Select(x => x.Value).OrderBy(x => x)
                .SequenceEqual(rx.Matches(translation).Select(x => x.Value).OrderBy(x => x));
        }
        private static void WriteNative(string path, List<List<string>> rows, int textColumn)
        {
            var native = new List<List<string>> { new List<string> { "id", "text", "comment" } };
            foreach (var row in rows.Skip(1))
                native.Add(new List<string> { row[0], row[textColumn].Length == 0 ? row[7] : row[textColumn], $"{row[5]};speaker={row[1]};source_hash={row[6]}" });
            File.WriteAllText(path, Csv.Write(native), new UTF8Encoding(true));
        }

        private static class Csv
        {
            public static List<List<string>> Read(string source)
            {
                var rows = new List<List<string>>(); var row = new List<string>(); var field = new StringBuilder(); bool quoted = false;
                for (int i = 0; i < source.Length; i++)
                {
                    char c = source[i];
                    if (quoted)
                    {
                        if (c == '"' && i + 1 < source.Length && source[i + 1] == '"') { field.Append('"'); i++; }
                        else if (c == '"') quoted = false;
                        else field.Append(c);
                    }
                    else if (c == '"') quoted = true;
                    else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                    else if (c == '\n') { row.Add(field.ToString().TrimEnd('\r')); field.Clear(); if (rows.Count == 0 && row.Count > 0) row[0] = row[0].TrimStart('\uFEFF'); rows.Add(row); row = new List<string>(); }
                    else field.Append(c);
                }
                if (quoted) throw new FormatException("CSV contém aspas sem fechamento.");
                if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }
                return rows;
            }
            public static string Write(IEnumerable<List<string>> rows) => string.Join("\n", rows.Select(row => string.Join(",", row.Select(Escape)))) + "\n";
            private static string Escape(string value) => value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r')
                ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }
    }
}
#endif
