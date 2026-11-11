using Game.Data.Presentation;
using Game.Presentation.Cards;
using Game.Presentation.Table;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Game.Presentation.Tests
{
    public sealed class CardFeedbackTests
    {
        [TestCase(5)]
        [TestCase(9)]
        public void HandFan_OuterEdgesAndCentresPointDown(int count)
        {
            var go=new GameObject("FanTest");var cameraGo=new GameObject("Camera");
            try{
                var settings=AssetDatabase.LoadAssetAtPath<TablePresentationSettings>("Assets/Data/Config/TablePresentation.asset");
                var layout=go.AddComponent<TableLayout>();layout.Configure(settings,cameraGo.AddComponent<Camera>());
                var left=layout.Hand(TableSide.Bottom,0,count,true);var right=layout.Hand(TableSide.Bottom,count-1,count,true);var centre=layout.Hand(TableSide.Bottom,count/2,count,true);
                var up=Quaternion.Euler(settings.CameraPitch,0,0)*Vector3.up;
                Assert.Greater(Vector3.Dot(left.Rotation*Vector3.right,up),0,"Left outer edge must descend");
                Assert.Less(Vector3.Dot(right.Rotation*Vector3.right,up),0,"Right outer edge must descend");
                Assert.Less(Vector3.Dot(left.Position-centre.Position,up),0);
                Assert.Less(Vector3.Dot(right.Position-centre.Position,up),0);
            }finally{Object.DestroyImmediate(go);Object.DestroyImmediate(cameraGo);}
        }
        [Test]
        public void Highlight_ReusesRendererAndNeverAddsColliders()
        {
            var go=new GameObject("GlowTest");
            try{
                var settings=AssetDatabase.LoadAssetAtPath<CardHighlightSettings>("Assets/Data/Config/CardHighlight.asset");
                Assert.NotNull(settings.Material);Assert.IsTrue(settings.Material.shader.isSupported);
                var glow=go.AddComponent<CardPlayableGlow>();glow.Configure(settings);var line=go.GetComponent<LineRenderer>();
                for(var i=0;i<10;i++){glow.SetColor(Color.cyan);Assert.IsTrue(line.enabled);glow.SetColor(null);Assert.IsFalse(line.enabled);}
                Assert.AreEqual(1,go.GetComponents<LineRenderer>().Length);Assert.AreEqual(0,go.GetComponents<Collider>().Length);
                Assert.Less(line.widthMultiplier,.1f,"Contour must not become an opaque card-sized panel");
            }finally{Object.DestroyImmediate(go);}
        }
    }
}
