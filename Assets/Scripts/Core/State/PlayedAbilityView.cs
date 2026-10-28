namespace Game.Core.State
{
    /// <summary>An ability played in the current battle, as both players see it.</summary>
    public sealed class PlayedAbilityView
    {
        internal PlayedAbilityView(PlayedAbility played)
        {
            Player = played.Player;
            Card = CardRef.From(played.Instance);
            IsCancelled = played.IsCancelled;
        }

        public PlayerId Player { get; }
        public CardRef Card { get; }
        public bool IsCancelled { get; }
    }
}
