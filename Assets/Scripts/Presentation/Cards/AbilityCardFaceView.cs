using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Cards
{
    /// <summary>The shared spell face used in the collection, table and enlarged preview.</summary>
    public sealed class AbilityCardFaceView : MonoBehaviour
    {
        private readonly Dictionary<Graphic, Color> _colors = new Dictionary<Graphic, Color>();
        private CardFace _face;
        private CardSkin _skin;
        private TMP_FontAsset _font;

        public static AbilityCardFaceView Create(Transform parent)
        {
            var rect = new GameObject("AbilityFace", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect.gameObject.AddComponent<AbilityCardFaceView>();
        }

        public void Bind(CardFace face, CardSkin skin, TMP_FontAsset font)
        {
            gameObject.SetActive(true);
            if (_face == face && _skin == skin && _font == font) return;
            foreach (Transform child in transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            _colors.Clear(); _face = face; _skin = skin; _font = font;
            var style = skin.AbilityStyle;
            var art = Node("Art", style.Art).gameObject.AddComponent<UnityEngine.UI.RawImage>();
            art.raycastTarget = false;
            if (face.Art != null)
            {
                art.texture = face.Art.texture;
                var uv = face.Art.textureRect; var texture = face.Art.texture;
                var aspect = style.WorldSize.x / style.WorldSize.y * style.Art.width / style.Art.height;
                if (uv.width / uv.height > aspect) { var width = uv.height * aspect; uv.x += (uv.width - width) / 2; uv.width = width; }
                else { var height = uv.width / aspect; uv.y += (uv.height - height) / 2; uv.height = height; }
                art.uvRect = new Rect(uv.x / texture.width, uv.y / texture.height, uv.width / texture.width, uv.height / texture.height);
                Remember(art, Color.white);
            }
            else
            {
                Remember(art, style.Placeholder);
                Image("ArtPlaceholder", new Rect(.36f, .61f, .28f, .20f), face.RarityIcon, face.RarityColor, preserveAspect: true);
            }
            Image("Frame", new Rect(0, 0, 1, 1), style.Frame, Color.white);
            Image("RarityGem", style.RarityIcon, face.RarityIcon, face.RarityColor, preserveAspect: true);
            var title = Text("Title", style.Title, face.Title);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            var timing = Text("Timing", style.Timing, face.Ability.Timing);
            timing.textWrappingMode = TextWrappingModes.NoWrap;
            Text("Rules", style.Rules, face.Ability.Rules);
            Text("Type", style.Type, face.TypeLabel).textWrappingMode = TextWrappingModes.NoWrap;
            Text("Rarity", style.Rarity, face.Rarity).textWrappingMode = TextWrappingModes.NoWrap;
            if (face.Ability.Bonus.HasValue)
            {
                var panel = Node("Bonus", style.Bonus);
                Image("Panel", new Rect(0, 0, 1, 1), style.BonusPanel, Color.white, panel);
                var value = face.Ability.Bonus.Value;
                Text("Value", new Rect(.10f, .13f, .43f, .74f), (value > 0 ? "+" : "") + value, panel).textWrappingMode = TextWrappingModes.NoWrap;
                Text("Suffix", new Rect(.53f, .20f, .38f, .50f), face.Ability.BonusSuffix, panel).textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        public void SetDimming(float brightness)
        {
            foreach (var pair in _colors) if (pair.Key != null)
            {
                var color = pair.Value;
                pair.Key.color = new Color(color.r * brightness, color.g * brightness, color.b * brightness, color.a);
            }
        }

        private RectTransform Node(string name, Rect area, Transform parent = null)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent != null ? parent : transform, false);
            rect.anchorMin = area.min; rect.anchorMax = area.max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private void Image(string name, Rect area, Sprite sprite, Color color, Transform parent = null, bool preserveAspect = false)
        {
            if (sprite == null) return;
            var image = Node(name, area, parent).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite; image.raycastTarget = false; image.preserveAspect = preserveAspect;
            Remember(image, color);
        }

        private TMP_Text Text(string name, Rect area, string value, Transform parent = null)
        {
            var text = Node(name, area, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _font; text.text = value ?? ""; text.richText = false;
            text.enableAutoSizing = true; text.fontSizeMin = 1; text.fontSizeMax = 250;
            text.alignment = TextAlignmentOptions.Midline; text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false; Remember(text, _skin.AbilityStyle.Ink);
            return text;
        }

        private void Remember(Graphic graphic, Color color) { graphic.color = color; _colors.Add(graphic, color); }
    }
}
