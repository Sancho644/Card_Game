using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Forecast;
using Game.Core.Randomness;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// What the opponent may hold, by the deck rules: every deck slot whose card was seen is settled, the rest
    /// can be any card fitting the slot. Unseen creatures and abilities are in the hand, unseen field cards are
    /// the closed fields. Built for one decision from the view and the memory only.
    /// </summary>
    internal sealed class OpponentModel
    {
        private readonly PlayerView _view;
        private readonly CardCatalog _catalog;
        private readonly DeckSlots _slots;
        private readonly List<string> _knownCardIds = new List<string>();
        private readonly List<string> _knownInHand = new List<string>();
        private readonly Dictionary<int, string> _knownHiddenFields = new Dictionary<int, string>();
        private readonly List<int> _unknownFieldIds = new List<int>();
        private readonly List<DeckSlot> _openSlots;

        public OpponentModel(PlayerView view, AiMemory memory, CardCatalog catalog, DeckSlots slots)
        {
            _view = view;
            _catalog = catalog;
            _slots = slots;
            var opponent = view.Viewer.Opponent();

            var visible = new HashSet<int>(view.Fields
                .Select(field => field.Occupant(opponent))
                .Where(card => card != null)
                .Concat(view.Discard(opponent))
                .Concat(view.OutOfGame(opponent))
                .Select(card => card.InstanceId));
            foreach (var pair in memory.OpponentCards.OrderBy(pair => pair.Key))
            {
                _knownCardIds.Add(pair.Value);
                if (!visible.Contains(pair.Key))
                    _knownInHand.Add(pair.Value);
            }

            foreach (var field in view.Fields.Where(field => field.Owner == opponent).OrderBy(field => field.Id))
            {
                if (field.CardId != null)
                    _knownCardIds.Add(field.CardId);
                else if (memory.Fields.TryGetValue(field.Id, out var cardId))
                {
                    _knownCardIds.Add(cardId);
                    _knownHiddenFields[field.Id] = cardId;
                }
                else
                    _unknownFieldIds.Add(field.Id);
            }

            _openSlots = OpenSlots();
        }

        /// <summary>Deck slots whose card the AI has not seen yet.</summary>
        public int UnknownCardCount => _openSlots.Count;

        public HiddenCardsGuess Sample(IRandom random)
        {
            var chosen = new List<string>(_knownCardIds);
            var hand = new List<string>(_knownInHand);
            var fieldPool = new List<string>();
            foreach (var slot in _openSlots)
            {
                var options = _slots.Candidates(slot).Where(card => _slots.FitsWith(card, chosen)).ToList();
                if (options.Count == 0)
                    options = _slots.Candidates(slot).ToList();
                if (options.Count == 0)
                    continue;

                var card = options[random.Next(options.Count)];
                chosen.Add(card.Id);
                (slot.Type == CardType.Field ? fieldPool : hand).Add(card.Id);
            }

            return new HiddenCardsGuess(FitHand(hand, random), AssignFields(fieldPool, random));
        }

        /// <summary>Settles one slot per seen card; the slots left are open.</summary>
        private List<DeckSlot> OpenSlots()
        {
            var open = _slots.Slots.ToList();
            foreach (var cardId in _knownCardIds)
            {
                if (!_catalog.TryGet(cardId, out var card))
                    continue;
                var index = open.FindIndex(slot => slot.Accepts(card));
                if (index >= 0)
                    open.RemoveAt(index);
            }

            return open;
        }

        /// <summary>The guess must have exactly as many cards as the opponent holds; a mismatch means a memory gap.</summary>
        private List<string> FitHand(List<string> hand, IRandom random)
        {
            var size = _view.OpponentHandCount;
            if (hand.Count > size)
                hand.RemoveRange(size, hand.Count - size);

            var abilities = _catalog.All.Where(card => card.Type == CardType.Ability && !((AbilityCard)card).IsSignature).ToList();
            while (hand.Count < size && abilities.Count > 0)
                hand.Add(abilities[random.Next(abilities.Count)].Id);
            return hand;
        }

        private Dictionary<int, string> AssignFields(List<string> pool, IRandom random)
        {
            var fields = new Dictionary<int, string>(_knownHiddenFields);
            var allFields = _catalog.All.Where(card => card.Type == CardType.Field).ToList();
            foreach (var fieldId in _unknownFieldIds)
            {
                string cardId;
                if (pool.Count > 0)
                {
                    var index = random.Next(pool.Count);
                    cardId = pool[index];
                    pool.RemoveAt(index);
                }
                else
                    cardId = allFields[random.Next(allFields.Count)].Id;

                fields[fieldId] = cardId;
            }

            return fields;
        }
    }
}
