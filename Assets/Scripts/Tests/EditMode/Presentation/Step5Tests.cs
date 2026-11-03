using System;
using Game.Core.Battle;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Match;
using NUnit.Framework;
using UnityEditor;

namespace Game.Presentation.Tests
{
    public class Step5Tests
    {
        [TestCase(PlayerId.Player1, PlayerId.Player1, false, UiTextKeys.EndVictory)]
        [TestCase(PlayerId.Player2, PlayerId.Player1, false, UiTextKeys.EndDefeat)]
        [TestCase(PlayerId.Player2, PlayerId.Player2, false, UiTextKeys.EndVictory)]
        [TestCase(PlayerId.Player1, PlayerId.Player2, true, UiTextKeys.EndWinner)]
        public void EndTitle_UsesLocalPerspectiveOrHotSeat(PlayerId winner, PlayerId viewer, bool hotSeat, string expected)
        {
            Assert.AreEqual(expected, MatchEndText.TitleKey(winner, viewer, hotSeat));
        }

        [Test]
        public void Draw_HasNoWinner_InEitherMode()
        {
            Assert.AreEqual(UiTextKeys.EndDraw, MatchEndText.TitleKey(null, PlayerId.Player1, false));
            Assert.AreEqual(UiTextKeys.EndDraw, MatchEndText.TitleKey(null, PlayerId.Player2, true));
        }

        [Test]
        public void AllEndReasons_HaveLocalizedTextInProject()
        {
            var texts = AssetDatabase.LoadAssetAtPath<UiTexts>("Assets/Data/Config/UiTexts.asset");
            foreach (MatchEndReason reason in Enum.GetValues(typeof(MatchEndReason)))
            {
                var key = MatchEndText.ReasonKey(reason);
                Assert.IsFalse(texts.Get(key).StartsWith("["), key);
                Assert.IsNotEmpty(texts.Get(key));
            }
        }

        [Test]
        public void SyncBeforeRetreat_IsDeferredUntilOutcome()
        {
            var moves = 0;
            var queue = new AnimationQueue();
            var context = new PresentationContext(new GameEvent[] { new TurnStarted(PlayerId.Player1),
                new BattleRetreated(1, PlayerId.Player1) }, null, null, queue,
                interval => new ActionStep(() => moves++));
            context.Index = 0;
            context.EnqueueSync();
            Assert.AreEqual(0, moves, "An ability must not move cards away before the outcome animation.");
            context.Index = 1;
            context.EnqueueSync();
            context.EnqueueSync();
            Assert.AreEqual(1, moves, "One sync after the outcome, consecutive duplicates coalesced.");
        }

        [Test]
        public void SyncBeforeResolution_IsDeferredUntilOutcome()
        {
            var count = 0;
            var power = new PowerBreakdown(150, 0, new PowerContribution[0], 150);
            var context = new PresentationContext(new GameEvent[] { new TurnStarted(PlayerId.Player1),
                new BattleResolved(1, power, power, ComparisonRule.HigherPowerWins, TieBreak.EarlierPlacement, PlayerId.Player1) },
                null, null, new AnimationQueue(), interval => new ActionStep(() => count++));
            context.EnqueueSync();
            Assert.AreEqual(0, count);
            context.Index = 1;
            context.EnqueueSync();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void OutcomeQueue_MovesAndChangesTurnOnlyAfterAnimationCompletes()
        {
            var queue = new AnimationQueue();
            Action finish = null;
            var order = "";
            queue.Enqueue(new WaitForPlayerStep(done => { finish = done; order += "outcome;"; }));
            queue.Enqueue(() => order += "move;");
            queue.Enqueue(() => order += "turn;");
            Assert.AreEqual("outcome;", order);
            Assert.IsTrue(queue.IsBusy);
            finish();
            Assert.AreEqual("outcome;move;turn;", order);
            Assert.IsFalse(queue.IsBusy);
        }
    }
}
