using System.Collections.Generic;
using System.Linq;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Match
{
    /// <summary>Finds placements that reached the table in this event batch.</summary>
    public static class CardSoundPresenter
    {
        public static Dictionary<int, SoundCue> Arrivals(IReadOnlyList<GameEvent> batch, PlayerView previous, PlayerView current)
        {
            var arrivals = new Dictionary<int, SoundCue>();
            if (previous == null)
                return arrivals;

            foreach (var gameEvent in batch)
            {
                switch (gameEvent)
                {
                    case FieldsDealt fields:
                        foreach (var fieldId in fields.FieldIds)
                        {
                            var field = current.Field(fieldId);
                            if (field != null && field.IsActive && field.Owner == fields.Owner &&
                                previous.Field(fieldId)?.IsInReserve == true)
                                arrivals[fieldId] = SoundCue.FieldPlace;
                        }
                        break;
                    case CreaturePlaced placed:
                        AddCreature(placed.Creature.InstanceId, placed.Player, placed.FieldId, SoundCue.CardPlace);
                        break;
                    case CreatureMoved moved when previous.Field(moved.FromFieldId)?.Occupant(moved.Player)?.InstanceId == moved.CreatureId:
                        AddCreature(moved.CreatureId, moved.Player, moved.ToFieldId, SoundCue.CreatureShift);
                        break;
                }
            }

            return arrivals;

            void AddCreature(int creatureId, PlayerId player, int fieldId, SoundCue cue)
            {
                if (current.Field(fieldId)?.Occupant(player)?.InstanceId == creatureId &&
                    previous.Field(fieldId)?.Occupant(player)?.InstanceId != creatureId)
                    arrivals[creatureId] = cue;
            }
        }

        public static HashSet<int> HandReturns(IReadOnlyList<GameEvent> batch, PlayerView previous, PlayerView current)
        {
            var returns = new HashSet<int>();
            if (previous == null || previous.Viewer != current.Viewer)
                return returns;

            foreach (var gameEvent in batch)
            {
                switch (gameEvent)
                {
                    case CreatureReturnedToHand creature when creature.Owner == current.Viewer:
                        AddReturn(creature.CreatureId);
                        break;
                    case AbilityReturnedToHand ability when ability.Owner == current.Viewer:
                        AddReturn(ability.Ability.InstanceId);
                        break;
                }
            }
            return returns;

            void AddReturn(int id)
            {
                if (!previous.Hand.Any(card => card.InstanceId == id) && current.Hand.Any(card => card.InstanceId == id))
                    returns.Add(id);
            }
        }
    }
}
