using System;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Effects;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>What a single card is worth to the AI by the profile's weights.</summary>
    internal sealed class CardValues
    {
        private readonly AiWeights _weights;

        public CardValues(AiWeights weights)
        {
            _weights = weights;
        }

        public double Creature(CardDefinition card) =>
            card is CreatureCard creature ? creature.Power * _weights.CreaturePerPower : 0;

        public double Ability(CardDefinition card)
        {
            switch (card.Rarity)
            {
                case Rarity.Common:
                    return _weights.CommonAbility;
                case Rarity.Epic:
                    return _weights.EpicAbility;
                default:
                    return _weights.LegendaryAbility;
            }
        }

        /// <summary>
        /// An ability in its owner's hand. One that brings back a spent ability is worth a share of what it
        /// could bring back, not its rarity: priced as a legendary, returning a common or epic card would
        /// always look like a loss and the card would never be played.
        /// </summary>
        public double AbilityInHand(CardInstance card, PlayerState owner)
        {
            var ability = (AbilityCard)card.Definition;
            if (!ability.Effects.OfType<ReturnAbilityFromDiscardEffect>().Any())
                return Ability(ability);

            var best = owner.Discard
                .Where(spent => spent.Id != card.Id)
                .Select(spent => Ability(spent.Definition))
                .DefaultIfEmpty(0)
                .Max();
            return _weights.ReturnedCardShare * Math.Max(_weights.CommonAbility, best);
        }
    }
}
