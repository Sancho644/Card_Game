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
            ActivePlayer = state.ActivePlayer;
            CoinTossWinner = state.CoinTossWinner;
            Result = state.Result;
            Hand = state.Player(viewer).Hand.Select(CardRef.From).ToList();
            OpponentHandCount = state.Player(viewer.Opponent()).Hand.Count;
            Fields = state.Fields.Select(field => new FieldView(field, viewer)).ToList();
            Battle = state.Battle == null ? null : new BattleView(state.Battle);

            _wins = PlayerIds.All.Select(id => state.Player(id).Wins).ToArray();
            _discards = PlayerIds.All.Select(id => Refs(state.Player(id).Discard)).ToArray();
            _outOfGame = PlayerIds.All.Select(id => Refs(state.Player(id).OutOfGame)).ToArray();
        }

        public PlayerId Viewer { get; }
        public MatchPhase Phase { get; }
        public PlayerId ActivePlayer { get; }
        public PlayerId? CoinTossWinner { get; }
        public MatchResult Result { get; }
        public IReadOnlyList<CardRef> Hand { get; }
        public int OpponentHandCount { get; }
        public IReadOnlyList<FieldView> Fields { get; }
        public BattleView Battle { get; }

        public int Wins(PlayerId player) => _wins[(int)player];
        public IReadOnlyList<CardRef> Discard(PlayerId player) => _discards[(int)player];
        public IReadOnlyList<CardRef> OutOfGame(PlayerId player) => _outOfGame[(int)player];
        public FieldView Field(int fieldId) => Fields.FirstOrDefault(field => field.Id == fieldId);

        private static IReadOnlyList<CardRef> Refs(IEnumerable<CardInstance> cards) =>
            cards.Select(CardRef.From).ToList();
    }

    public sealed class FieldView
    {
        private readonly CardRef[] _occupants;

        internal FieldView(FieldSlot field, PlayerId viewer)
        {
            Id = field.Id;
            Owner = field.Owner;
            IsOpen = field.IsOpen;
            IsRemoved = field.IsRemoved;
            // A closed field removed after a battle stays hidden from the opponent (docs/rules.md §6).
            CardId = field.IsOpen || field.Owner == viewer ? field.Definition.Id : null;
            _occupants = PlayerIds.All.Select(id => CardRef.From(field.OccupantOf(id))).ToArray();
        }

        public int Id { get; }
        public PlayerId Owner { get; }

        /// <summary>Card id, or null when the viewer may not see it.</summary>
        public string CardId { get; }

        public bool IsOpen { get; }
        public bool IsRemoved { get; }

        /// <summary>Creatures stand face up, so both players see them.</summary>
        public CardRef Occupant(PlayerId player) => _occupants[(int)player];
    }

    public sealed class BattleView
    {
        private readonly bool[] _ready;

        internal BattleView(BattleState battle)
        {
            FieldId = battle.Field.Id;
            Host = battle.Host;
            Guest = battle.Guest;
            CurrentActor = battle.CurrentActor;
            _ready = PlayerIds.All.Select(battle.IsReady).ToArray();
        }

        public int FieldId { get; }
        public PlayerId Host { get; }
        public PlayerId Guest { get; }
        public PlayerId CurrentActor { get; }

        public bool IsReady(PlayerId player) => _ready[(int)player];
    }
}
