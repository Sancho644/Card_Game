using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Compares two power breakdowns of the same creature by source: the field's value and each ability's
    /// contribution. A cancelled ability shows up as its contribution taken back.
    /// </summary>
    public sealed class PowerDiff
    {
        /// <summary>Key of the field's value among the sources; abilities use their instance ids.</summary>
        public const int FieldSource = 0;

        private readonly CardNameLookup _lookup;

        public PowerDiff(CardNameLookup lookup)
        {
            _lookup = lookup;
        }

        /// <param name="before">The breakdown shown last time, null if the creature had none.</param>
        /// <param name="fieldId">The battle's field, named as the source of the field value.</param>
        public IReadOnlyList<PowerChange> Changes(PowerBreakdown before, PowerBreakdown after, int fieldId)
        {
            var old = Sources(before);
            var now = Sources(after);
            var changes = new List<PowerChange>();
            foreach (var source in now.Keys.Concat(old.Keys).Distinct())
            {
                now.TryGetValue(source, out var newAmount);
                old.TryGetValue(source, out var oldAmount);
                if (newAmount != oldAmount)
                    changes.Add(new PowerChange(_lookup.NameOf(source == FieldSource ? fieldId : source), newAmount - oldAmount));
            }

            return changes;
        }

        private Dictionary<int, int> Sources(PowerBreakdown power)
        {
            var sources = new Dictionary<int, int>();
            if (power == null)
                return sources;

            sources[FieldSource] = power.FieldValue;
            foreach (var ability in power.Abilities)
            {
                _lookup.Remember(ability.Source);
                var id = ability.Source?.InstanceId ?? FieldSource;
                sources.TryGetValue(id, out var amount);
                sources[id] = amount + ability.Amount;
            }

            return sources;
        }
    }
}
