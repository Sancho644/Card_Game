using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Effects;
using Game.Core.Events;
using Game.Core.Rules;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Match;
using Game.Presentation.Network;
using Game.Services.Match;
using NUnit.Framework;

namespace Game.Presentation.Tests
{
    public sealed class DiscardAudioTests
    {
        [TestCase(Rarity.Common)]
        [TestCase(Rarity.Legendary)]
        public void AcceptedLocalPlayAddsOneDiscardSoundCandidate(Rarity rarity)
        {
            var session = CreateSession(out var setup);
            session.EventsProduced += _ => { };
            session.Start(setup, 5);
            ReachBattle(session, command =>
            {
                var result = session.Submit(command);
                Assert.IsTrue(result.IsAccepted, result.Reason.ToString());
            });
            var command = Ability(session, rarity);
            var before = session.GetView(command.Player);
            var accepted = session.Submit(command);
            Assert.IsTrue(accepted.IsAccepted, accepted.Reason.ToString());
            var after = session.GetView(command.Player);

            CollectionAssert.AreEquivalent(new[] { command.AbilityId }, DiscardAudio.NewCards(accepted.Events, before, after));
            CollectionAssert.IsEmpty(DiscardAudio.NewCards(accepted.Events, after, after), "A replay is not an arrival.");

            var played = accepted.Events.OfType<AbilityPlayed>().Single();
            var forcedDiscard = new AbilityDiscarded(played.Player, played.Ability);
            CollectionAssert.AreEquivalent(new[] { command.AbilityId },
                DiscardAudio.NewCards(new GameEvent[] { forcedDiscard, forcedDiscard }, before, after));
            CollectionAssert.IsEmpty(DiscardAudio.NewCards(accepted.Events, before, before), "The resulting pile must contain the card.");
            CollectionAssert.IsEmpty(DiscardAudio.NewCards(new GameEvent[] { new AbilityCancelled(played.Player, played.Ability) }, before, after));
        }

        [Test]
        public void RejectedPlayDoesNotProduceAConfirmedDiscard()
        {
            var session = CreateSession(out var setup);
            session.EventsProduced += _ => { };
            session.Start(setup, 5);
            var before = session.GetView(PlayerId.Player1);
            var rejected = session.Submit(new PlayAbility(PlayerId.Player1, before.Hand.First().InstanceId));
            Assert.IsFalse(rejected.IsAccepted);
            CollectionAssert.IsEmpty(DiscardAudio.NewCards(rejected.Events, before, session.GetView(PlayerId.Player1)));
        }

        [TestCase(Rarity.Common)]
        [TestCase(Rarity.Legendary)]
        public void NetworkRecipientSeesExactlyOneArrivalForRepeatedDelivery(Rarity rarity)
        {
            var session = CreateSession(out var setup);
            var authority = new OnlineAuthority(session);
            var viewer = PlayerId.Player1;
            var client = new OnlineMatchSession(viewer, (_, _, _) => Assert.Fail("Unexpected client command"));
            var arrivals = new List<int>();
            var placements = new List<SoundCue>();
            var requests = new long[2];
            PlayerView previous = null;
            client.EventsProduced += batch =>
            {
                var current = client.GetView(viewer);
                arrivals.AddRange(DiscardAudio.NewCards(batch, previous, current));
                placements.AddRange(CardSoundPresenter.Arrivals(batch, previous, current).Values);
                previous = current;
            };
            authority.Updated += (player, frame) =>
            {
                if (player != viewer)
                    return;
                var copied = OnlineCodec.Decode<OnlineFrame>(OnlineCodec.Encode(frame));
                client.Receive(copied);
                client.Receive(copied);
            };
            client.Start(null, 0);
            authority.Start(setup, 5);
            ReachBattle(session, Submit);
            var play = Ability(session, rarity);
            Submit(play);
            CollectionAssert.AreEqual(new[] { play.AbilityId }, arrivals);
            CollectionAssert.AreEqual(
                new[] { SoundCue.FieldPlace, SoundCue.FieldPlace, SoundCue.CardPlace, SoundCue.CardPlace }, placements);

            void Submit(GameCommand command)
            {
                var actions = session.GetLegalActions(command.Player);
                var index = actions.ToList().FindIndex(candidate => OnlineCodec.Encode(candidate) == OnlineCodec.Encode(command));
                Assert.GreaterOrEqual(index, 0, command.GetType().Name);
                authority.Submit(command.Player, ++requests[(int)command.Player], authority.Version, index);
            }
        }

