using System.Collections.Generic;
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
                        AddCreature(placed.Creature.InstanceId, placed.Player, placed.FieldId);
                        break;
                    case CreatureMoved moved:
                        AddCreature(moved.CreatureId, moved.Player, moved.ToFieldId);
                        break;
                }
            }

            return arrivals;

            void AddCreature(int creatureId, PlayerId player, int fieldId)
            {
                if (current.Field(fieldId)?.Occupant(player)?.InstanceId == creatureId &&
                    previous.Field(fieldId)?.Occupant(player)?.InstanceId != creatureId)
                    arrivals[creatureId] = SoundCue.CardPlace;
            }
        }
    }
}
