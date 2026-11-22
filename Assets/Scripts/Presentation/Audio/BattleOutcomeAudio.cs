using System.Collections.Generic;
using DG.Tweening;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Audio
{
    public static class BattleOutcomeAudio
    {
        public static SoundCue ResultCue(PlayerId? winner, PlayerId viewer, bool hotSeat)
        {
            if (!winner.HasValue)
                return SoundCue.BattleDraw;

            return hotSeat || winner.Value == viewer ? SoundCue.BattleWin : SoundCue.BattleLose;
        }

        public static void Schedule(Sequence timeline, SoundCue? resultCue, Tween firstCreatureFade, ISoundPlayer sound)
        {
            if (resultCue.HasValue)
                timeline.OnStart(() => sound.Play(resultCue.Value));
            firstCreatureFade?.OnStart(() => sound.Play(SoundCue.CreatureOut));
        }

        public static HashSet<int> RemovedFields(IReadOnlyList<GameEvent> batch, PlayerView previous, PlayerView current)
        {
            var removals = new HashSet<int>();
            if (previous == null)
                return removals;

            foreach (var gameEvent in batch)
            {
                if (gameEvent is FieldRemoved removed
                    && previous.Field(removed.FieldId)?.IsActive == true
                    && current.Field(removed.FieldId)?.IsRemoved == true)
                    removals.Add(removed.FieldId);
            }

            return removals;
        }
    }
}
