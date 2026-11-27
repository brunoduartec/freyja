#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Freyja.Editor
{
    internal static class NpcAnimation091AssetAudit
    {
        private const string OutputPath = "../../specs/091-npc-animation-system/asset-inventory.txt";

        [MenuItem("Freyja/NPC Animation/Export Asset Inventory")]
        private static void Export()
        {
            var report = new StringBuilder();
            report.AppendLine("Unity Animation Asset Inventory");
            report.AppendLine("Generated UTC: " + DateTime.UtcNow.ToString("O"));
            report.AppendLine("Unity: " + Application.unityVersion);

            report.AppendLine("\n## Resident prefabs");
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/Moradores060" });
            foreach (string guid in prefabs.OrderBy(x => AssetDatabase.GUIDToAssetPath(x), StringComparer.Ordinal))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                report.AppendLine($"- {path}");
                try
                {
                    GameObject contents = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        foreach (Animator animator in contents.GetComponentsInChildren<Animator>(true))
                        {
                            report.AppendLine($"  animator={Hierarchy(animator.transform)} enabled={animator.enabled} rootMotion={animator.applyRootMotion} avatar={(animator.avatar == null ? "none" : animator.avatar.name + ":valid=" + animator.avatar.isValid + ":human=" + animator.avatar.isHuman)} controller={(animator.runtimeAnimatorController == null ? "none" : AssetDatabase.GetAssetPath(animator.runtimeAnimatorController))}");
                        }
                    }
                    finally { PrefabUtility.UnloadPrefabContents(contents); }
                }
                catch (Exception exception) { report.AppendLine("  prefab-read-error=" + exception.GetType().Name + ":" + exception.Message); }
            }

            report.AppendLine("\n## Animator Controllers");
            foreach (string guid in AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/_Project/Art/Moradores060", "Assets/ThirdParty" })
                         .OrderBy(x => AssetDatabase.GUIDToAssetPath(x), StringComparer.Ordinal))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (controller == null) continue;
                report.AppendLine("- " + path);
                report.AppendLine("  params=" + String.Join(", ", controller.parameters.Select(x => x.name + ":" + x.type)));
                foreach (AnimatorControllerLayer layer in controller.layers)
                    AppendStates(report, layer.stateMachine, "  " + layer.name + "/");
            }

            report.AppendLine("\n## Animation clips (seat/stand, locomotion or idle names)");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (AnimationClip clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())
                {
                    string token = (clip.name + " " + path).ToLowerInvariant();
                    if (!(token.Contains("sit") || token.Contains("seat") || token.Contains("stand") || token.Contains("idle") || token.Contains("walk") || token.Contains("run"))) continue;
                    string key = path + "|" + clip.name;
                    if (!seen.Add(key)) continue;
                    ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    string rig = importer == null ? "n/a" : importer.animationType.ToString();
                    report.AppendLine($"- {path} :: {clip.name} duration={clip.length:F3}s loop={clip.isLooping} importRig={rig}");
                }
            }

            string destination = Path.GetFullPath(Path.Combine(Application.dataPath, OutputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.WriteAllText(destination, report.ToString());
            Debug.Log("[NpcAnimation091] Asset inventory written to " + destination);
        }

        private static void AppendStates(StringBuilder report, AnimatorStateMachine stateMachine, string prefix)
        {
            foreach (ChildAnimatorState child in stateMachine.states)
                report.AppendLine($"  state={prefix}{child.state.name} motion={(child.state.motion == null ? "none" : child.state.motion.name)}");
            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
                AppendStates(report, child.stateMachine, prefix + child.stateMachine.name + "/");
        }

        private static string Hierarchy(Transform transform)
        {
            string value = transform.name;
            while (transform.parent != null) { transform = transform.parent; value = transform.name + "/" + value; }
            return value;
        }
    }
}
#endif
