namespace Game.Core.Cards
{
    public sealed class CreatureCard : CardDefinition
    {
        public CreatureCard(string id, Rarity rarity, Element element, int power)
            : base(id, rarity)
        {
            Element = element;
            Power = power;
        }

        public override CardType Type => CardType.Creature;
        public Element Element { get; }
        public int Power { get; }
    }
}
