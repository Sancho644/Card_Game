using System;
using System.Collections.Generic;
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
    /// Working state of a battle being resolved. Field modifiers come first, then ability
    /// effects in play order; a later effect may overwrite what an earlier one set.
    /// </summary>
    public sealed class BattleCalculation
    {
        private readonly CreatureCard[] _creatures = new CreatureCard[PlayerIds.All.Count];
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
            FieldModifiers = field.Modifiers;
        }

        public PlayerId Host { get; }
        public PlayerId Guest => Host.Opponent();
        public FieldCard Field { get; }
        public bool FieldOpen { get; }

        /// <summary>Bonuses and penalties in force. Effects may replace them, e.g. to invert or double.</summary>
        public IReadOnlyDictionary<Element, int> FieldModifiers { get; set; }

        /// <summary>Set to false by an effect that cancels the field condition.</summary>
        public bool FieldConditionActive { get; set; } = true;

        public IBattleComparison Comparison { get; set; } = new HigherPowerWins();

        public IReadOnlyList<IConsequenceModifier> ConsequenceModifiers => _consequenceModifiers;

        public CreatureCard CreatureOf(PlayerId player) => _creatures[(int)player];

        public BattleRole RoleOf(PlayerId player) => player == Host ? BattleRole.Host : BattleRole.Guest;

        public int FieldModifierFor(PlayerId player) =>
            FieldOpen && FieldModifiers.TryGetValue(CreatureOf(player).Element, out var value) ? value : 0;

        public int PowerOf(PlayerId player) =>
            CreatureOf(player).Power + FieldModifierFor(player) + _abilityPower[(int)player];

        public void AddPower(PlayerId player, int amount) => _abilityPower[(int)player] += amount;

        public void AddConsequenceModifier(IConsequenceModifier modifier) =>
            _consequenceModifiers.Add(modifier ?? throw new ArgumentNullException(nameof(modifier)));
    }
}
