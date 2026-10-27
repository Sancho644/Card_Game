using System.Linq;
using Game.Core.Cards;
using Game.Data.Cards;
using Game.Data.Presentation;

namespace Game.Presentation.Cards
{
    /// <summary>Builds card faces from ScriptableObject data: frame color, rarity, name, numbers, text.</summary>
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
                return new CardFace(_texts.Get(UiTextKeys.HiddenCard), "", "", "", _visual.NeutralFrame, _visual.NeutralFrame);

            var rarity = _visual.StyleOf(card.Rarity);
            return new CardFace(
                card.DisplayName,
                StatsOf(card),
                card.Description,
                _texts.Get(UiTextKeys.RarityPrefix + card.Rarity),
                FrameColorOf(card),
                rarity.Color,
                card is FieldDef field && field.Conditions.Count > 0);
        }

        public string NameOf(string cardId)
        {
            var card = cardId == null ? null : _database.Find(cardId);
            return card == null ? _texts.Get(UiTextKeys.HiddenCard) : card.DisplayName;
        }

        public string ElementName(Element element) => _texts.Get(UiTextKeys.ElementPrefix + element);

        private string StatsOf(CardDef card)
        {
            switch (card)
            {
                case CreatureDef creature:
                    return $"{ElementName(creature.Element)} · {_texts.Format(UiTextKeys.CardPower, creature.Power)}";
                case FieldDef field:
                    return string.Join("  ", field.Modifiers.Select(modifier =>
                        $"{ElementName(modifier.Element)} {(modifier.Value > 0 ? "+" : "-")}{System.Math.Abs(modifier.Value)}"));
                default:
                    return "";
            }
        }

        private UnityEngine.Color FrameColorOf(CardDef card)
        {
            switch (card)
            {
                case CreatureDef creature:
                    return _visual.FrameColorOf(creature.Element);
                case FieldDef field when field.Modifiers.Count > 0:
                    return _visual.FrameColorOf(field.Modifiers.OrderByDescending(modifier => modifier.Value).First().Element);
                case AbilityDef _:
                    return _visual.AbilityFrame;
                default:
                    return _visual.NeutralFrame;
            }
        }
    }
}
