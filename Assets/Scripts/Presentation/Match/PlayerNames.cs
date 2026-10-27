using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Match
{
    public sealed class PlayerNames
    {
        private readonly UiTexts _texts;

        public PlayerNames(UiTexts texts)
        {
            _texts = texts;
        }

        public string Of(PlayerId player) =>
            _texts.Get(player == PlayerId.Player1 ? UiTextKeys.Player1 : UiTextKeys.Player2);

        public string Of(PlayerId? player) => player == null ? "" : Of(player.Value);
    }
}
