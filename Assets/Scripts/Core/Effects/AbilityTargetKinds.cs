using System;

namespace Game.Core.Effects
{
    /// <summary>Which targets a play effect reads from <see cref="Commands.AbilityTargets"/>.</summary>
    [Flags]
    public enum AbilityTargetKinds
    {
        None = 0,

        /// <summary>A creature on a field.</summary>
        Creature = 1 << 0,

        /// <summary>A field card.</summary>
        Field = 1 << 1,

        /// <summary>A card in a hand or a pile.</summary>
        Card = 1 << 2,
    }
}
