using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Freyja.WorldBuilder.Tests
{
    public class WorldSmokeTests
    {
        [UnityTest] public IEnumerator TerrainSupportsCharacter()
        {
            var td=new TerrainData
            {
                heightmapResolution=33,size=new Vector3(20,4,20)
            };
            var h=new float[33,33];
            for(int z=0;z<33;z++)for(int x=0;x<33;x++)h[z,x]=.25f;
            td.SetHeights(0,0,h);
            var terrain=Terrain.CreateTerrainGameObject(td);
            var go=new GameObject("QA Character");
            var cc=go.AddComponent<CharacterController>();
            cc.height=1.8f;
            cc.center=new Vector3(0,.9f,0);
            cc.enabled=false;
            go.transform.position=new Vector3(5,2,5);
            cc.enabled=true;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            for(int i=0;i<30;i++)
            {
                cc.Move(new Vector3(.01f,-.1f,0));
                yield return null;
            }
            Assert.That(go.transform.position.y,Is.InRange(.9f,1.2f));
            Assert.Greater(go.transform.position.x,5.1f);
            Object.Destroy(go);
            Object.Destroy(terrain);
            Object.Destroy(td);
        }
    }
}
