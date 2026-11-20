using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Audio
{
    public static class BattleOutcomeAudio
    {
        public static SoundCue? ResultCue(PlayerId? winner, PlayerId viewer, bool hotSeat)
        {
            if (!winner.HasValue)
                return null;

            return hotSeat || winner.Value == viewer ? SoundCue.BattleWin : SoundCue.BattleLose;
        }

        public static void Schedule(Sequence timeline, SoundCue? resultCue, IReadOnlyList<GameEvent> consequences, ISoundPlayer sound)
        {
            // Use the existing visual interval. At the default maximum speed (8x),
            // the three attacks still have distinct onsets. Group simultaneous removals.
            var duration = timeline.Duration();
            if (resultCue.HasValue)
                timeline.InsertCallback(0f, () => sound.Play(resultCue.Value));
            if (consequences.OfType<CreatureOut>().Any())
                timeline.InsertCallback(duration * .34f, () => sound.Play(SoundCue.CreatureOut));
            if (consequences.OfType<FieldRemoved>().Any())
                timeline.InsertCallback(duration * .67f, () => sound.Play(SoundCue.FieldRemove));
        }
    }
}
