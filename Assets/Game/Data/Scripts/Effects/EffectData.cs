using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    /// <summary>Name shown in the inspector's "add effect" menu. Designer-facing, not player-facing.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DataLabelAttribute : Attribute
    {
        public DataLabelAttribute(string label)
        {
            Label = label;
        }

        public string Label { get; }
    }

    /// <summary>
    /// An ability effect as stored in a card asset ([SerializeReference]). Each subclass builds one
    /// engine effect; a new effect is a new subclass, nothing else changes.
    /// </summary>
    [Serializable]
    public abstract class AbilityEffectData
    {
        public abstract IAbilityEffect Create();
    }

    /// <summary>A precondition of a power bonus, as stored in a card asset.</summary>
    [Serializable]
    public abstract class RequirementData
    {
        public abstract IEffectRequirement Create();
    }

    /// <summary>A field condition, as stored in a card asset.</summary>
    [Serializable]
    public abstract class ConditionData
    {
        public abstract FieldCondition Create();
    }
}
