using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>Answers a pending choice (see MatchState.PendingChoice).</summary>
    public sealed class ChooseCard : GameCommand
    {
        public ChooseCard(PlayerId player, int cardId)
            : base(player)
        {
            CardId = cardId;
        }

        public int CardId { get; }
    }
}
