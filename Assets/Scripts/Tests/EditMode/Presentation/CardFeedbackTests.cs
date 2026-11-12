using Game.Data.Presentation;
using Game.Presentation.Cards;
using Game.Presentation.Table;
using Game.Presentation.Interaction;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Game.Presentation.Tests
{
    public sealed class CardFeedbackTests
    {
        [TestCase(160)]
        [TestCase(800)]
        public void FourFieldBonuses_ShowEverySignedNumberAtCardAndPreviewSize(int height)
        {
            var canvas = new GameObject("BonusCanvas", typeof(RectTransform), typeof(Canvas));
            var holder = new GameObject("BonusCard", typeof(RectTransform));
            try
            {
                holder.transform.SetParent(canvas.transform, false);
                ((RectTransform)holder.transform).sizeDelta = new Vector2(height * .7f, height);
                var visual = AssetDatabase.LoadAssetAtPath<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
                var database = AssetDatabase.LoadAssetAtPath<Game.Data.Cards.CardDatabase>("Assets/Data/CardDatabase.asset");
                var texts = AssetDatabase.LoadAssetAtPath<UiTexts>("Assets/Data/Config/UiTexts.asset");
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab");
                var font = prefab.GetComponentInChildren<TMPro.TMP_Text>(true).font;
                var face = new CardFaceBuilder(database, visual, texts).Build("field.sacrificial-altar");
                var view = FieldCardFaceView.Create(holder.transform);
                view.Bind(face, new CardSkin(visual), font);
                Canvas.ForceUpdateCanvases();
                for (var i = 0; i < face.FieldBonuses.Count; i++)
                {
                    var panel = view.transform.Find("Bonuses/Bonus" + i);
                    var number = panel.Find("Value").GetComponent<TMPro.TMP_Text>();
                    var element = panel.Find("Element").GetComponent<TMPro.TMP_Text>();
                    number.ForceMeshUpdate(); element.ForceMeshUpdate();
                    var value = face.FieldBonuses[i].Value;
                    Assert.AreEqual((value > 0 ? "+" : "") + value, number.text);
                    Assert.IsFalse(number.isTextTruncated, number.text);
                    Assert.IsFalse(element.isTextTruncated, element.text);
                    Assert.Greater(number.rectTransform.rect.height, 0);
                }
            }
            finally { Object.DestroyImmediate(holder); Object.DestroyImmediate(canvas); }
        }

        [Test]
        public void FieldFaces_KeepEveryCatalogModifier_AndHideUnknownCardData()
        {
            var visual = AssetDatabase.LoadAssetAtPath<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
            var database = AssetDatabase.LoadAssetAtPath<Game.Data.Cards.CardDatabase>("Assets/Data/CardDatabase.asset");
            var texts = AssetDatabase.LoadAssetAtPath<UiTexts>("Assets/Data/Config/UiTexts.asset");
            var builder = new CardFaceBuilder(database, visual, texts);
            var fields = 0;
            foreach (var definition in database.Cards)
            {
                if (!(definition is Game.Data.Cards.FieldDef field)) continue;
                fields++;
                var face = builder.Build(field.Id);
                Assert.IsTrue(face.IsField, field.Id);
                Assert.AreEqual(field.Modifiers.Count, face.FieldBonuses.Count, field.Id);
                for (var i = 0; i < field.Modifiers.Count; i++)
                {
                    Assert.AreEqual(field.Modifiers[i].Value, face.FieldBonuses[i].Value, field.Id);
                    Assert.AreEqual(builder.ElementName(field.Modifiers[i].Element), face.FieldBonuses[i].Element, field.Id);
                    Assert.AreSame(visual.IconOf(field.Modifiers[i].Element), face.FieldBonuses[i].Icon, field.Id);
                }
            }
            Assert.Greater(fields, 0);
            Assert.IsFalse(builder.Build(null).IsField, "A hidden field must not reveal its elements or bonuses.");
            Assert.IsNull(builder.Build(null).FieldBonuses);
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void PreviewRotation_DragDoesNotClick_AndReleaseReturnsToFront(int pointerId)
        {
            var activeBefore = DOTween.TotalActiveTweens();
            var canvasGo = new GameObject("PreviewCanvas", typeof(RectTransform), typeof(Canvas));
            var cardGo = new GameObject("PreviewCard", typeof(RectTransform));
            var rotation = cardGo.AddComponent<Game.Presentation.Menu.CardPreviewRotation>();
            try
            {
                cardGo.transform.SetParent(canvasGo.transform, false);
                var rect = (RectTransform)cardGo.transform; rect.sizeDelta = new Vector2(400, 600);
                var settings = AssetDatabase.LoadAssetAtPath<CollectionSettings>("Assets/Data/Config/Collection.asset");
                var clicks = 0;
                rotation.Configure(rect, settings, () => clicks++); rotation.Interactable = true;
                var data = new UnityEngine.EventSystems.PointerEventData(null) { pointerId = pointerId, position = Vector2.zero };
                rotation.OnPointerDown(data); rotation.OnBeginDrag(data);
                data.position = new Vector2(2000, 2000); rotation.OnDrag(data);
                Assert.Greater(Quaternion.Angle(Quaternion.identity, rect.localRotation), 1);
                Assert.LessOrEqual(Mathf.Abs(Mathf.DeltaAngle(0, rect.localEulerAngles.x)), settings.PreviewTiltDegrees + .01f);
                Assert.LessOrEqual(Mathf.Abs(Mathf.DeltaAngle(0, rect.localEulerAngles.y)), settings.PreviewTiltDegrees + .01f);
                var angle = rect.localRotation;
                var second = new UnityEngine.EventSystems.PointerEventData(null) { pointerId = pointerId + 100, position = -data.position };
                rotation.OnDrag(second); rotation.OnPointerUp(second);
                Assert.Less(Quaternion.Angle(angle, rect.localRotation), .01f, "Another finger must not take over the gesture.");
                rotation.OnPointerUp(data); rotation.OnEndDrag(data); rotation.OnPointerClick(data);
                Assert.AreEqual(0, clicks, "Dragging must not close the preview.");
                DOTween.Complete(rotation);
                Assert.Less(Quaternion.Angle(Quaternion.identity, rect.localRotation), .01f);
                data.position = Vector2.zero;
                rotation.OnPointerDown(data); rotation.OnPointerUp(data); rotation.OnPointerClick(data);
                Assert.AreEqual(1, clicks, "A normal tap must still close the preview.");
            }
            finally { rotation.Stop(); Object.DestroyImmediate(cardGo); Object.DestroyImmediate(canvasGo); }
            Assert.AreEqual(activeBefore, DOTween.TotalActiveTweens(), "Preview rotation must not leak animations after teardown.");
        }

        [TestCase(6)]
        [TestCase(9)]
        public void HandCreatureFace_HasOpaqueFrontBackingBehindItsArt(int count)
        {
            var go = new GameObject("HandDepthTest");
            var cameraGo = new GameObject("HandDepthCamera");
            GameObject instance = null;
            try
            {
                var settings = AssetDatabase.LoadAssetAtPath<TablePresentationSettings>("Assets/Data/Config/TablePresentation.asset");
                var style = AssetDatabase.LoadAssetAtPath<CreatureCardStyle>("Assets/Data/Config/CreatureCardStyle.asset");
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab");
                instance = Object.Instantiate(prefab);
                var card = instance.GetComponent<CardView>();
                var visual = AssetDatabase.LoadAssetAtPath<VisualSettings>("Assets/Data/Config/VisualSettings.asset");
                var database = AssetDatabase.LoadAssetAtPath<Game.Data.Cards.CardDatabase>("Assets/Data/CardDatabase.asset");
                var texts = AssetDatabase.LoadAssetAtPath<UiTexts>("Assets/Data/Config/UiTexts.asset");
                var faces = new CardFaceBuilder(database, visual, texts);
                card.Bind(999, "creature.lord-of-crimson-forge", faces.Build("creature.lord-of-crimson-forge"), new CardSkin(visual));
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var back = (Renderer)typeof(CardView).GetField("_frame", flags).GetValue(card);
                Assert.IsTrue(back.gameObject.activeInHierarchy && back.enabled, "Creature binding must retain the opaque front backing.");
                Assert.Less(back.sharedMaterial.renderQueue, 2500, "The backing must occlude transparent creature canvases.");
                Assert.AreEqual(1f, back.sharedMaterial.GetFloat("_ZWrite"));
                var backOffset = instance.transform.InverseTransformPoint(back.transform.position);
                var faceOffset = instance.transform.InverseTransformPoint(card.Visual.TransformPoint(Vector3.forward * style.FaceDepth));
                var layout = go.AddComponent<TableLayout>();
                layout.Configure(settings, cameraGo.AddComponent<Camera>());
                var normal = Quaternion.Euler(settings.CameraPitch, 0, 0) * Vector3.forward;
                for (var i = 1; i < count; i++)
                {
                    var left = layout.Hand(TableSide.Bottom, i - 1, count, true);
                    var right = layout.Hand(TableSide.Bottom, i, count, true);
                    var leftFace = left.Position + left.Rotation * (faceOffset * left.Scale);
                    var rightBack = right.Position + right.Rotation * (backOffset * right.Scale);
                    Assert.Greater(Vector3.Dot(leftFace - rightBack, normal), .005f,
                        "The complete creature art must be behind its right neighbour, not only the glow or root transform.");
                }
            }
            finally { if (instance != null) Object.DestroyImmediate(instance); Object.DestroyImmediate(go); Object.DestroyImmediate(cameraGo); }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void ReturnedCreature_DoesNotRestoreOldHoverOrPin(bool pinned, bool focusedCardLeaves)
        {
            var activeBefore = DOTween.TotalActiveTweens();
            var cameraGo = new GameObject("RecycleFocusCamera");
            var cards = new CardView[3];
            try
            {
                var camera = cameraGo.AddComponent<Camera>();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab");
                var animation = AssetDatabase.LoadAssetAtPath<AnimationSettings>("Assets/Data/Config/AnimationSettings.asset");
                for (var i = 0; i < cards.Length; i++)
                {
                    cards[i] = Object.Instantiate(prefab).GetComponent<CardView>();
                    cards[i].transform.position = new Vector3(i - 1, 0, 10);
                    cards[i].SetInHand(focusedCardLeaves || i != 0);
                }
                var focus = new HandFocus(camera, animation, () => cards, new Vector2(1.4f, 2));
                if (pinned) focus.Pin(cards[1]);
                else focus.Hover(cards[1]);
                foreach (var card in cards) DOTween.Complete(card);
                Assert.IsTrue(cards[1].IsFocused);

                // The same CardView survives in the field/out pile and later returns to the hand.
                if (focusedCardLeaves)
                {
                    cards[1].SetInHand(false);
                    focus.Refresh();
                    foreach (var card in cards) DOTween.Complete(card);
                }
                cards[focusedCardLeaves ? 1 : 0].SetInHand(true);
                focus.Refresh();
                foreach (var card in cards) DOTween.Complete(card);
                foreach (var card in cards)
                {
                    Assert.IsFalse(card.IsFocused, "Recycling must not resurrect old selection.");
                    Assert.IsFalse(card.IsDisplaced);
                    Assert.AreEqual(Vector3.zero, card.Visual.localPosition);
                }

                // A new gesture must still be able to raise the returned creature.
                focus.Pin(cards[1]);
                Assert.IsTrue(cards[1].IsFocused);
            }
            finally
            {
                foreach (var card in cards) if (card != null)
                {
                    // EditMode does not run the card's normal MonoBehaviour destruction lifecycle.
                    DOTween.Kill(card, complete: false);
                    Object.DestroyImmediate(card.gameObject);
                }
                Object.DestroyImmediate(cameraGo);
            }
            Assert.AreEqual(activeBefore, DOTween.TotalActiveTweens(), "The test must not leave tweens targeting destroyed cards.");
        }

        [Test]
        public void CancelledFieldCondition_StrikesTextWithoutBadge_AndRestoresIt()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab");
            var card = Object.Instantiate(prefab).GetComponent<CardView>();
            try
            {
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var face = new CardFace("Поле", "", "Условие", "", Color.white, Color.white, null,
                    Color.white, null, null, null, hasCondition: true);
                typeof(CardView).GetField("_face", flags).SetValue(card, face);
                var text = (TMPro.TMP_Text)typeof(CardView).GetField("_text", flags).GetValue(card);
                var badge = (GameObject)typeof(CardView).GetField("_conditionBadge", flags).GetValue(card);
                card.SetCondition(ConditionMark.Cancelled);
                Assert.IsTrue((text.fontStyle & TMPro.FontStyles.Strikethrough) != 0);
                Assert.IsFalse(badge.activeSelf);
                card.SetCondition(ConditionMark.Active);
                Assert.IsFalse((text.fontStyle & TMPro.FontStyles.Strikethrough) != 0);
                Assert.IsFalse(badge.activeSelf);
                card.SetCondition(ConditionMark.None);
                Assert.IsFalse((text.fontStyle & TMPro.FontStyles.Strikethrough) != 0);
            }
            finally { Object.DestroyImmediate(card.gameObject); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PlayedFocusedCard_ReleasesItsNeighbours(bool refreshWithoutPointerMove)
        {
            var activeBefore = DOTween.TotalActiveTweens();
            var cameraGo = new GameObject("FocusCamera");
            var cards = new CardView[3];
            try
            {
                var camera = cameraGo.AddComponent<Camera>();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Card3D.prefab");
                var animation = AssetDatabase.LoadAssetAtPath<AnimationSettings>("Assets/Data/Config/AnimationSettings.asset");
                Assert.NotNull(animation);
                for (var i = 0; i < cards.Length; i++)
                {
                    cards[i] = Object.Instantiate(prefab).GetComponent<CardView>();
                    cards[i].transform.position = new Vector3(i - 1, 0, 10);
                    cards[i].SetInHand(true);
                }
                var focus = new HandFocus(camera, animation, () => cards, new Vector2(1.4f, 2));
                focus.Hover(cards[1]);
                foreach (var card in cards) DOTween.Complete(card);
                Assert.IsTrue(cards[0].IsDisplaced);
                Assert.IsTrue(cards[2].IsDisplaced);

                cards[1].SetInHand(false);
                // TableSync has compacted the root slots, but no pointer movement is required to settle their visuals.
                cards[0].Place(new CardPose(new Vector3(-.5f, 0, 10), Quaternion.identity));
                cards[2].Place(new CardPose(new Vector3(.5f, 0, 10), Quaternion.identity));
                if (refreshWithoutPointerMove) focus.Refresh();
                else focus.Hover(null);
                foreach (var card in cards) DOTween.Complete(card);
                foreach (var card in new[] { cards[0], cards[2] })
                {
                    Assert.IsFalse(card.IsDisplaced, "Played card must not leave neighbour spread behind.");
                    Assert.AreEqual(Vector3.zero, card.Visual.localPosition);
                }
            }
            finally
            {
                foreach (var card in cards) if (card != null)
                {
                    DOTween.Kill(card, complete: false);
                    Object.DestroyImmediate(card.gameObject);
                }
                Object.DestroyImmediate(cameraGo);
            }
            Assert.AreEqual(activeBefore, DOTween.TotalActiveTweens(), "The test must not leave tweens targeting destroyed cards.");
        }

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
