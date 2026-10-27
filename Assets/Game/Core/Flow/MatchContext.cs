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
            Battles = new BattleFlow(state, rules, Events, Turns, new BattleResolver(new ConsequenceTable()));
            Abilities = new AbilityFlow(this);
        }

        public MatchState State { get; }
        public GameRules Rules { get; }
        public IRandom Random { get; }
        public EventLog Events { get; }
        public TurnFlow Turns { get; }
        public BattleFlow Battles { get; }
        public AbilityFlow Abilities { get; }
    }
}
