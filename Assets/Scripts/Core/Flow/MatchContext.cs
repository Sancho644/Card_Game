using Game.Core.Battle;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>Everything a command handler needs. Composition root of the engine internals.</summary>
    internal sealed class MatchContext
    {
        public MatchContext(MatchState state, GameRules rules, IRandom random)
        {
            State = state;
            Rules = rules;
            Random = random;
            Events = new EventLog();
            Turns = new TurnFlow(state, Events);
            PowerCounter = new BattlePowerCounter();
            Resolver = new BattleResolver(PowerCounter, new ConsequenceTable(rules.DrawRule));
            Battles = new BattleFlow(state, rules, Events, Turns, Resolver);
            Abilities = new AbilityFlow(this);
        }

        public MatchState State { get; }
        public GameRules Rules { get; }
        public IRandom Random { get; }
        public EventLog Events { get; }
        public TurnFlow Turns { get; }
        public BattlePowerCounter PowerCounter { get; }
        public BattleResolver Resolver { get; }
        public BattleFlow Battles { get; }
        public AbilityFlow Abilities { get; }
    }
}
