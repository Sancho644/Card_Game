using System;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Audio;
using Game.Presentation.Match;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>Final result, score and rule that ended the match, including both-skip outcomes.</summary>
    public sealed class MatchEndView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private Button _primary;
        [SerializeField] private Button _secondary;

        public void Show(PlayerView view, MatchMode mode, PlayerNames names, UiTexts texts, ISoundPlayer sound, Action menu)
        {
            var result = view.Result;
            var key = MatchEndText.TitleKey(result.Winner, view.Viewer, mode.ShowsHandoff);
            _title.text = mode.ShowsHandoff && result.Winner.HasValue
                ? texts.Format(key, names.Of(result.Winner.Value)) : texts.Get(key);
            _body.text = texts.Format(UiTextKeys.EndScore, names.Of(PlayerId.Player1), view.Wins(PlayerId.Player1),
                names.Of(PlayerId.Player2), view.Wins(PlayerId.Player2)) + "\n\n" + texts.Get(MatchEndText.ReasonKey(result.Reason));
            UiButtons.SetLabel(_primary, texts.Get(UiTextKeys.ToMenu));
            UiButtons.Bind(_primary, menu);
            UiButtons.Bind(_secondary, null);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            sound.Play(result.IsDraw ? SoundCue.Draw : mode.ShowsHandoff || result.Winner == view.Viewer ? SoundCue.Victory : SoundCue.Defeat);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
