using Game.Core.State;

namespace Game.Core.Battle
{
    public enum CreatureFate
    {
        StaysOnField,
        ReturnsToHand,
        OutOfGame,
    }

    /// <summary>What happens after a battle. Built from the rules table, then adjusted by effects.</summary>
    public sealed class BattleConsequences
    {
        private readonly CreatureFate[] _fates = new CreatureFate[PlayerIds.All.Count];

        internal BattleConsequences(PlayerId host, PlayerId? winner, bool fieldWasOpen)
        {
            Host = host;
            Winner = winner;
            FieldWasOpen = fieldWasOpen;
        }

        public PlayerId Host { get; }
        public PlayerId Guest => Host.Opponent();
        public PlayerId? Winner { get; }
        public bool IsDraw => Winner == null;
        public bool FieldWasOpen { get; }
        public bool FieldRemoved { get; set; }
        public PlayerId? WinScoredBy { get; set; }

        public CreatureFate FateOf(PlayerId player) => _fates[(int)player];

        public void SetFate(PlayerId player, CreatureFate fate) => _fates[(int)player] = fate;
    }

    /// <summary>Lets a condition or an ability change the consequences, e.g. "the loser returns to hand".</summary>
    public interface IConsequenceModifier
    {
        void Modify(BattleConsequences consequences);
    }
}
