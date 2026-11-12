using System.Linq;
using System;
using Game.Data.Effects;
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
                card is FieldDef field && field.Conditions.Count > 0, _texts.Get("collection.type." + card.Type),
                card is CreatureDef c ? _visual.CreatureFrameOf(c.Element) : null,
                card is FieldDef f ? f.Modifiers.Select(m=>new FieldBonusFace(ElementName(m.Element),m.Value,_visual.IconOf(m.Element))).ToArray() : null,
                card is AbilityDef ability ? BuildAbility(ability) : null);
        }

        public string NameOf(string cardId)
        {
            var card = cardId == null ? null : _database.Find(cardId);
            return card == null ? _texts.Get(UiTextKeys.HiddenCard) : card.DisplayName;
        }

        public string ElementName(Element element) => _texts.Get(UiTextKeys.ElementPrefix + element);

        private AbilityFace BuildAbility(AbilityDef ability)
        {
            var effects = ability.Effects.OfType<PowerBonusData>().ToArray();
            int? bonus = effects.Length == 1 && effects[0].Amount != 0 ? effects[0].Amount : (int?)null;
            var rules = ability.Description ?? "";
            if (bonus.HasValue)
            {
                // Remove only a duplicated numeric prefix with punctuation; retain every condition and other effect.
                var prefix = (bonus.Value > 0 ? "+" : "") + bonus.Value;
                if (rules.StartsWith(prefix + ".", StringComparison.Ordinal)
                    || rules.StartsWith(prefix + ",", StringComparison.Ordinal))
                {
                    rules = rules.Substring(prefix.Length + 1).TrimStart();
                    if (rules.Length > 0) rules = char.ToUpperInvariant(rules[0]) + rules.Substring(1);
                }
            }
            var timing = string.Join(" · ", Enum.GetValues(typeof(AbilityTiming)).Cast<AbilityTiming>()
                .Where(t => t != AbilityTiming.None && (ability.Timing & t) == t)
                .Select(t => _texts.Get("card.ability.timing." + t)));
            return new AbilityFace(bonus, _texts.Get("card.ability.bonusSuffix"), timing, rules);
        }

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
