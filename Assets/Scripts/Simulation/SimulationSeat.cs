using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.State;
using Game.Services.Match;

namespace Game.Simulation
{
    /// <summary>
    /// A player in a simulated match: the AI decides everything except the turn order, which a coin decides,
    /// so the report shows the bare advantage of going first. Counts the challenges the player makes.
    /// </summary>
    internal sealed class SimulationSeat : IPlayerController, IMatchObserver
    {
        private readonly IPlayerController _ai;
        private readonly IRandom _coin;
        private readonly GameRecord _record;

        public SimulationSeat(IPlayerController ai, IRandom coin, GameRecord record)
        {
            _ai = ai;
            _coin = coin;
            _record = record;
        }

        public PlayerId Player => _ai.Player;

        public void Observe(IReadOnlyList<GameEvent> visibleEvents) => (_ai as IMatchObserver)?.Observe(visibleEvents);

        public void RequestDecision(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit)
        {
            var orders = legalActions.OfType<ChooseTurnOrder>().ToList();
            if (orders.Count > 0)
            {
                submit(orders[_coin.Next(orders.Count)]);
                return;
            }

            _ai.RequestDecision(view, legalActions, Recorded(submit));
        }

        public void OfferReaction(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit) =>
            _ai.OfferReaction(view, legalActions, Recorded(submit));

        public void CancelDecision() => _ai.CancelDecision();

        public void AbandonDecision() => _ai.AbandonDecision();

        private Action<GameCommand> Recorded(Action<GameCommand> submit) => command =>
        {
            if (command is Challenge)
                _record.Challenges++;
            submit(command);
        };
    }
}