        [Test]
        public void AcceptedPlacementsUseDistinctCuesAndDoNotReplayFromTheSameSnapshot()
        {
            var session = CreateSession(out var setup);
            session.EventsProduced += _ => { };
            session.Start(setup, 5);
            var field = session.GetLegalActions(PlayerId.Player1).First();
            AssertArrival(field, SoundCue.FieldPlace);

            session.Submit(session.GetLegalActions(PlayerId.Player2).First());
            var coinWinner = session.GetView(PlayerId.Player1).CoinTossWinner.Value;
            session.Submit(new ChooseTurnOrder(coinWinner, coinWinner == PlayerId.Player1));
            var creature = session.GetLegalActions(PlayerId.Player1).OfType<PlaceCreature>().First();
            AssertArrival(creature, SoundCue.CardPlace);

            void AssertArrival(GameCommand command, SoundCue cue)
            {
                var before = PlayerIds.All.Select(session.GetView).ToArray();
                var accepted = session.Submit(command);
                Assert.IsTrue(accepted.IsAccepted, accepted.Reason.ToString());
                foreach (var viewer in PlayerIds.All)
                {
                    var after = session.GetView(viewer);
                    var repeated = accepted.Events.Concat(accepted.Events).ToArray();
                    CollectionAssert.AreEqual(new[] { cue },
                        CardSoundPresenter.Arrivals(repeated, before[(int)viewer], after).Values);
                    CollectionAssert.IsEmpty(CardSoundPresenter.Arrivals(repeated, after, after));
                    CollectionAssert.IsEmpty(CardSoundPresenter.Arrivals(repeated, before[(int)viewer], before[(int)viewer]));
                }
            }
        }

        private static LocalMatchSession CreateSession(out MatchSetup setup)
        {
            var cards = new List<CardDefinition>();
            foreach (var rarity in new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary })
            {
                cards.Add(new FieldCard("field-" + rarity, rarity, new Dictionary<Element, int>()));
                cards.Add(new CreatureCard("creature-" + rarity, rarity, Element.Fire, 180));
                cards.Add(new AbilityCard("ability-" + rarity, rarity, AbilityTiming.InBattle, new[] { new PowerBonusEffect(40) }));
            }
            var deck = new DeckList(cards.Select(card => card.Id));
            setup = new MatchSetup(deck, deck);
            return new LocalMatchSession(new CardCatalog(cards), GameRules.Default);
        }

        private static void ReachBattle(LocalMatchSession session, System.Action<GameCommand> submit)
        {
            foreach (var player in PlayerIds.All)
                submit(session.GetLegalActions(player).First());
            var coinWinner = session.GetView(PlayerId.Player1).CoinTossWinner.Value;
            submit(new ChooseTurnOrder(coinWinner, coinWinner == PlayerId.Player1));
            var host = session.GetLegalActions(PlayerId.Player1).OfType<PlaceCreature>().First();
            submit(host);
            submit(new EndTurn(PlayerId.Player1));
            submit(session.GetLegalActions(PlayerId.Player2).OfType<PlaceCreature>().First(action => action.FieldId == host.FieldId));
        }

        private static PlayAbility Ability(LocalMatchSession session, Rarity rarity)
        {
            var player = session.GetView(PlayerId.Player1).AwaitingPlayer.Value;
            var id = session.GetView(player).Hand.Single(card => card.CardId == "ability-" + rarity).InstanceId;
            return session.GetLegalActions(player).OfType<PlayAbility>().Single(action => action.AbilityId == id);
        }
    }
}
