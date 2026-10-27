using System.Collections.Generic;
using System.Linq;

namespace Game.Core.State
{
    /// <summary>
    /// The match as one player is allowed to see it: the opponent's hand and the
    /// content of the opponent's closed field cards are hidden.
    /// </summary>
    public sealed class PlayerView
    {
        private readonly int[] _wins;
        private readonly IReadOnlyList<CardRef>[] _discards;
        private readonly IReadOnlyList<CardRef>[] _outOfGame;

        internal PlayerView(MatchState state, PlayerId viewer)
        {
            Viewer = viewer;
            Phase = state.Phase;
            AwaitingPlayer = AwaitingPlayerOf(state);
            ActivePlayer = state.ActivePlayer;
            CoinTossWinner = state.CoinTossWinner;
            Result = state.Result;
            Hand = state.Player(viewer).Hand.Select(CardRef.From).ToList();
            OpponentHandCount = state.Player(viewer.Opponent()).Hand.Count;
            Fields = state.Fields.Select(field => new FieldView(field, viewer)).ToList();
            Battle = state.Battle == null ? null : new BattleView(state.Battle);
            ChoosingPlayer = state.PendingChoice?.Player;
            ChoiceOptions = state.PendingChoice == null ? new CardRef[0] : Refs(state.PendingChoice.Options);

            _wins = PlayerIds.All.Select(id => state.Player(id).Wins).ToArray();
            _discards = PlayerIds.All.Select(id => Refs(state.Player(id).Discard)).ToArray();
            _outOfGame = PlayerIds.All.Select(id => Refs(state.Player(id).OutOfGame)).ToArray();
        }

        public PlayerId Viewer { get; }
        public MatchPhase Phase { get; }

        /// <summary>
        /// The player whose decision the match waits for: the coin toss winner, the active player,
        /// the player acting in the battle or the one choosing a card. Null before the start and after the end.
        /// </summary>
        public PlayerId? AwaitingPlayer { get; }
        public PlayerId ActivePlayer { get; }
        public PlayerId? CoinTossWinner { get; }
        public MatchResult Result { get; }
        public IReadOnlyList<CardRef> Hand { get; }
        public int OpponentHandCount { get; }
        public IReadOnlyList<FieldView> Fields { get; }
        public BattleView Battle { get; }

        /// <summary>The player who must choose a card, or null. Options come from public piles.</summary>
        public PlayerId? ChoosingPlayer { get; }

        public IReadOnlyList<CardRef> ChoiceOptions { get; }

        public int Wins(PlayerId player) => _wins[(int)player];
        public IReadOnlyList<CardRef> Discard(PlayerId player) => _discards[(int)player];
        public IReadOnlyList<CardRef> OutOfGame(PlayerId player) => _outOfGame[(int)player];
        public FieldView Field(int fieldId) => Fields.FirstOrDefault(field => field.Id == fieldId);

        private static IReadOnlyList<CardRef> Refs(IEnumerable<CardInstance> cards) =>
            cards.Select(CardRef.From).ToList();

        private static PlayerId? AwaitingPlayerOf(MatchState state)
        {
            switch (state.Phase)
            {
                case MatchPhase.ChoosingTurnOrder:
                    return state.CoinTossWinner;
                case MatchPhase.Turn:
                    return state.ActivePlayer;
                case MatchPhase.Battle:
                    return state.Battle.CurrentActor;
                case MatchPhase.AwaitingChoice:
                    return state.PendingChoice.Player;
                default:
                    return null;
            }
        }
    }
}
