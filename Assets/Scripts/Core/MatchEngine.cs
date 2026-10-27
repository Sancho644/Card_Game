using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Commands.Handlers;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;

namespace Game.Core
{
    /// <summary>
    /// Rules engine for one match. Deterministic: the same setup, random seed and commands
    /// always produce the same events. State changes only through <see cref="Execute"/>.
    /// </summary>
    public sealed class MatchEngine
    {
        private readonly MatchContext _context;
        private readonly Dictionary<Type, ICommandHandler> _handlers;
        private readonly LegalActionFinder _legalActions;

        public MatchEngine(MatchSetup setup, CardCatalog catalog, GameRules rules, IRandom random)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (random == null) throw new ArgumentNullException(nameof(random));

            ValidateDecks(setup, catalog, rules);
            _context = new MatchContext(CreateState(setup, catalog), rules, random);
            _handlers = CreateHandlers().ToDictionary(handler => handler.CommandType);
            _legalActions = new LegalActionFinder(_context.State, Validate);
        }

        /// <summary>Authoritative state, for the server and tests. Clients should use <see cref="GetView"/>.</summary>
        public MatchState State => _context.State;

        /// <summary>Deals the cards and tosses the coin.</summary>
        public IReadOnlyList<GameEvent> Start()
        {
            var state = State;
            if (state.Phase != MatchPhase.NotStarted)
                throw new InvalidOperationException("The match has already started.");

            foreach (var id in PlayerIds.All)
            {
                var fieldIds = state.Fields.Where(field => field.Owner == id).Select(field => field.Id).ToList();
                _context.Events.Emit(new FieldsDealt(id, fieldIds));
                _context.Events.Emit(new HandDealt(id, state.Player(id).Hand.Select(CardRef.From).ToList()));
            }

            var coinWinner = PlayerIds.All[_context.Random.Next(PlayerIds.All.Count)];
            state.CoinTossWinner = coinWinner;
            state.Phase = MatchPhase.ChoosingTurnOrder;
            _context.Events.Emit(new CoinTossed(coinWinner));
            return _context.Events.Drain();
        }

        public CommandResult Execute(GameCommand command)
        {
            var reason = Validate(command);
            if (reason != RejectReason.None)
                return CommandResult.Rejected(reason);

            _handlers[command.GetType()].Apply(command, _context);
            return CommandResult.Accepted(_context.Events.Drain());
        }

        /// <summary>Checks a command without changing anything. <see cref="RejectReason.None"/> means it would be accepted.</summary>
        public RejectReason Validate(GameCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (State.Phase == MatchPhase.Ended)
                return RejectReason.MatchOver;
            if (State.Phase == MatchPhase.NotStarted)
                return RejectReason.WrongPhase;
            return _handlers.TryGetValue(command.GetType(), out var handler)
                ? handler.Validate(command, _context)
                : RejectReason.UnknownCommand;
        }

        /// <summary>Every command the player may send right now. Presentation and AI choose only from this list.</summary>
        public IReadOnlyList<GameCommand> GetLegalActions(PlayerId player) => _legalActions.Find(player);

        public PlayerView GetView(PlayerId viewer) => new PlayerView(State, viewer);

        /// <summary>
        /// The current battle's powers by the same count as the final result, or null outside a battle.
        /// Public information, the same for both players.
        /// </summary>
        public BattlePower GetBattlePower() => State.Battle == null ? null : _context.PowerCounter.Power(State.Battle);

        private static IEnumerable<ICommandHandler> CreateHandlers() => new ICommandHandler[]
        {
            new ChooseTurnOrderHandler(),
            new PlaceCreatureHandler(),
            new ChallengeHandler(),
            new OpenFieldHandler(),
            new PlayAbilityHandler(),
            new ReadyHandler(),
            new EndTurnHandler(),
            new ChooseCardHandler(),
        };

        private static void ValidateDecks(MatchSetup setup, CardCatalog catalog, GameRules rules)
        {
            var validator = new DeckValidator(catalog, rules.Deck);
            foreach (var id in PlayerIds.All)
            {
                var result = validator.Validate(setup.DeckOf(id));
                if (!result.IsValid)
                    throw new ArgumentException($"Invalid deck for {id}: {string.Join(", ", result.Errors)}", nameof(setup));
            }
        }

        /// <summary>Field cards go on the table face down, creatures and abilities to the hand.</summary>
        private static MatchState CreateState(MatchSetup setup, CardCatalog catalog)
        {
            var nextInstanceId = 1;
            var players = new PlayerState[PlayerIds.All.Count];
            var fields = new List<FieldSlot>();

            foreach (var id in PlayerIds.All)
            {
                var player = new PlayerState(id);
                foreach (var cardId in setup.DeckOf(id).CardIds)
                {
                    var card = new CardInstance(nextInstanceId++, id, catalog.Get(cardId));
                    if (card.Type == CardType.Field)
                        fields.Add(new FieldSlot(card));
                    else
                        player.AddToHand(card);
                }

                players[(int)id] = player;
            }

            return new MatchState(players, fields);
        }
    }
}
