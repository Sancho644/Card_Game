using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>Shared layered creature face for world cards, zoom and collection. All labels remain live.</summary>
    public sealed class CreatureCardFaceView : CardFaceView
    {
        private CardFace _face;
        private CardSkin _skin;
        private TMP_FontAsset _font;
        private TMP_Text _power;
        public TMP_Text PowerText => _power;

        public static CreatureCardFaceView Create(Transform parent) => CreateFace<CreatureCardFaceView>(parent, "CreatureFace");

        public void Bind(CardFace face, CardSkin skin, TMP_FontAsset font)
        {
            gameObject.SetActive(true);
            if (_face == face && _skin == skin && _font == font) return;
            DOTween.Kill(this);
            ResetFace(font);
            _face = face;
            _skin = skin;
            _font = font;
            var style = skin.CreatureStyle;

            DrawArt(style.Art, face.Art, style.WorldSize.x / style.WorldSize.y, ElementBackdrop(face.FrameColor), .96f);
            if (face.Art == null)
                SpriteNode("ArtIcon", new Rect(.25f, .43f, .50f, .27f), face.ElementIcon, Color.white);
            SpriteNode("Frame", style.FrameArea, face.CreatureFrame != null ? face.CreatureFrame : style.Frame,
                Color.white, preserveAspect: false);
            SpriteNode("ElementIcon", style.ElementIcon, face.ElementIcon, Color.white);
            _power = Label("Power", style.Power, face.Power?.ToString(), style.Ink, 74, 128);
            Label("Element", style.Element, face.Stats, style.Ink, 25, 37);
            Label("Title", style.Title, face.Title, style.Ink, 49, 76, wrap: true);
            Label("Type", style.Type, face.TypeLabel, style.Ink, 23, 32);
            DrawRarity(style.Rarity, face, style.Ink);

            if (!string.IsNullOrEmpty(face.Body))
            {
                Panel("RulesBackground", style.Rules, new Color(.04f, .06f, .07f, .93f));
                Label("Rules", style.Rules, face.Body, style.Ink, 24, 39, wrap: true);
            }
        }

        public void SetPower(int? value)
        {
            if (_power != null) _power.text = (value ?? _face.Power ?? 0).ToString();
        }

        public Tween FlashPower(Color color, float seconds)
        {
            if (_power == null) return null;
            var original = _skin.CreatureStyle.Ink;
            return DOTween.Sequence().Append(_power.DOColor(color, seconds * .25f))
                .Append(_power.DOColor(original, seconds * .75f)).SetTarget(this);
        }

        protected override void OnDestroy()
        {
            DOTween.Kill(this);
            base.OnDestroy();
        }
    }
}
