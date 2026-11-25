using System.Linq;
using Freyja.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Freyja.EditorTools
{
    public static class IslandDialogueSetup
    {
        private const string SceneName = "Ilha_Principal";
        private const string NunoPrefabPath = "Assets/_Project/Prefabs/Moradores060/NunoPires.prefab";

        [MenuItem("Freyja/Diálogos/Instalar piloto")]
        private static void InstallFromMenu() => Debug.Log(Install());

        public static string Install()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != SceneName || EditorApplication.isPlayingOrWillChangePlaymode)
                return "Abra a cena principal em Edit Mode.";

            var roots = scene.GetRootGameObjects();
            var session = roots.SelectMany(root => root.GetComponentsInChildren<CaisSessionController>(true))
                .SingleOrDefault();
            var player = session == null ? null : new SerializedObject(session)
                .FindProperty("player").objectReferenceValue as CaisPlayerController;
            var transforms = roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var luana = transforms.SingleOrDefault(item => item.name == "Luana Batista");
            var ivo = transforms.SingleOrDefault(item => item.name == "Ivo Batista");
            if (player == null || luana == null || ivo == null)
                return "A cena precisa do jogador e dos moradores Ivo e Luana da entrega 060.";

            var provisional = transforms.SingleOrDefault(item => item.name == "Nuno Pires (provisório)");
            var nuno = transforms.SingleOrDefault(item => item.name == "Nuno Pires");
            if (nuno == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NunoPrefabPath);
                var cast = ivo.parent;
                if (prefab == null || cast == null)
                    return "O prefab de Nuno e o grupo dos moradores precisam estar disponíveis.";
                if (!Physics.Raycast(new Vector3(-32.5f, 5f, -41.5f), Vector3.down, out var ground, 6f,
                    ~0, QueryTriggerInteraction.Ignore))
                    return "Piso do cais indisponível para Nuno.";

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, cast);
                Undo.RegisterCreatedObjectUndo(instance, "Posicionar Nuno Pires");
                instance.name = "Nuno Pires";
                nuno = instance.transform;
                nuno.SetPositionAndRotation(ground.point + Vector3.up * 0.015f,
                    Quaternion.Euler(0f, 180f, 0f));
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
                PrefabUtility.RecordPrefabInstancePropertyModifications(nuno);
            }

            var dialogue = session.GetComponent<IslandDialogue>() ??
                Undo.AddComponent<IslandDialogue>(session.gameObject);
            var binding = new SerializedObject(dialogue);
            binding.FindProperty("session").objectReferenceValue = session;
            binding.FindProperty("player").objectReferenceValue = player.transform;
            binding.FindProperty("nuno").objectReferenceValue = nuno;
            binding.FindProperty("luana").objectReferenceValue = luana;
            binding.FindProperty("ivo").objectReferenceValue = ivo;
            binding.ApplyModifiedProperties();
            if (provisional != null)
                Undo.DestroyObjectImmediate(provisional.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            return "Piloto configurado na cena principal.";
        }
    }
}
