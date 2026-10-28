using Game.Core.Battle;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>What the match score is worth to a player, in the AI's points.</summary>
    internal interface IMatchScoreValue
    {
        /// <summary>The value for <paramref name="me"/> now, or once the consequences of the battle in progress apply.</summary>
        double Value(MatchState state, PlayerId me, BattleConsequences after = null);
    }
}
