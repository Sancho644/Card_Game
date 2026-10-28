using Game.Core.Cards;

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
    }
}
