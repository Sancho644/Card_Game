using System;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    /// <summary>
    /// An ability effect as stored in a card asset ([SerializeReference]). Each subclass builds one
    /// engine effect; a new effect is a new subclass, nothing else changes.
    /// </summary>
    [Serializable]
    public abstract class AbilityEffectData
    {
        public abstract IAbilityEffect Create();
    }
}
