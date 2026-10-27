namespace Game.Core.State
{
    public enum MatchPhase
    {
        NotStarted,
        ChoosingTurnOrder,
        Turn,
        Battle,

        /// <summary>A player must choose a card (see <see cref="MatchState.PendingChoice"/>).</summary>
        AwaitingChoice,

        Ended,
    }
}
