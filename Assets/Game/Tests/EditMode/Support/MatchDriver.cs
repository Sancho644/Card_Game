using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.Rules;
using Game.Core.State;
using NUnit.Framework;

namespace Game.Core.Tests
{
    /// <summary>Drives a match in tests: sends commands by card id, asserts acceptance, keeps the event log.</summary>
    internal sealed class MatchDriver
    {
        public const PlayerId P1 = PlayerId.Player1;
        public const PlayerId P2 = PlayerId.Player2;

        private readonly List<GameEvent> _log = new List<GameEvent>();

        private MatchDriver(MatchEngine engine)
        {
            Engine = engine;
        }

        public MatchEngine Engine { get; }
        public MatchState State => Engine.State;
        public IReadOnlyList<GameEvent> Log => _log;

        /// <summary>Creates and starts a match; the turn order is not chosen yet.</summary>
        public static MatchDriver Create(
            ulong seed = 1,
            GameRules rules = null,
            DeckList player1Deck = null,
            DeckList player2Deck = null)
        {
            var setup = new MatchSetup(player1Deck ?? TestCards.Deck(), player2Deck ?? TestCards.Deck());
            var engine = new MatchEngine(setup, TestCards.Catalog(), rules ?? GameRules.Default, new XorShiftRandom(seed));
            var driver = new MatchDriver(engine);
            driver._log.AddRange(engine.Start());
            return driver;
        }

        /// <summary>Creates a match where <paramref name="first"/> takes the first turn.</summary>
        public static MatchDriver Begin(
            PlayerId first,
            GameRules rules = null,
            DeckList player1Deck = null,
            DeckList player2Deck = null)
        {
            var driver = Create(rules: rules, player1Deck: player1Deck, player2Deck: player2Deck);
            var coinWinner = driver.State.CoinTossWinner.Value;
            driver.Ok(new ChooseTurnOrder(coinWinner, goFirst: coinWinner == first));
            return driver;
        }

        /// <summary>
        /// Battle on Player1's closed Forge: Player1 placed first, Player2 placed second on its own turn.
        /// Player1 (the host) acts first.
        /// </summary>
        public static MatchDriver BattleOnP1Forge(
            string hostCreature,
            string guestCreature,
            GameRules rules = null,
            DeckList player1Deck = null,
            DeckList player2Deck = null) =>
            BattleOnP1Field(TestCards.Forge, hostCreature, guestCreature, rules, player1Deck, player2Deck);

        /// <summary>The same as <see cref="BattleOnP1Forge"/>, on another field from Player1's deck.</summary>
        public static MatchDriver BattleOnP1Field(
            string field,
            string hostCreature,
            string guestCreature,
            GameRules rules = null,
            DeckList player1Deck = null,
            DeckList player2Deck = null)
        {
            var driver = Begin(P1, rules, player1Deck, player2Deck);
            driver.Place(P1, hostCreature, P1, field);
            driver.EndTurn(P1);
            driver.Place(P2, guestCreature, P1, field);
            return driver;
        }

        public CommandResult Do(GameCommand command)
        {
            var result = Engine.Execute(command);
            if (result.IsAccepted)
                _log.AddRange(result.Events);
            return result;
        }

        public void Ok(GameCommand command)
        {
            var result = Do(command);
            Assert.IsTrue(result.IsAccepted, $"{command.GetType().Name} by {command.Player} rejected: {result.Reason}");
        }

        public void Rejected(GameCommand command, RejectReason expected) =>
            Assert.AreEqual(expected, Do(command).Reason);

        public void Place(PlayerId player, string creature, PlayerId fieldOwner, string field) =>
            Ok(new PlaceCreature(player, Id(player, creature), Field(fieldOwner, field).Id));

        public void Challenge(PlayerId player, string creature, PlayerId fieldOwner, string field) =>
            Ok(new Challenge(player, Id(player, creature), Field(fieldOwner, field).Id));

        public void Play(PlayerId player, string ability) => Ok(new PlayAbility(player, Id(player, ability)));

        public void Open(PlayerId player) => Ok(new OpenField(player));

        public void Ready(PlayerId player) => Ok(new Ready(player));

        public void EndTurn(PlayerId player) => Ok(new EndTurn(player));

        /// <summary>Instance id of the player's card with this card id, wherever it is.</summary>
        public int Id(PlayerId owner, string cardId) => AllCards(owner).First(card => card.Definition.Id == cardId).Id;

        public FieldSlot Field(PlayerId owner, string cardId) =>
            State.Fields.First(field => field.Owner == owner && field.Definition.Id == cardId);

        public bool InHand(PlayerId owner, string cardId) => Contains(State.Player(owner).Hand, cardId);

        public bool InDiscard(PlayerId owner, string cardId) => Contains(State.Player(owner).Discard, cardId);

        public bool IsOut(PlayerId owner, string cardId) => Contains(State.Player(owner).OutOfGame, cardId);

        public int Wins(PlayerId player) => State.Player(player).Wins;

        public T Last<T>() where T : GameEvent => _log.OfType<T>().Last();

        private IEnumerable<CardInstance> AllCards(PlayerId owner)
        {
            var player = State.Player(owner);
            return player.Hand
                .Concat(player.Discard)
                .Concat(player.OutOfGame)
                .Concat(State.Fields.Select(field => field.OccupantOf(owner)).Where(card => card != null))
                .Concat(State.Fields.Where(field => field.Owner == owner).Select(field => field.Card));
        }

        private static bool Contains(IEnumerable<CardInstance> cards, string cardId) =>
            cards.Any(card => card.Definition.Id == cardId);

        /// <summary>Rules with a custom deck composition, used to build small scenarios.</summary>
        public static GameRules RulesWithDeck(Rarity[] fields, Rarity[] creatures, Rarity[] abilities, int winsToWin = 3) =>
            new GameRules(winsToWin, 1, new DeckRules(new Dictionary<CardType, IReadOnlyList<Rarity>>
            {
                [CardType.Field] = fields,
                [CardType.Creature] = creatures,
                [CardType.Ability] = abilities,
            }));
    }
}
