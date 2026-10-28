using System.Collections.Generic;
using System.Linq;
using Game.Core.Decks;
using Game.Core.State;

namespace Game.Simulation
{
    /// <summary>What happened in one simulated match. The seed and the options are enough to replay it.</summary>
    public sealed class GameRecord
    {
        private readonly DeckList[] _decks;
        private readonly string[] _profiles;
        private readonly Dictionary<string, int>[] _cardUses =
            { new Dictionary<string, int>(), new Dictionary<string, int>() };
        private readonly List<BattleRecord> _battles = new List<BattleRecord>();

        internal GameRecord(int index, ulong seed, DeckList player1Deck, DeckList player2Deck, string player1Profile, string player2Profile)
        {
            Index = index;
            Seed = seed;
            _decks = new[] { player1Deck, player2Deck };
            _profiles = new[] { player1Profile, player2Profile };
        }

        public int Index { get; }
        public ulong Seed { get; }
        public PlayerId? FirstPlayer { get; internal set; }
        public bool Ended { get; internal set; }
        public PlayerId? Winner { get; internal set; }
        public MatchEndReason EndReason { get; internal set; }
        public int Turns { get; internal set; }
        public int Skips { get; internal set; }
        public int Challenges { get; internal set; }
        public int FieldOpenings { get; internal set; }

        /// <summary>The match did not end within the turn limit, or nobody could act.</summary>
        public bool Hung { get; internal set; }

        /// <summary>An exception or a rejected AI command; null if none.</summary>
        public string Error { get; internal set; }

        public bool Completed => Ended && !Hung && Error == null;
        public IReadOnlyList<BattleRecord> Battles => _battles;

        public DeckList DeckOf(PlayerId player) => _decks[(int)player];
        public string ProfileOf(PlayerId player) => _profiles[(int)player];

        /// <summary>How many times the player used the card: an ability played, a creature or a field in a battle.</summary>
        public int UsesOf(PlayerId player, string cardId) =>
            _cardUses[(int)player].TryGetValue(cardId, out var uses) ? uses : 0;

        public IEnumerable<string> UsedCards(PlayerId player) => _cardUses[(int)player].Keys.OrderBy(id => id);

        internal void AddBattle(BattleRecord battle) => _battles.Add(battle);

        internal void CountUse(PlayerId player, string cardId)
        {
            if (cardId == null)
                return;
            var uses = _cardUses[(int)player];
            uses[cardId] = UsesOf(player, cardId) + 1;
        }
    }
}
