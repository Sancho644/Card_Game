using Game.Core.Cards;
using UnityEngine;

namespace Game.Data.Cards
{
    [CreateAssetMenu(menuName = "Game/Cards/Creature", fileName = "Creature")]
    public sealed class CreatureDef : CardDef
    {
        [SerializeField] private Element _element;
        [SerializeField] private int _power = 150;

        public override CardType Type => CardType.Creature;
        public Element Element => _element;
        public int Power => _power;

        public override CardDefinition ToCore() => new CreatureCard(Id, Rarity, _element, _power);
    }
}
