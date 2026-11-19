using System.Collections.Generic;
using Game.Core.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Presentation.Cards
{
    [ExecuteAlways]
    public abstract class CardFaceView : UIBehaviour
    {
        private const float ReferenceHeight = 1000f;
        private static readonly Rect FullArea = new Rect(0, 0, 1, 1);
        private readonly Dictionary<Graphic, Color> _colors = new Dictionary<Graphic, Color>();
        private readonly List<(TMP_Text Text, float Min, float Max)> _labels = new List<(TMP_Text, float, float)>();
        private TMP_FontAsset _font;
        private float _labelScale;

        protected static T CreateFace<T>(Transform parent, string name) where T : CardFaceView =>
            NewRect(parent, name, FullArea).gameObject.AddComponent<T>();

        protected void ResetFace(TMP_FontAsset font)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                child.SetActive(false);
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
            _colors.Clear();
            _labels.Clear();
            _font = font;
            _labelScale = HeightScale();
        }

        public void SetDimming(float brightness)
        {
            foreach (var entry in _colors)
            {
                var color = entry.Value;
                entry.Key.color = new Color(color.r * brightness, color.g * brightness, color.b * brightness, color.a);
            }
        }

        protected RectTransform Node(string name, Rect area, Transform parent = null) =>
            NewRect(parent ?? transform, name, area);

        protected UnityEngine.UI.Image SpriteNode(string name, Rect area, Sprite sprite, Color color,
            Transform parent = null, bool preserveAspect = true)
        {
            if (sprite == null) return null;
            var image = Node(name, area, parent).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;
            Remember(image, color);
            return image;
        }

        protected void Panel(string name, Rect area, Color color, Transform parent = null)
        {
            var image = Node(name, area, parent).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.raycastTarget = false;
            Remember(image, color);
        }

        protected TMP_Text Label(string name, Rect area, string value, Color color, float min, float max,
            Transform parent = null, bool wrap = false, TextAlignmentOptions alignment = TextAlignmentOptions.Midline)
        {
            var label = Node(name, area, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = _font;
            label.text = value ?? "";
            label.richText = false;
            label.enableAutoSizing = true;
            label.alignment = alignment;
            label.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            Remember(label, color);
            _labels.Add((label, min, max));
            ApplyFontRange(label, min, max, _labelScale);
            return label;
        }

        protected void DrawArt(Rect area, Sprite sprite, float cardAspect, Color placeholder, float minimumVisible = 0)
        {
            if (sprite == null)
            {
                Panel("Art", area, placeholder);
                return;
            }

            var source = sprite.textureRect;
            var sourceAspect = source.width / source.height;
            var windowAspect = cardAspect * area.width / area.height;
            // Limit how much of either source dimension can be lost. Keep a subdued bleed in the
            // unused part of the window, then render the illustration itself with its original colors.
            var artAspect = Mathf.Clamp(windowAspect, sourceAspect * minimumVisible,
                minimumVisible > 0 ? sourceAspect / minimumVisible : float.PositiveInfinity);
            var artArea = area;
            if (artAspect < windowAspect)
            {
                artArea.width *= artAspect / windowAspect;
                artArea.x += (area.width - artArea.width) / 2;
            }
            else if (artAspect > windowAspect)
            {
                artArea.height *= windowAspect / artAspect;
                artArea.y += (area.height - artArea.height) / 2;
            }

            var fitted = artArea != area;
            if (fitted)
                DrawSpriteCrop("ArtBleed", area, sprite, windowAspect, new Color(.58f, .58f, .58f, 1));
            DrawSpriteCrop("Art", artArea, sprite, artAspect, Color.white, fitted, artArea.width < area.width);
        }

        protected void DrawRarity(Rect area, CardFace face, Color ink)
        {
            var group = Node("RarityFinish", area);
            var grade = face.Grade;
            var distinguished = grade != Rarity.Common;
            var legendary = grade == Rarity.Legendary;
            var metal = legendary ? new Color(.91f, .65f, .28f) : new Color(.65f, .60f, .77f);
            var accent = legendary ? new Color(.28f, .17f, .07f) : distinguished
                ? new Color(.19f, .14f, .27f) : new Color(.12f, .13f, .13f);
            // Inset sits inside each template's existing metal footer.
            Panel("RarityInset", FullArea, accent, group);
            if (distinguished)
            {
                Panel("MetalTrim", new Rect(.02f, .89f, .96f, .10f), metal, group);
            }
            if (legendary)
            {
                Panel("GoldTrim", new Rect(.02f, 0, .96f, .085f), metal, group);
                SpriteNode("SecondGem", new Rect(.86f, .09f, .12f, .72f), face.RarityIcon, face.RarityColor, group);
            }
            SpriteNode("RarityGem", new Rect(.02f, .08f, .16f, .78f), face.RarityIcon, face.RarityColor, group);
            Label("Rarity", new Rect(.18f, .07f, legendary ? .67f : .80f, .79f), face.Rarity, ink, 21, 31, group);
        }

        protected Color ElementBackdrop(Color color) => new Color(color.r * .24f, color.g * .24f, color.b * .24f, 1);

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            var scale = HeightScale();
            if (Mathf.Approximately(scale, _labelScale)) return;
            _labelScale = scale;
            foreach (var label in _labels)
                if (label.Text != null) ApplyFontRange(label.Text, label.Min, label.Max, scale);
        }

        private float HeightScale()
        {
            // World cards use the design height. Collection tiles, deck slots and zoom have their
            // own smaller rectangles. Scale the limits with the card rather than the screen.
            var height = ((RectTransform)transform).rect.height;
            return height > 0 ? height / ReferenceHeight : 1;
        }

        private static void ApplyFontRange(TMP_Text label, float min, float max, float scale)
        {
            label.fontSizeMin = min * scale;
            label.fontSizeMax = max * scale;
            label.fontSize = max * scale;
        }

        private static RectTransform NewRect(Transform parent, string name, Rect area)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = area.min;
            rect.anchorMax = area.max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private void DrawSpriteCrop(string name, Rect area, Sprite sprite, float aspect, Color tint,
            bool feather = false, bool horizontal = false)
        {
            var uv = sprite.textureRect;
            if (uv.width / uv.height > aspect)
            {
                var width = uv.height * aspect;
                uv.x += (uv.width - width) / 2;
                uv.width = width;
            }
            else
            {
                var height = uv.width / aspect;
                uv.y += (uv.height - height) / 2;
                uv.height = height;
            }
            var texture = sprite.texture;
            var node = Node(name, area).gameObject;
            RawImage image;
            if (feather)
            {
                var fitted = node.AddComponent<CardArtImage>();
                fitted.FeatherHorizontally = horizontal;
                image = fitted;
            }
            else
            {
                image = node.AddComponent<RawImage>();
            }
            image.texture = texture;
            image.uvRect = new Rect(uv.x / texture.width, uv.y / texture.height,
                uv.width / texture.width, uv.height / texture.height);
            image.raycastTarget = false;
            Remember(image, tint);
        }

        private void Remember(Graphic graphic, Color color)
        {
            graphic.color = color;
            _colors.Add(graphic, color);
        }
    }
}
