using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Data.Effects;
using UnityEngine;

namespace Game.Data.Cards
{
    [Serializable]
    public struct ElementValue
    {
        public Element Element;
        public int Value;

        public ElementValue(Element element, int value)
        {
            Element = element;
            Value = value;
        }
    }

    [CreateAssetMenu(menuName = "Game/Cards/Field", fileName = "Field")]
    public sealed class FieldDef : CardDef
    {
        [Tooltip("Bonuses (positive) and penalties (negative) by element.")]
        [SerializeField] private List<ElementValue> _modifiers = new List<ElementValue>();

        [SerializeReference] private List<ConditionData> _conditions = new List<ConditionData>();

        public override CardType Type => CardType.Field;
        public IReadOnlyList<ElementValue> Modifiers => _modifiers;
        public IReadOnlyList<ConditionData> Conditions => _conditions;

        public override CardDefinition ToCore() =>
            new FieldCard(
                Id,
                Rarity,
                _modifiers.ToDictionary(modifier => modifier.Element, modifier => modifier.Value),
                _conditions.Where(condition => condition != null).Select(condition => condition.Create()));

        public override IEnumerable<string> Validate()
        {
            foreach (var problem in base.Validate())
                yield return problem;
            foreach (var element in _modifiers.GroupBy(modifier => modifier.Element).Where(group => group.Count() > 1))
                yield return $"element {element.Key} is listed twice";
            if (_conditions.Any(condition => condition == null))
                yield return "a condition has no type selected";
        }
    }
}
