using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Forecast
{
    /// <summary>Rebuilds a full match state from a player view and a guess about the hidden cards. Single use.</summary>
    internal sealed class ViewStateRestorer
    {
        private readonly CardCatalog _catalog;
        private readonly PlayerView _view;
        private readonly HiddenCardsGuess _guess;
        private readonly Dictionary<int, CardInstance> _instances = new Dictionary<int, CardInstance>();

        public ViewStateRestorer(CardCatalog catalog, PlayerView view, HiddenCardsGuess guess)
        {
            _catalog = catalog;
            _view = view;
            _guess = guess;
        }

        public MatchState Restore()
        {
            var players = PlayerIds.All.Select(RestorePlayer).ToArray();
            AddGuessedOpponentHand(players[(int)_view.Viewer.Opponent()]);

            var fields = _view.Fields.Select(RestoreField).ToList();
            var state = new MatchState(players, fields)
            {
                Phase = _view.Phase,
                CoinTossWinner = _view.CoinTossWinner,
                ActivePlayer = _view.ActivePlayer,
                MainActionDone = _view.MainActionDone,
                BattleHappenedThisTurn = _view.BattleHappenedThisTurn,
                ConsecutiveSkips = _view.ConsecutiveSkips,
                Result = _view.Result == null ? null : new MatchResult(_view.Result.Winner, _view.Result.Reason),
            };
            state.RestorePlacementCounter(_view.Fields.SelectMany(field => PlayerIds.All.Select(field.PlacementOrder)).DefaultIfEmpty().Max());

            if (_view.Battle != null)
                state.Battle = RestoreBattle(fields);
            if (_view.ChoosingPlayer != null)
                state.PendingChoice = RestoreChoice(_view.ChoosingPlayer.Value);
            return state;
        }

        private PlayerState RestorePlayer(PlayerId id)
        {
            var player = new PlayerState(id)
            {
                Wins = _view.Wins(id),
                LegendariesPlayedThisTurn = _view.LegendariesPlayedThisTurn(id),
            };
            if (id == _view.Viewer)
            {
                foreach (var card in _view.Hand)
                    player.AddToHand(Instance(id, card));
            }

            foreach (var card in _view.Discard(id))
                player.AddToDiscard(Instance(id, card));
            foreach (var card in _view.OutOfGame(id))
                player.AddToOutOfGame(Instance(id, card));
            return player;
        }

        /// <summary>The opponent's hand gets fresh instance ids: the real ones are hidden too.</summary>
        private void AddGuessedOpponentHand(PlayerState opponent)
        {
            if (_guess.OpponentHand.Count != _view.OpponentHandCount)
                throw new ArgumentException(
                    $"The guess has {_guess.OpponentHand.Count} cards for a hand of {_view.OpponentHandCount}.");

            var nextId = MaxVisibleInstanceId() + 1;
            foreach (var cardId in _guess.OpponentHand)
                opponent.AddToHand(new CardInstance(nextId++, opponent.Id, _catalog.Get(cardId)));
        }

        private FieldSlot RestoreField(FieldView view)
        {
            var cardId = view.CardId;
            if (cardId == null && !_guess.HiddenFields.TryGetValue(view.Id, out cardId))
                throw new ArgumentException($"The guess has no card for hidden field #{view.Id}.");

            var slot = new FieldSlot(Instance(view.Owner, new CardRef(view.Id, cardId)))
            {
                IsOpen = view.IsOpen,
                IsRemoved = view.IsRemoved,
                IsLocked = view.IsLocked,
            };
            foreach (var id in PlayerIds.All)
            {
                var occupant = view.Occupant(id);
                if (occupant != null)
                    slot.Occupy(id, Instance(id, occupant), view.PlacementOrder(id));
            }

            return slot;
        }

        private BattleState RestoreBattle(IReadOnlyList<FieldSlot> fields)
        {
            var view = _view.Battle;
            var battle = new BattleState(fields.First(field => field.Id == view.FieldId), view.CurrentActor);
            foreach (var id in PlayerIds.All)
                battle.SetReady(id, view.IsReady(id));
            foreach (var played in view.PlayedAbilities)
            {
                battle.AddPlayedAbility(new PlayedAbility(played.Player, Instance(played.Player, played.Card))
                {
                    IsCancelled = played.IsCancelled,
                });
            }

            return battle;
        }

        private PendingChoice RestoreChoice(PlayerId player) =>
            new PendingChoice(
                player,
                _view.ChoicePurpose ?? throw new ArgumentException("A pending choice without a purpose."),
                _view.ChoiceOptions.Select(option => Instance(player, option)).ToList());

        /// <summary>One instance per id, so a card shared by two places (discard and battle) stays one object.</summary>
        private CardInstance Instance(PlayerId owner, CardRef card)
        {
            if (!_instances.TryGetValue(card.InstanceId, out var instance))
            {
                instance = new CardInstance(card.InstanceId, owner, _catalog.Get(card.CardId));
                _instances.Add(card.InstanceId, instance);
            }

            return instance;
        }

        private int MaxVisibleInstanceId()
        {
            var ids = _view.Hand.Select(card => card.InstanceId)
                .Concat(PlayerIds.All.SelectMany(id => _view.Discard(id).Concat(_view.OutOfGame(id))).Select(card => card.InstanceId))
                .Concat(_view.Fields.Select(field => field.Id))
                .Concat(_view.Fields.SelectMany(field => PlayerIds.All.Select(field.Occupant)).Where(card => card != null).Select(card => card.InstanceId));
            return ids.DefaultIfEmpty().Max();
        }
    }
}
