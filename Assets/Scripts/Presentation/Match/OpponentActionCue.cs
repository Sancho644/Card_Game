using System;
using System.Collections.Generic;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Presentation.Match
{
    /// <summary>The initiating action, not its secondary effects. Never contains hidden card definitions.</summary>
    public sealed class OpponentActionCue
    {
        public OpponentActionCue(PlayerId player, int? cardId)
        {
            Player = player;
            CardId = cardId;
        }

        public PlayerId Player { get; }
        public int? CardId { get; }

        public static OpponentActionCue FromBatch(IEnumerable<GameEvent> batch, Func<int, PlayerId?> fieldOwner)
        {
            foreach (var item in batch)
            {
                switch (item)
                {
                    case AbilityPlayed e: return new OpponentActionCue(e.Player, e.Ability.InstanceId);
                    case CreaturePlaced e: return new OpponentActionCue(e.Player, e.Creature.InstanceId);
                    case CreatureMoved e: return new OpponentActionCue(e.Player, e.CreatureId);
                    case PlayerReady e: return new OpponentActionCue(e.Player, null);
                    case TurnEnded e: return new OpponentActionCue(e.Player, null);
                    case TurnOrderChosen e: return new OpponentActionCue(e.Chooser, null);
                    case MainActionSkipped e: return new OpponentActionCue(e.Player, null);
                    case BattleRetreated e: return new OpponentActionCue(e.Player, null);
                    case AbilityReturnedToHand e: return new OpponentActionCue(e.Owner, null);
                    case FieldOpened e:
                        var owner = fieldOwner(e.FieldId);
                        if (owner.HasValue) return new OpponentActionCue(owner.Value, e.FieldId);
                        break;
                }
            }
            return null;
        }
    }
}
