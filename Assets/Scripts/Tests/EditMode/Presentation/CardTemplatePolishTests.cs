using System;
using System.Linq;
using Game.Core.Cards;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    public sealed class CardTemplatePolishTests
    {
        private VisualSettings _visual;
        private CardDatabase _database;
        private CardSkin _skin;
        private CardFaceBuilder _faces;
        private GameObject _prefab;

        [SetUp]
        public void LoadCardAssets()
        {
            _visual = AssetDatabase.LoadAssetAtPath<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
            _database = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset");
            _skin = new CardSkin(_visual);
            _faces = new CardFaceBuilder(_database, _visual,
                AssetDatabase.LoadAssetAtPath<UiTexts>("Assets/Data/Config/UiTexts.asset"));
            _prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab");
        }

        [Test]
        public void StylesRetainSpritesAndKeepInformationWithinTheCard()
        {
            var creature = _skin.CreatureStyle;
            var field = _skin.FieldStyle;
            var ability = _skin.AbilityStyle;
            var elements = (Element[])Enum.GetValues(typeof(Element));
            Assert.That(elements.Select(_visual.CreatureFrameOf).Distinct().Count(), Is.EqualTo(6));
            Assert.That(elements.Select(_visual.IconOf).Distinct().Count(), Is.EqualTo(6));
            foreach (var sprite in elements.Select(_visual.CreatureFrameOf).Concat(elements.Select(_visual.IconOf))
                         .Concat(new[] { creature.Frame, field.Frame, field.ClosedIcon, field.LockedIcon, ability.Frame,
                             ability.BonusPanel, _visual.CardBackArt }))
                Assert.That(sprite, Is.Not.Null);
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
                Assert.That(_visual.StyleOf(rarity).Icon, Is.Not.Null, rarity.ToString());

            AssertInside(creature.Art, creature.Power, creature.ElementIcon, creature.Element, creature.Title,
                creature.Type, creature.Rarity, creature.Rules);
            AssertInside(field.Art, field.Title, field.BonusRail, field.ConditionTitle, field.Rules,
                field.Type, field.Rarity, field.StateBadge);
            AssertInside(ability.Art, ability.Title, ability.Bonus, ability.Timing, ability.Rules, ability.Type, ability.Rarity,
                ability.RarityIcon);
            Assert.That(field.Art.Overlaps(field.BonusRail), Is.False);
            Assert.That(field.BonusRail.Overlaps(field.ConditionTitle), Is.False);
            Assert.That(field.ConditionTitle.Overlaps(field.Rules), Is.False);
            Assert.That(ability.Bonus.Overlaps(ability.Timing), Is.False);
            Assert.That(ability.Timing.Overlaps(ability.Rules), Is.False);
        }

        [Test]
        public void FullCatalogFitsItsLabelsAndShowsTheCorrectRarityFinish()
        {
            var card = Object.Instantiate(_prefab).GetComponent<CardView>();
            try
            {
                foreach (var definition in _database.Cards)
                {
                    var face = _faces.Build(definition.Id);
                    card.Bind(1, definition.Id, face, _skin);
                    var view = card.GetComponentInChildren<CardFaceView>();
                    Assert.That(view, Is.Not.Null, definition.Id);
                    Assert.That(view.transform.Find("RarityFinish/RarityGem"), Is.Not.Null, definition.Id);
                    Assert.That(view.transform.Find("RarityFinish/MetalTrim") != null,
                        Is.EqualTo(definition.Rarity != Rarity.Common), definition.Id);
                    Assert.That(view.transform.Find("RarityFinish/SecondGem") != null,
                        Is.EqualTo(definition.Rarity == Rarity.Legendary), definition.Id);
                    Canvas.ForceUpdateCanvases();
                    foreach (var label in view.GetComponentsInChildren<TMP_Text>())
                    {
                        label.ForceMeshUpdate();
                        var context = $"{definition.Id} / {label.name}: {label.text}";
                        Assert.That(label.fontSizeMin, Is.GreaterThan(1f), context);
                        Assert.That(label.isTextTruncated, Is.False, context);
                        Assert.That(label.textBounds.size.x, Is.LessThanOrEqualTo(label.rectTransform.rect.width + 2), context);
                        Assert.That(label.textBounds.size.y, Is.LessThanOrEqualTo(label.rectTransform.rect.height + 2), context);
                    }
                }
            }
            finally { Object.DestroyImmediate(card.gameObject); }
        }

        [Test]
        public void PowerStatusAndOptionalPanelsResetOnRebind()
        {
            var card = Object.Instantiate(_prefab).GetComponent<CardView>();
            try
            {
                void Bind(string id) => card.Bind(1, id, _faces.Build(id), _skin);
                Bind("creature.stone-beetle");
                var creature = card.GetComponentInChildren<CreatureCardFaceView>();
                card.SetPower(287);
                Assert.That(creature.PowerText.text, Is.EqualTo("287"));
                Bind("creature.stone-beetle");
                Assert.That(creature.PowerText.text, Is.EqualTo("135"));

                Bind("field.sacrificial-altar");
                var field = card.GetComponentInChildren<FieldCardFaceView>();
                var badge = field.transform.Find("FieldState").GetComponent<UnityEngine.UI.Image>();
                card.SetFieldState(true, false);
                card.SetDimming(.5f);
                Assert.That(badge.sprite, Is.SameAs(_skin.FieldStyle.ClosedIcon));
                card.SetFieldState(true, true);
                Assert.That(badge.sprite, Is.SameAs(_skin.FieldStyle.LockedIcon));
                field.SetCancelled(true);
                Assert.That(field.transform.Find("Rules").GetComponent<TMP_Text>().fontStyle.HasFlag(FontStyles.Strikethrough), Is.True);
                card.SetFieldState(false, false);
                Assert.That(badge.gameObject.activeSelf, Is.False);
                Bind("field.warm-hearth");
                Assert.That(field.transform.Find("Rules"), Is.Null);
                Assert.That(field.transform.Find("FieldState").gameObject.activeSelf, Is.False);
                Assert.That(field.transform.Find("Bonuses").childCount, Is.EqualTo(2));

                Bind("ability.surge");
                var ability = card.GetComponentInChildren<AbilityCardFaceView>();
                Assert.That(ability.transform.Find("Bonus/Value").GetComponent<TMP_Text>().text, Is.EqualTo("+30"));
                Bind("ability.maneuver");
                Assert.That(ability.transform.Find("Bonus"), Is.Null);
                Assert.That(ability.transform.Find("Timing").GetComponent<TMP_Text>().text, Is.Not.Empty);
            }
            finally { Object.DestroyImmediate(card.gameObject); }
        }

        [Test]
        public void CollectionSizedCardScalesFontLimitsWhenEnlarged()
        {
            var root = new GameObject("CardBounds", typeof(RectTransform), typeof(Canvas));
            try
            {
                root.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                var rect = (RectTransform)root.transform;
                rect.sizeDelta = new Vector2(140, 200);
                var font = _prefab.GetComponentInChildren<TMP_Text>(true).font;
                var face = AbilityCardFaceView.Create(rect);
                face.Bind(_faces.Build("ability.retreat"), _skin, font);
                var rules = face.transform.Find("Rules").GetComponent<TMP_Text>();
                Canvas.ForceUpdateCanvases();
                var small = rules.fontSizeMin;
                rect.sizeDelta *= 3;
                ((RectTransform)face.transform).ForceUpdateRectTransforms();
                Canvas.ForceUpdateCanvases();
                Assert.That(((RectTransform)face.transform).rect.height, Is.EqualTo(600f).Within(.05f));
                rules.ForceMeshUpdate();
                Assert.That(rules.fontSizeMin, Is.EqualTo(small * 3).Within(.05f));
                Assert.That(rules.textBounds.size.y, Is.LessThanOrEqualTo(rules.rectTransform.rect.height + 2));
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void AssertInside(params Rect[] areas)
        {
            foreach (var area in areas)
            {
                Assert.That(area.width, Is.GreaterThan(0), area.ToString());
                Assert.That(area.height, Is.GreaterThan(0), area.ToString());
                Assert.That(area.xMin, Is.GreaterThanOrEqualTo(0), area.ToString());
                Assert.That(area.yMin, Is.GreaterThanOrEqualTo(0), area.ToString());
                Assert.That(area.xMax, Is.LessThanOrEqualTo(1), area.ToString());
                Assert.That(area.yMax, Is.LessThanOrEqualTo(1), area.ToString());
            }
        }
    }
}
