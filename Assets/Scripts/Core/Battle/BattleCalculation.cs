using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>
    /// Working state of a battle being resolved. Field values are kept per player, because some
    /// effects change them for one creature only.
    /// </summary>
    public sealed class BattleCalculation
    {
        private readonly CreatureCard[] _creatures = new CreatureCard[PlayerIds.All.Count];
        private readonly bool[] _ignoresFieldPenalties = new bool[PlayerIds.All.Count];
        private readonly Dictionary<Element, int>[] _fieldValues;
        private readonly int[] _abilityPower = new int[PlayerIds.All.Count];
        private readonly List<PowerContribution>[] _contributions =
            PlayerIds.All.Select(_ => new List<PowerContribution>()).ToArray();
        private readonly List<Action<BattleConsequences>> _consequenceChanges = new List<Action<BattleConsequences>>();

        internal BattleCalculation(BattleState battle)
        {
            Host = battle.Host;
            EarlierPlaced = battle.Field.EarlierOccupant;
            Field = battle.Field.Definition;
            FieldOpen = battle.Field.IsOpen;
            FieldConditionActive = battle.IsConditionActive;
            foreach (var player in PlayerIds.All)
                _creatures[(int)player] = (CreatureCard)battle.Field.OccupantOf(player).Definition;

            _fieldValues = PlayerIds.All
                .Select(_ => Field.Modifiers.ToDictionary(pair => pair.Key, pair => pair.Value))
                .ToArray();
        }

        public PlayerId Host { get; }
        public PlayerId Guest => Host.Opponent();
        public FieldCard Field { get; }
        public bool FieldOpen { get; }

        /// <summary>The field is open and its condition was not cancelled.</summary>
        public bool FieldConditionActive { get; }

        /// <summary>The player whose creature was placed on the field first; it also acts first in the battle.</summary>
        public PlayerId EarlierPlaced { get; }

        /// <summary>«Перевес»: this player wins on equal final power, whoever was placed first.</summary>
        public PlayerId? TieWinner { get; set; }

        // TODO(rules): «Штормовой натиск» gives the first battle turn to the creature placed later; equal power
        // still goes to the creature placed earlier, as the rules word it. See docs/decisions.md.
        /// <summary>
        /// Who wins on equal final power (docs/rules.md §6): the creature placed on the field earlier, unless an
        /// ability («Перевес») gives the win to its owner. The only place of this rule.
        /// </summary>
        public PlayerId EqualPowerWinner => TieWinner ?? EarlierPlaced;

        public IBattleComparison Comparison { get; set; } = new HigherPowerWins();

        internal IReadOnlyList<Action<BattleConsequences>> ConsequenceChanges => _consequenceChanges;

        /// <summary>The ability whose effects are being applied; recorded with every power change.</summary>
        internal CardInstance CurrentSource { get; set; }

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

        /// <summary>
        /// The field's penalties do not act on this player's creature («Рассвет»): whatever the value ends up
        /// after step 1 and in any play order, so a bonus turned into a penalty («Зеркало бонусов») is ignored too.
        /// </summary>
        public void IgnoreFieldPenalties(PlayerId player) => _ignoresFieldPenalties[(int)player] = true;

        public int FieldModifierFor(PlayerId player)
        {
            if (!FieldOpen || !_fieldValues[(int)player].TryGetValue(CreatureOf(player).Element, out var value))
                return 0;
            return _ignoresFieldPenalties[(int)player] ? Math.Max(value, 0) : value;
        }

        public int PowerOf(PlayerId player) =>
            CreatureOf(player).Power + FieldModifierFor(player) + _abilityPower[(int)player];

        public void AddPower(PlayerId player, int amount)
        {
            _abilityPower[(int)player] += amount;
            _contributions[(int)player].Add(new PowerContribution(CardRef.From(CurrentSource), amount));
        }

        /// <summary>The player's power by the steps of the count, as it stands now.</summary>
        public PowerBreakdown BreakdownOf(PlayerId player) =>
            new PowerBreakdown(
                CreatureOf(player).Power,
                FieldModifierFor(player),
                _contributions[(int)player].ToList(),
                PowerOf(player));

        /// <summary>Registers a change to the consequences, applied after the rules table.</summary>
        public void ModifyConsequences(Action<BattleConsequences> change) =>
            _consequenceChanges.Add(change ?? throw new ArgumentNullException(nameof(change)));
    }
}
