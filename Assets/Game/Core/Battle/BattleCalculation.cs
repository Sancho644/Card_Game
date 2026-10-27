using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Battle
{
    public enum BattleRole
    {
        Host,
        Guest,
    }

    /// <summary>
    /// Working state of a battle being resolved. Field values are kept per player, because some
    /// effects change them for one creature only.
    /// </summary>
    public sealed class BattleCalculation
    {
        private readonly CreatureCard[] _creatures = new CreatureCard[PlayerIds.All.Count];
        private readonly Dictionary<Element, int>[] _fieldValues;
        private readonly int[] _abilityPower = new int[PlayerIds.All.Count];
        private readonly List<IConsequenceModifier> _consequenceModifiers = new List<IConsequenceModifier>();

        internal BattleCalculation(
            PlayerId host,
            CreatureCard hostCreature,
            CreatureCard guestCreature,
            FieldCard field,
            bool fieldOpen)
        {
            Host = host;
            _creatures[(int)host] = hostCreature;
            _creatures[(int)host.Opponent()] = guestCreature;
            Field = field;
            FieldOpen = fieldOpen;
            _fieldValues = PlayerIds.All
                .Select(_ => field.Modifiers.ToDictionary(pair => pair.Key, pair => pair.Value))
                .ToArray();
        }

        public PlayerId Host { get; }
        public PlayerId Guest => Host.Opponent();
        public FieldCard Field { get; }
        public bool FieldOpen { get; }

        /// <summary>Set to false by an effect that cancels the field condition.</summary>
        public bool FieldConditionActive { get; set; } = true;

        public IBattleComparison Comparison { get; set; } = new HigherPowerWins();

        public IReadOnlyList<IConsequenceModifier> ConsequenceModifiers => _consequenceModifiers;

        public CreatureCard CreatureOf(PlayerId player) => _creatures[(int)player];

        public BattleRole RoleOf(PlayerId player) => player == Host ? BattleRole.Host : BattleRole.Guest;

        /// <summary>Field bonuses and penalties by element, as they apply to this player's creature.</summary>
        public IReadOnlyDictionary<Element, int> FieldValuesFor(PlayerId player) => _fieldValues[(int)player];

        /// <summary>Changes every field value for this player's creature, e.g. to invert or double it.</summary>
        public void MapFieldValues(PlayerId player, Func<int, int> map)
        {
            var values = _fieldValues[(int)player];
            foreach (var element in values.Keys.ToList())
                values[element] = map(values[element]);
        }

        public int FieldModifierFor(PlayerId player) =>
            FieldOpen && _fieldValues[(int)player].TryGetValue(CreatureOf(player).Element, out var value) ? value : 0;

        public int PowerOf(PlayerId player) =>
            CreatureOf(player).Power + FieldModifierFor(player) + _abilityPower[(int)player];

        public void AddPower(PlayerId player, int amount) => _abilityPower[(int)player] += amount;

        public void AddConsequenceModifier(IConsequenceModifier modifier) =>
            _consequenceModifiers.Add(modifier ?? throw new ArgumentNullException(nameof(modifier)));
    }
}
