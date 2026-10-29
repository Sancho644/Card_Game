using System.Linq;
using Game.Core.Cards;
using Game.Data.Cards;
using Game.Data.Presentation;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>Builds card faces from ScriptableObject data: element, rarity, name, numbers, text, art.</summary>
    public sealed class CardFaceBuilder
    {
        private readonly CardDatabase _database;
        private readonly VisualSettings _visual;
        private readonly UiTexts _texts;

        public CardFaceBuilder(CardDatabase database, VisualSettings visual, UiTexts texts)
        {
            _database = database;
            _visual = visual;
            _texts = texts;
        }

        public CardFace Build(string cardId)
        {
            var card = cardId == null ? null : _database.Find(cardId);
            if (card == null)
            {
                return new CardFace(
                    _texts.Get(UiTextKeys.HiddenCard), "", "", "", _visual.NeutralFrame, _visual.NeutralFrame, null,
                    _visual.NeutralFrame, null, null, null);
            }

            var rarity = _visual.StyleOf(card.Rarity);
            var element = ElementOf(card);
            var frame = element.HasValue ? _visual.FrameColorOf(element.Value) : card is AbilityDef ? _visual.AbilityFrame : _visual.NeutralFrame;
            return new CardFace(
                card.DisplayName,
                StatsOf(card),
                card.Description,
                _texts.Get(UiTextKeys.RarityPrefix + card.Rarity),
                frame,
                rarity.Color,
                rarity.Icon,
                frame,
                element.HasValue ? _visual.IconOf(element.Value) : null,
                card.Art,
                card is CreatureDef creature ? creature.Power : (int?)null,
                card is FieldDef field && field.Conditions.Count > 0);
        }

        public string NameOf(string cardId)
        {
            var card = cardId == null ? null : _database.Find(cardId);
            return card == null ? _texts.Get(UiTextKeys.HiddenCard) : card.DisplayName;
        }

        public string ElementName(Element element) => _texts.Get(UiTextKeys.ElementPrefix + element);

        /// <summary>The creature's element, or the element of the field's largest bonus; null for abilities.</summary>
        private static Element? ElementOf(CardDef card)
        {
            switch (card)
            {
                case CreatureDef creature:
                    return creature.Element;
                case FieldDef field when field.Modifiers.Count > 0:
                    return field.Modifiers.OrderByDescending(modifier => modifier.Value).First().Element;
                default:
                    return null;
            }
        }

        private string StatsOf(CardDef card)
        {
            switch (card)
            {
                case CreatureDef creature:
                    return ElementName(creature.Element);
                case FieldDef field:
                    return string.Join("  ", field.Modifiers.Select(modifier =>
                        $"{ElementName(modifier.Element)} {(modifier.Value > 0 ? "+" : "-")}{Mathf.Abs(modifier.Value)}"));
                case AbilityDef _:
                    return _texts.Get(UiTextKeys.CardAbility);
                default:
                    return "";
            }
        }
    }
}
