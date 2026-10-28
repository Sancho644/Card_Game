using System;
using System.Collections.Generic;
using Game.Core.Cards;
using Game.Core.Events;
using Game.Core.State;
using Game.Services.Match;

namespace Game.Simulation
{
    /// <summary>
    /// Fills a game record from all events of a simulated match. Card ids of fields and creatures in a battle
    /// are read from the views of their owners when the battle starts.
    /// </summary>
    internal sealed class MatchStatsCollector : IDisposable
    {
        private readonly IMatchSession _session;
        private readonly CardCatalog _catalog;
        private readonly GameRecord _record;
        private readonly HashSet<int> _openFields = new HashSet<int>();
        private BattleRecord _battle;

        public MatchStatsCollector(IMatchSession session, CardCatalog catalog, GameRecord record)
        {
            _session = session;
            _catalog = catalog;
            _record = record;
            _session.EventsProduced += OnEvents;
        }

        public void Dispose() => _session.EventsProduced -= OnEvents;

        private void OnEvents(IReadOnlyList<GameEvent> batch)
        {
            foreach (var gameEvent in batch)
            {
                switch (gameEvent)
                {
                    case TurnOrderChosen order:
                        _record.FirstPlayer = order.FirstPlayer;
                        break;
                    case TurnStarted _:
                        _record.Turns++;
                        break;
                    case MainActionSkipped _:
                        _record.Skips++;
                        break;
                    case FieldOpened opened:
                        _openFields.Add(opened.FieldId);
                        _record.FieldOpenings++;
                        break;
                    case BattleStarted started:
                        _battle = StartBattle(started);
                        break;
                    case AbilityPlayed played:
                        _record.CountUse(played.Player, played.Ability.CardId);
                        break;
                    case BattleRetreated _:
                        FinishBattle(winner: null, retreated: true);
                        break;
                    case BattleResolved resolved:
                        if (_battle != null)
                            _battle.TieBreak = resolved.TieBreak;
                        FinishBattle(resolved.Winner, retreated: false);
                        break;
                    case MatchEnded ended:
                        _record.Ended = true;
                        _record.Winner = ended.Winner;
                        _record.EndReason = ended.Reason;
                        break;
                }
            }
        }

        private BattleRecord StartBattle(BattleStarted started)
        {
            var field = _session.GetView(started.Host).Field(started.FieldId);
            var battle = new BattleRecord(started.FieldId, field.CardId, started.Host)
            {
                FirstActor = started.FirstActor,
                HostCreatureId = field.Occupant(started.Host)?.CardId,
                GuestCreatureId = field.Occupant(started.Guest)?.CardId,
            };
            battle.HostElement = ElementOf(battle.HostCreatureId);
            battle.GuestElement = ElementOf(battle.GuestCreatureId);

            _record.CountUse(started.Host, field.CardId);
            _record.CountUse(started.Host, battle.HostCreatureId);
            _record.CountUse(started.Guest, battle.GuestCreatureId);
            return battle;
        }

        private void FinishBattle(PlayerId? winner, bool retreated)
        {
            if (_battle == null)
                return;

            _battle.Winner = winner;
            _battle.Retreated = retreated;
            _battle.FieldOpen = _openFields.Contains(_battle.FieldId);
            _record.AddBattle(_battle);
            _battle = null;
        }

        private Element ElementOf(string creatureId) =>
            creatureId != null && _catalog.Get(creatureId) is CreatureCard creature ? creature.Element : default;
    }
}
