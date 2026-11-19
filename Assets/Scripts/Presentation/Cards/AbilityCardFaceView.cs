using TMPro;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>The shared spell face used in the collection, table and enlarged preview.</summary>
    public sealed class AbilityCardFaceView : CardFaceView
    {
        private CardFace _face;
        private CardSkin _skin;
        private TMP_FontAsset _font;

        public static AbilityCardFaceView Create(Transform parent) => CreateFace<AbilityCardFaceView>(parent, "AbilityFace");

        public void Bind(CardFace face, CardSkin skin, TMP_FontAsset font)
        {
            gameObject.SetActive(true);
            if (_face == face && _skin == skin && _font == font) return;
            ResetFace(font);
            _face = face;
            _skin = skin;
            _font = font;
            var style = skin.AbilityStyle;

            DrawArt(style.Art, face.Art, style.WorldSize.x / style.WorldSize.y, style.Placeholder);
            if (face.Art == null)
                SpriteNode("ArtPlaceholder", new Rect(.36f, .61f, .28f, .20f), face.RarityIcon, face.RarityColor);
            SpriteNode("Frame", new Rect(0, 0, 1, 1), style.Frame, Color.white, preserveAspect: false);
            SpriteNode("CornerGem", style.RarityIcon, face.RarityIcon, face.RarityColor);
            Label("Title", style.Title, face.Title, style.Ink, 32, 54, wrap: true);
            var timing = style.Timing;
            if (!face.Ability.Bonus.HasValue)
            {
                timing.xMin = style.Bonus.xMin;
            }
            else
            {
                var panel = Node("Bonus", style.Bonus);
                SpriteNode("Panel", new Rect(0, 0, 1, 1), style.BonusPanel, Color.white, panel, false);
                var value = face.Ability.Bonus.Value;
                Label("Value", new Rect(.07f, .09f, .43f, .82f), (value > 0 ? "+" : "") + value,
                    new Color(1, .87f, .61f), 30, 51, panel);
                Label("Suffix", new Rect(.50f, .09f, .46f, .82f), face.Ability.BonusSuffix, style.Ink, 19, 28, panel);
            }
            Panel("TimingPlate", timing, new Color(.055f, .17f, .16f));
            Panel("TimingTrim", new Rect(timing.x, timing.yMax - .004f, timing.width, .004f), new Color(.60f, .46f, .26f));
            Label("Timing", timing, face.Ability.Timing, new Color(.90f, .97f, .82f), 23, 35, wrap: true);
            Label("Rules", style.Rules, face.Ability.Rules, style.Ink, 25, 43,
                wrap: true, alignment: TextAlignmentOptions.MidlineLeft);
            Label("Type", style.Type, face.TypeLabel, style.Ink, 22, 32);
            DrawRarity(style.Rarity, face, style.Ink);
        }
    }
}
