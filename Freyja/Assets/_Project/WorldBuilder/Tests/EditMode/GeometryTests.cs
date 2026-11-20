using NUnit.Framework;
using UnityEngine;
namespace Freyja.WorldBuilder.Tests
{
    public class GeometryTests
    {
        [TestCase(-1.5f,-2f)][TestCase(1.5f,2f)][TestCase(-.4f,0f)] public void SnapSigned(float x,float expected)=>Assert.AreEqual(expected,GridMath.Snap(x,1));
        [Test] public void HalfGrid()=>Assert.AreEqual(-1.5f,GridMath.Snap(-1.3f,.5f));
        [Test] public void Coordinates()
        {
            var w=ScriptableObject.CreateInstance<WorldDefinition>();
            Assert.AreEqual(Vector2.zero,TerrainSpace.UV(w,new Vector2(-80,-80)));
            Assert.AreEqual(Vector2.one,TerrainSpace.UV(w,new Vector2(80,80)));
            Assert.AreEqual(.125f,TerrainSpace.Normalized(w,0));
            Assert.AreEqual(0,TerrainSpace.WorldY(w,.125f));
            Object.DestroyImmediate(w);
        }
        [Test] public void Rectangles()
        {
            var a=new OrientedRectXZ(Vector2.zero,new Vector2(12,14),0);
            Assert.IsFalse(a.Overlaps(new OrientedRectXZ(new Vector2(12,0),new Vector2(12,14),0)));
            Assert.IsTrue(a.Overlaps(new OrientedRectXZ(new Vector2(8,0),new Vector2(12,14),45)));
            Assert.IsFalse(a.Overlaps(new OrientedRectXZ(new Vector2(30,0),new Vector2(12,14),90)));
        }
        [TestCase(0)][TestCase(45)][TestCase(90)] public void StampFlatAndLocal(float yaw)
        {
            var w=ScriptableObject.CreateInstance<WorldDefinition>();
            var l=new LotDefinition
            {
                yaw=yaw,center=Vector2.zero,manualHeight=true,foundationY=3
            };
            w.lots.Add(l);
            var h=TerrainMath.Generate(w);
            foreach(var q in new[]
            {
                new Vector2(-4,-5),new Vector2(4,5),Vector2.zero
            }
            )
            {
                var v=l.Rect.LocalToWorld(q);
                Assert.Less(Mathf.Abs(TerrainMath.Sample(w,h,v)-3),.05f);
            }
            var outside=new Vector2(50,50);
            Assert.AreEqual(TerrainMath.BaseHeight(w,outside),TerrainMath.Sample(w,h,outside),.02f);
            Object.DestroyImmediate(w);
        }
        [Test] public void Determinism()
        {
            var w=ScriptableObject.CreateInstance<WorldDefinition>();
            var a=TerrainMath.Generate(w);
            var b=TerrainMath.Generate(w);
            Assert.AreEqual(a,b);
            w.seed++;
            var c=TerrainMath.Generate(w);
            Assert.AreNotEqual(a[80,70],c[80,70]);
            Object.DestroyImmediate(w);
        }
    }
}
