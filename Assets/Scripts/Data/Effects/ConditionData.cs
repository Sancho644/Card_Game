using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    /// <summary>A field condition, as stored in a card asset.</summary>
    [Serializable]
    public abstract class ConditionData
    {
        public abstract FieldCondition Create();
    }
}
