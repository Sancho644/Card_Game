using Game.Core.State;
using Game.Data.Presentation;

namespace Game.Presentation.Match
{
    public sealed class PlayerNames
    {
        private readonly UiTexts _texts;
        private string _player1Key = UiTextKeys.Player1;
        private string _player2Key = UiTextKeys.Player2;

        public PlayerNames(UiTexts texts)
        {
            _texts = texts;
        }

        /// <summary>"Игрок 1" and "Игрок 2" in hot-seat, "Вы" and "Соперник" against an opponent.</summary>
        public void Use(MatchMode mode)
        {
            _player1Key = mode.Player1NameKey;
            _player2Key = mode.Player2NameKey;
        }

        public string Of(PlayerId player) => _texts.Get(player == PlayerId.Player1 ? _player1Key : _player2Key);

        public string Of(PlayerId? player) => player == null ? "" : Of(player.Value);
    }
}
