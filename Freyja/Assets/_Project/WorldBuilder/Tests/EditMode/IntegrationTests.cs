using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Freyja.WorldBuilder.Editor;
namespace Freyja.WorldBuilder.Tests
{
    public class IntegrationTests
    {
        WorldDefinition w;
        Scene scene,previous;
        string folder;
        [SetUp] public void Setup()
        {
            previous=SceneManager.GetActiveScene();
            scene=previous;
            folder="Assets/_Project/WorldBuilder/Demo/TestTemp";
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            w=ScriptableObject.CreateInstance<WorldDefinition>();
            w.worldId="qa-temp-021";
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.localPosition=new Vector3(0,1,0);
            go.transform.localScale=new Vector3(5,2,6);
            var prefab=ExternalModelNormalizer.Normalize(PrefabUtility.SaveAsPrefabAsset(go,folder+"/Source.prefab"),new Vector2(5,6),Vector3.zero,folder+"/House.prefab").normalizedPrefab;
            Object.DestroyImmediate(go);
            w.lots.Add(new LotDefinition
            {
                id="one",prefab=prefab
            }
            );
        }
        [TearDown] public void TearDown()
        {
            if(w)
            {
                var root=WorldGenerator.FindRoot(w);
                if(root)Object.DestroyImmediate(root);
            }
            Object.DestroyImmediate(w);
            if(!string.IsNullOrEmpty(folder))AssetDatabase.DeleteAsset(folder);
            if(AssetDatabase.IsValidFolder(WorldGenerator.Folder+"/Generated"))foreach(var guid in AssetDatabase.FindAssets("qa-temp-021",new[]
            {
                WorldGenerator.Folder+"/Generated"
            }
            ))AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        }
        [Test] public void RebuildPreservesManualAndIdentity()
        {
            var root=WorldGenerator.Apply(w);
            var lot=root.GetComponentInChildren<WorldInstanceId>();
            var manual=new GameObject("Manual");
            manual.transform.SetParent(root.transform);
            WorldGenerator.Apply(w);
            WorldGenerator.Apply(w);
            Assert.IsTrue(manual);
            Assert.AreEqual(1,System.Linq.Enumerable.Count(root.GetComponentsInChildren<WorldInstanceId>(),x=>x.kind=="house"));
            Assert.AreSame(root,WorldGenerator.FindRoot(w));
        }
        [Test] public void ReplacingHouseKeepsLot()
        {
            var root=WorldGenerator.Apply(w);
            var lot=System.Linq.Enumerable.First(root.GetComponentsInChildren<WorldInstanceId>(),x=>x.kind=="lot");
            var position=lot.transform.position;
            var copy=Object.Instantiate(w.lots[0].prefab);
            w.lots[0].prefab=PrefabUtility.SaveAsPrefabAsset(copy,folder+"/Replacement.prefab");
            Object.DestroyImmediate(copy);
            WorldGenerator.Apply(w);
            Assert.AreEqual(position,lot.transform.position);
            Assert.AreEqual(1,System.Linq.Enumerable.Count(root.GetComponentsInChildren<WorldInstanceId>(),x=>x.kind=="house"));
        }
        [Test] public void ConflictsBlockMutation()
        {
            w.lots.Add(new LotDefinition
            {
                id="two",prefab=w.lots[0].prefab
            }
            );
            Assert.Throws<System.InvalidOperationException>(()=>WorldGenerator.Apply(w));
            Assert.IsNull(WorldGenerator.FindRoot(w));
        }
        [Test] public void RemovingLotKeepsManualChild()
        {
            var root=WorldGenerator.Apply(w);
            var lot=System.Linq.Enumerable.First(root.GetComponentsInChildren<WorldInstanceId>(),x=>x.kind=="lot");
            var manual=new GameObject("Manual child");
            manual.transform.SetParent(lot.transform);
            w.lots.Clear();
            WorldGenerator.Apply(w);
            Assert.IsTrue(manual);
            Assert.AreEqual(root.transform,manual.transform.parent);
        }
        [Test] public void UndoRebuildRestoresPosition()
        {
            var root=WorldGenerator.Apply(w);
            var lot=System.Linq.Enumerable.First(root.GetComponentsInChildren<WorldInstanceId>(),x=>x.kind=="lot");
            var before=lot.transform.position;
            w.lots[0].center=new Vector2(20,20);
            WorldGenerator.Apply(w);
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Assert.That(Vector3.Distance(before,lot.transform.position),Is.LessThan(.001f));
        }
        [Test] public void NormalizerBottomCenter()
        {
            var d=AssetDatabase.LoadAssetAtPath<AssetDescriptor>(folder+"/House.asset");
            Assert.AreEqual(Vector3.one,d.normalizedPrefab.transform.localScale);
            Assert.AreEqual(0,d.finalBounds.min.y,.001f);
            Assert.AreEqual(0,d.finalBounds.center.x,.001f);
            Assert.AreEqual(5,d.finalBounds.size.x,.01f);
            Assert.AreEqual(6,d.finalBounds.size.z,.01f);
        }
    }
}
