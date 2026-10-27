using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    /// <summary>A precondition of a power bonus, as stored in a card asset.</summary>
    [Serializable]
    public abstract class RequirementData
    {
        public abstract IEffectRequirement Create();
    }
}
