using System.Collections.Generic;
using System.Linq;
using Game.Data.Cards;
using Game.Data.Effects;

namespace Game.Data.Config
{
    /// <summary>Checks cards against the reference numbers. Warnings, not errors: a card may break them on purpose.</summary>
    public sealed class CardBalanceValidator
    {
        private readonly BalanceConfig _balance;

        public CardBalanceValidator(BalanceConfig balance)
        {
            _balance = balance;
        }

        public IEnumerable<string> Check(IEnumerable<CardDef> cards)
        {
            foreach (var card in cards.Where(card => card != null))
            {
                var balance = _balance.For(card.Rarity);
                if (balance == null)
                {
                    yield return $"{card.name}: no balance numbers for {card.Rarity}";
                    continue;
                }

                foreach (var warning in Check(card, balance))
                    yield return $"{card.name}: {warning}";
            }
        }

        private IEnumerable<string> Check(CardDef card, RarityBalance balance)
        {
            switch (card)
            {
                case CreatureDef creature when !balance.CreaturePower.Contains(creature.Power):
                    yield return $"power {creature.Power} is outside {balance.CreaturePower}";
                    break;

                case FieldDef field:
                    var bonuses = field.Modifiers.Where(modifier => modifier.Value > 0).ToList();
                    if (!balance.FieldBonusElements.Contains(bonuses.Count))
                        yield return $"bonuses to {bonuses.Count} elements, expected {balance.FieldBonusElements}";
                    foreach (var bonus in bonuses.Where(bonus => !balance.FieldBonus.Contains(bonus.Value)))
                        yield return $"{bonus.Element} +{bonus.Value} is outside {balance.FieldBonus}";
                    break;

                case AbilityDef ability:
                    var power = ability.Effects.OfType<PowerBonusData>().Sum(effect => effect.Amount);
                    var expected = ability.BoundCreature != null ? _balance.SignatureAbilityPower : balance.AbilityPower;
                    if (!expected.Contains(power))
                        yield return $"adds {power} power, expected {expected}";
                    break;
            }
        }
    }
}
