using System.Collections.Generic;

namespace Game.Core.Battle
{
    /// <summary>A creature's final power by the steps of the count (docs/rules.md §6).</summary>
    public sealed class PowerBreakdown
    {
        public PowerBreakdown(int basePower, int fieldValue, IReadOnlyList<PowerContribution> abilities, int finalPower)
        {
            BasePower = basePower;
            FieldValue = fieldValue;
            Abilities = abilities;
            FinalPower = finalPower;
        }

        public int BasePower { get; }

        /// <summary>Step 1: the open field's value for the creature's element, 0 if the field is closed.</summary>
        public int FieldValue { get; }

        /// <summary>Step 2: changes made by abilities, in play order.</summary>
        public IReadOnlyList<PowerContribution> Abilities { get; }

        public int FinalPower { get; }
    }
}
