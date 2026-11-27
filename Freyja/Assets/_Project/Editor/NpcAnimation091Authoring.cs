using System;
using System.Linq;
using Freyja.NpcAnimation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Freyja.Editor
{
    internal static class NpcAnimation091Authoring
    {
        private const string OutputDirectory = "Assets/_Project/Art/NpcAnimation091";
        private const string ProfilePath = "Assets/_Project/Resources/NpcAnimation091/SharedResidentProfile.asset";
        private const string ControllerPath = OutputDirectory + "/SharedResident.controller";
        private const string ClipDirectory = "Assets/ThirdParty/Kevin Iglesias/Human Basic Motions FREE/";
        private const string QuaterniusModelPath = "Assets/ThirdParty/Quaternius/Universal Animation Library/UAL1_Standard.fbx";

        [MenuItem("Freyja/NPC Animation 091/Create Shared Locomotion Assets")]
        private static void CreateSharedLocomotionAssets()
        {
            if (!PrepareQuaterniusImport()) return;
            EnsureFolder("Assets/_Project/Art", "NpcAnimation091");
            EnsureFolder("Assets/_Project", "Resources");
            EnsureFolder("Assets/_Project/Resources", "NpcAnimation091");

            AnimationClip idle = FindClip("HumanM@Idle01");
            AnimationClip walk = FindClip("HumanM@Walk01_Forward");
            AnimationClip run = FindClip("HumanM@Run01_Forward");
            AnimationClip sitDown = FindQuaterniusClip("Sitting_Enter");
            AnimationClip sitIdle = FindQuaterniusClip("Sitting_Idle_Loop");
            AnimationClip standUp = FindQuaterniusClip("Sitting_Exit");
            if (idle == null || walk == null || run == null || sitDown == null || sitIdle == null || standUp == null)
            {
                Debug.LogError("NPC Animation 091: a required Humanoid clip is missing. Expected Human Basic Motions idle/walk/run and Quaternius Sitting_Enter/Sitting_Idle_Loop/Sitting_Exit.");
                return;
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }
            controller.parameters = new[]
            {
                new AnimatorControllerParameter { name = "Speed", type = AnimatorControllerParameterType.Float }
            };

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState child in stateMachine.states)
                stateMachine.RemoveState(child.state);

            var tree = new BlendTree
            {
                name = "Resident Locomotion",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(idle, 0f);
            tree.AddChild(walk, 1.35f);
            tree.AddChild(run, 2.5f);
            AnimatorState locomotion = stateMachine.AddState("Locomotion", new Vector3(280f, 80f, 0f));
            locomotion.motion = tree;
            locomotion.writeDefaultValues = true;
            stateMachine.defaultState = locomotion;
            AddActionState(stateMachine, "Sitting_Enter", sitDown, new Vector3(560f, 0f, 0f));
            AddActionState(stateMachine, "Sitting_Idle_Loop", sitIdle, new Vector3(560f, 160f, 0f));
            AddActionState(stateMachine, "Sitting_Exit", standUp, new Vector3(560f, 320f, 0f));
            EditorUtility.SetDirty(controller);

            NpcAnimationProfile profile = AssetDatabase.LoadAssetAtPath<NpcAnimationProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<NpcAnimationProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }
            var serialized = new SerializedObject(profile);
            serialized.FindProperty("motionController").objectReferenceValue = controller;
            serialized.FindProperty("idleClip").objectReferenceValue = idle;
            serialized.FindProperty("walkClip").objectReferenceValue = walk;
            serialized.FindProperty("runClip").objectReferenceValue = run;
            serialized.FindProperty("locomotionSpeedParameter").stringValue = "Speed";
            serialized.FindProperty("stopThreshold").floatValue = 0.05f;
            serialized.FindProperty("walkThreshold").floatValue = 1.35f;
            serialized.FindProperty("runThreshold").floatValue = 2.5f;
            serialized.FindProperty("dampingSeconds").floatValue = 0.12f;
            serialized.FindProperty("cancelStateFullPath").stringValue = "Base Layer.Locomotion";
            SerializedProperty actions = serialized.FindProperty("actions");
            actions.arraySize = 3;
            SetAction(actions.GetArrayElementAtIndex(0), "sit_down", "Base Layer.Sitting_Enter", NpcAnimationActionMode.Finite, 5f);
            SetAction(actions.GetArrayElementAtIndex(1), "sit_idle", "Base Layer.Sitting_Idle_Loop", NpcAnimationActionMode.Continuous, 8f);
            SetAction(actions.GetArrayElementAtIndex(2), "stand_up", "Base Layer.Sitting_Exit", NpcAnimationActionMode.Finite, 5f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("NPC Animation 091: shared Humanoid locomotion and Quaternius sitting lifecycle assets created.", profile);
            Selection.activeObject = profile;
        }

        private static bool PrepareQuaterniusImport()
        {
            ModelImporter importer = AssetImporter.GetAtPath(QuaterniusModelPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError("NPC Animation 091: Quaternius FBX is missing at " + QuaterniusModelPath + ".");
                return false;
            }

            bool changed = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                changed = true;
            }
            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                changed = true;
            }
            if (!importer.importAnimation)
            {
                importer.importAnimation = true;
                changed = true;
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
            bool sittingIdleFound = false;
            for (int i = 0; clips != null && i < clips.Length; i++)
            {
                if (!HasAnimationSuffix(clips[i].name, "Sitting_Idle_Loop")) continue;
                sittingIdleFound = true;
                if (!clips[i].loopTime)
                {
                    clips[i].loopTime = true;
                    clips[i].loopPose = true;
                    changed = true;
                }
            }
            if (!sittingIdleFound)
            {
                Debug.LogError("NPC Animation 091: Quaternius FBX did not expose Sitting_Idle_Loop as an animation take. Found: " +
                    String.Join(", ", (clips ?? Array.Empty<ModelImporterClipAnimation>()).Select(item => item.name)));
                return false;
            }

            if (changed)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }

            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(QuaterniusModelPath).OfType<Avatar>()
                .FirstOrDefault(item => item != null && item.isValid && item.isHuman);
            if (avatar == null)
            {
                Debug.LogError("NPC Animation 091: Quaternius FBX did not produce a valid Humanoid Avatar. Sitting bindings were not created.");
                return false;
            }
            return true;
        }

        private static void AddActionState(AnimatorStateMachine stateMachine, string stateName, AnimationClip clip, Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            state.motion = clip;
            state.writeDefaultValues = true;
        }

        private static void SetAction(SerializedProperty property, string semanticId, string statePath,
            NpcAnimationActionMode mode, float timeoutSeconds)
        {
            property.FindPropertyRelative("semanticId").stringValue = semanticId;
            property.FindPropertyRelative("stateFullPath").stringValue = statePath;
            property.FindPropertyRelative("layer").intValue = 0;
            property.FindPropertyRelative("mode").enumValueIndex = (int)mode;
            property.FindPropertyRelative("transitionSeconds").floatValue = 0.12f;
            property.FindPropertyRelative("timeoutSeconds").floatValue = timeoutSeconds;
        }

        private static AnimationClip FindClip(string name)
        {
            foreach (string path in AssetDatabase.FindAssets(name + " t:Model", new[] { ClipDirectory })
                         .Select(AssetDatabase.GUIDToAssetPath))
            {
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                    .FirstOrDefault(item => item != null && item.name == name);
                if (clip != null) return clip;
            }
            return null;
        }

        private static AnimationClip FindQuaterniusClip(string name)
        {
            return AssetDatabase.LoadAllAssetsAtPath(QuaterniusModelPath).OfType<AnimationClip>()
                .Where(item => item != null && !item.name.StartsWith("__preview__", StringComparison.Ordinal))
                .FirstOrDefault(item => HasAnimationSuffix(item.name, name));
        }

        private static bool HasAnimationSuffix(string candidate, string animationName)
        {
            return String.Equals(candidate, animationName, StringComparison.Ordinal) ||
                   candidate != null && candidate.EndsWith("|" + animationName, StringComparison.Ordinal);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
