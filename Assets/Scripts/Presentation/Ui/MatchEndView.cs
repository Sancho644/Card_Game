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
        private BattleWindowSkin _skin;
        private Image _emblem;
        private TMP_Text _score;
        public void Configure(BattleWindowSkin skin)
        {
            _skin = skin;
            skin.Window(transform, _title, _body, _primary, _secondary, BattleEmblem.Victory, true);
            _emblem = _title.transform.parent.Find("WindowEmblem").GetComponent<Image>();
            _score = skin.Ui.Text(_title.transform.parent, "FinalScore", "", .08f, .43f, .84f, .17f);
            skin.Label(_score, skin.Settings.TitleSize);
            BattleWindowSkin.Place(_body.rectTransform, _body.transform.parent, .09f, .24f, .82f, .17f);
        }

        public void Show(PlayerView view, MatchMode mode, PlayerNames names, UiTexts texts, ISoundPlayer sound, Action menu)
        {
            var result = view.Result;
            if (_skin != null)
            {
                var emblem = result.IsDraw ? BattleEmblem.Draw : mode.ShowsHandoff || result.Winner == view.Viewer ? BattleEmblem.Victory : BattleEmblem.Defeat;
                _emblem.sprite = _skin.Sprite(emblem);
                _title.color = emblem == BattleEmblem.Defeat ? new Color(.73f, .77f, .75f) : _skin.Settings.Gold;
            }
            var key = MatchEndText.TitleKey(result.Winner, view.Viewer, mode.ShowsHandoff);
            _title.text = mode.ShowsHandoff && result.Winner.HasValue
                ? texts.Format(key, names.Of(result.Winner.Value)) : texts.Get(key);
            _body.text = texts.Format(UiTextKeys.EndScore, names.Of(PlayerId.Player1), view.Wins(PlayerId.Player1),
                names.Of(PlayerId.Player2), view.Wins(PlayerId.Player2)) + "\n\n" + texts.Get(MatchEndText.ReasonKey(result.Reason));
            if (_score != null)
            {
                _score.text = texts.Format(UiTextKeys.EndScore, names.Of(PlayerId.Player1), view.Wins(PlayerId.Player1), names.Of(PlayerId.Player2), view.Wins(PlayerId.Player2));
                _body.text = texts.Get(MatchEndText.ReasonKey(result.Reason));
            }
            UiButtons.SetLabel(_primary, texts.Get(UiTextKeys.ToMenu));
            UiButtons.Bind(_primary, menu);
            UiButtons.Bind(_secondary, null);
            _skin?.SingleButton(_primary, _secondary);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            sound.Play(result.IsDraw ? SoundCue.Draw : mode.ShowsHandoff || result.Winner == view.Viewer ? SoundCue.Victory : SoundCue.Defeat);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
