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
