using Game.Data.Presentation;
using Game.Presentation.Ui;
using UnityEngine.UI;

namespace Game.Presentation.Match
{
    /// <summary>Optional result inspection, retained until the next turn or hand-off.</summary>
    public sealed class BattleDetails
    {
        private readonly Button _button;
        private readonly DialogView _dialog;
        private readonly UiTexts _texts;
        private string _body;
        private bool _ownsDialog;

        public BattleDetails(Button button, DialogView dialog, UiTexts texts)
        {
            _button = button;
            _dialog = dialog;
            _texts = texts;
            UiButtons.SetLabel(button, texts.Get(UiTextKeys.BreakdownDetails));
            Clear();
        }

        public void Remember(string body)
        {
            _body = body;
            UiButtons.Bind(_button, Show);
        }

        public void Clear()
        {
            _body = null;
            UiButtons.Bind(_button, null);
            if (_ownsDialog) _dialog.Hide();
            _ownsDialog = false;
        }

        private void Show()
        {
            _ownsDialog = true;
            _dialog.Show(_texts.Get(UiTextKeys.BreakdownTitle), _body,
                _texts.Get(UiTextKeys.Close), () => _ownsDialog = false);
        }
    }
}
