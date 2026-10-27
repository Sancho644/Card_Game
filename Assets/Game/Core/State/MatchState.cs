using System.Collections.Generic;

namespace Game.Core.State
{
    public enum MatchPhase
    {
        NotStarted,
        ChoosingTurnOrder,
        Turn,
        Battle,

        /// <summary>A player must choose a card (see <see cref="MatchState.PendingChoice"/>).</summary>
        AwaitingChoice,

        Ended,
    }

    public sealed class MatchResult
    {
        internal MatchResult(PlayerId? winner)
        {
            Winner = winner;
        }

        public PlayerId? Winner { get; }
        public bool IsDraw => Winner == null;
    }

    public enum ChoicePurpose
    {
        /// <summary>Take one of the options from the discard pile into the hand.</summary>
        TakeAbilityFromDiscard,
    }

    public sealed class PendingChoice
    {
        internal PendingChoice(PlayerId player, ChoicePurpose purpose, IReadOnlyList<CardInstance> options)
        {
            Player = player;
            Purpose = purpose;
            Options = options;
        }

        public PlayerId Player { get; }
        public ChoicePurpose Purpose { get; }
        public IReadOnlyList<CardInstance> Options { get; }
    }

    /// <summary>
    /// Full authoritative match state. Only the engine changes it; clients should use
    /// <see cref="PlayerView"/> and events, which respect hidden information.
    /// </summary>
    public sealed class MatchState
    {
        private readonly PlayerState[] _players;
        private readonly List<FieldSlot> _fields;
        private int _placementCounter;

        internal MatchState(PlayerState[] players, List<FieldSlot> fields)
        {
            _players = players;
            _fields = fields;
        }

        public MatchPhase Phase { get; internal set; } = MatchPhase.NotStarted;
        public PlayerId? CoinTossWinner { get; internal set; }
        public PlayerId ActivePlayer { get; internal set; }
        public bool MainActionDone { get; internal set; }
        public bool BattleHappenedThisTurn { get; internal set; }

        /// <summary>Main actions skipped in a row, across both players.</summary>
        public int ConsecutiveSkips { get; internal set; }

        public BattleState Battle { get; internal set; }
        public PendingChoice PendingChoice { get; internal set; }
        public MatchResult Result { get; internal set; }
        public IReadOnlyList<FieldSlot> Fields => _fields;

        public PlayerState Player(PlayerId id) => _players[(int)id];

        public FieldSlot FindField(int fieldId) => _fields.Find(field => field.Id == fieldId);

        public FieldSlot FindFieldWith(PlayerId owner, int creatureId) =>
            _fields.Find(field => field.OccupantOf(owner)?.Id == creatureId);

        internal int NextPlacementOrder() => ++_placementCounter;
    }
}
