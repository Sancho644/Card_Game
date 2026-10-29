using Game.Data.Presentation;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>What every card shares: body and text colors, the back, the power badge, the art placeholder.</summary>
    public sealed class CardSkin
    {
        private const float LightBackground = 0.55f;

        public CardSkin(VisualSettings visual)
        {
            Body = visual.CardBody;
            Back = visual.CardBack;
            BackArt = visual.CardBackArt;
            Text = visual.CardText;
            PowerBadge = visual.PowerBadge;
            PlaceholderBrightness = visual.ArtPlaceholderBrightness;
            StripIcon = visual.StripIconColor;
        }

        public Color Body { get; }
        public Color Back { get; }
        public Sprite BackArt { get; }
        public Color Text { get; }
        public Color PowerBadge { get; }

        /// <summary>How dark the element color is behind the icon when a card has no art yet.</summary>
        public float PlaceholderBrightness { get; }

        /// <summary>Dark color for icons and text on a light strip.</summary>
        public Color StripIcon { get; }

        /// <summary>Text and icon color that reads on the background: dark on light colors, the card text color on dark ones.</summary>
        public Color InkOn(Color background) =>
            0.299f * background.r + 0.587f * background.g + 0.114f * background.b > LightBackground ? StripIcon : Text;
    }
}
