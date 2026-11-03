using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Match
{
    public static class MatchEndText
    {
        public static string TitleKey(PlayerId? winner, PlayerId viewer, bool hotSeat) =>
            winner == null ? UiTextKeys.EndDraw : hotSeat ? UiTextKeys.EndWinner :
            winner == viewer ? UiTextKeys.EndVictory : UiTextKeys.EndDefeat;

        public static string ReasonKey(MatchEndReason reason) => UiTextKeys.EndReasonPrefix + reason;
    }
}
