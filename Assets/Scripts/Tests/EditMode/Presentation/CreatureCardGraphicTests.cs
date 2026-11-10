using System.Linq;
using Game.Core.Cards;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class CreatureCardGraphicTests
    {
        private CardView _view;
        private CardFaceBuilder _faces;
        private CardSkin _skin;
        [SetUp]
        public void SetUp()
        {
            var visual=AssetDatabase.LoadAssetAtPath<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
            Assert.IsNotNull(visual.CreatureStyle.Frame);
            _faces=new CardFaceBuilder(AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset"),visual,
                AssetDatabase.LoadAssetAtPath<UiTexts>("Assets/Data/Config/UiTexts.asset"));
            _skin=new CardSkin(visual);
            _view=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab")).GetComponent<CardView>();
        }
        [TearDown]
        public void TearDown(){if(_view!=null)Object.DestroyImmediate(_view.gameObject);}
        [Test]
        public void EveryElement_HasDistinctFrame_AllCreaturesUseTheirElementFrame()
        {
            var visual=AssetDatabase.LoadAssetAtPath<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
            var elements=(Element[])System.Enum.GetValues(typeof(Element));
            var frames=elements.Select(visual.CreatureFrameOf).ToArray();
            Assert.IsTrue(frames.All(f=>f!=null));Assert.AreEqual(6,frames.Distinct().Count());
            var database=AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset");
            foreach(var card in database.Cards.OfType<CreatureDef>())
            {
                var face=_faces.Build(card.Id);
                Assert.AreSame(visual.CreatureFrameOf(card.Element),face.CreatureFrame,card.Id);
            }
            Assert.IsNull(_faces.Build("ability.heat").CreatureFrame);
        }
        [Test]
        public void DisplayedCreatureFrame_ChangesWhenElementChanges()
        {
            var database=AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset");
            foreach(var card in database.Cards.OfType<CreatureDef>().GroupBy(c=>c.Element).Select(g=>g.First()))
            {
                var face=_faces.Build(card.Id);
                Object.DestroyImmediate(_view.gameObject);
                _view=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab")).GetComponent<CardView>();
                _view.Bind(1,card.Id,face,_skin);
                var graphic=_view.GetComponentInChildren<CreatureCardFaceView>();
                Assert.AreSame(face.CreatureFrame,graphic.transform.Find("Frame").GetComponent<UnityEngine.UI.Image>().sprite);
            }
        }

        [Test]
        public void CreaturePower_IsLive_RestoresBase_AndSurvivesDimming()
        {
            _view.Bind(1,"creature.stone-beetle",_faces.Build("creature.stone-beetle"),_skin);
            var face=_view.GetComponentInChildren<CreatureCardFaceView>();
            Assert.IsNotNull(face);Assert.AreEqual("135",face.PowerText.text);
            _view.SetPower(295);Assert.AreEqual("295",face.PowerText.text);
            _view.SetDimming(.5f);Assert.AreEqual("295",face.PowerText.text);
            _view.SetPower(null);Assert.AreEqual("135",face.PowerText.text);
            Assert.Less(face.PowerText.rectTransform.anchorMin.x,.4f);
            Assert.Greater(face.PowerText.rectTransform.anchorMin.y,.7f);
        }
        [Test]
        public void RebindToHiddenCard_RemovesCreatureFace_WithoutExposingLabels()
        {
            _view.Bind(1,"creature.stone-beetle",_faces.Build("creature.stone-beetle"),_skin);
            var face=_view.GetComponentInChildren<CreatureCardFaceView>();
            _view.Bind(1,null,_faces.Build(null),_skin);
            Assert.IsFalse(face.gameObject.activeSelf);Assert.IsFalse(_view.IsKnown);
        }
        [Test]
        public void RebindToAbility_UsesLegacyFace_WithoutCreatureOverlay()
        {
            _view.Bind(1,"creature.stone-beetle",_faces.Build("creature.stone-beetle"),_skin);
            var face=_view.GetComponentInChildren<CreatureCardFaceView>();
            _view.Bind(2,"ability.heat",_faces.Build("ability.heat"),_skin);
            _view.SetDimming(.5f);
            Assert.IsFalse(face.gameObject.activeSelf);
            Assert.IsNull(_view.GetComponentInChildren<CreatureCardFaceView>());
        }
    }
}
