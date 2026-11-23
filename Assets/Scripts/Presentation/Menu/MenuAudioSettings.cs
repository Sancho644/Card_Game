using Game.Data.Presentation;
using Game.Presentation.Ui;
using Game.Services.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Menu
{
    /// <summary>Volume controls inside the existing main-menu settings dialog.</summary>
    public sealed class MenuAudioSettings
    {
        private readonly IPlayerSettingsService _settings;
        private readonly CollectionUi _ui;
        private readonly UiTexts _texts;
        private readonly TMP_Text _muteLabel;
        private readonly Button _preview;

        public MenuAudioSettings(RectTransform panel, CollectionUi ui, UiTexts texts, IPlayerSettingsService settings)
        {
            _ui = ui;
            _texts = texts;
            _settings = settings;
            AddSlider(panel, AudioBus.Master, "settings.audio.master", .55f);
            AddSlider(panel, AudioBus.Music, "settings.audio.music", .44f);
            AddSlider(panel, AudioBus.Sfx, "settings.audio.sfx", .33f);
            AddSlider(panel, AudioBus.Ui, "settings.audio.ui", .22f);

            _preview = ui.Button(panel, "PreviewAudio", texts.Get("settings.audio.preview"),
                .57f, .164f, .35f, .055f, Preview, null);
            var mute = ui.Button(panel, "Mute", "", .08f, .065f, .46f, .105f, ToggleMute, null);
            _muteLabel = mute.GetComponentInChildren<TMP_Text>();
            UpdateMuteLabel();
            UpdatePreview();
        }

        private void AddSlider(RectTransform panel, AudioBus bus, string label, float y)
        {
            var row = _ui.Rect(panel, bus + "Volume", .08f, y, .84f, .09f);
            _ui.Text(row, "Label", _texts.Get(label), 0f, 0f, .38f, 1f, 23f);
            var valueText = _ui.Text(row, "Value", "", .88f, 0f, .12f, 1f, 22f);
            valueText.alignment = TextAlignmentOptions.MidlineRight;

            var control = _ui.Rect(row, "Slider", .4f, 0f, .45f, 1f);
            var background = control.gameObject.AddComponent<Image>();
            background.color = Color.clear;
            _ui.Plaque(control, "Track", 0f, .39f, 1f, .22f);
            var handleArea = _ui.Rect(control, "HandleArea", .034f, 0f, .932f, 1f);
            var handle = _ui.Panel(handleArea, "Handle", 0f, .13f, 0f, .74f, new Color(.78f, .59f, .32f));
            handle.sizeDelta = new Vector2(18f, 0f);
            var slider = control.gameObject.AddComponent<Slider>();
            slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.maxValue = 100f;
            slider.wholeNumbers = true;
            slider.SetValueWithoutNotify(Mathf.RoundToInt(_settings.Volume(bus) * 100f));
            valueText.text = $"{slider.value:0}%";
            slider.onValueChanged.AddListener(value =>
            {
                _settings.SetVolume(bus, value / 100f);
                valueText.text = $"{value:0}%";
                UpdatePreview();
            });
        }

        private void ToggleMute()
        {
            _settings.AudioMuted = !_settings.AudioMuted;
            UpdateMuteLabel();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            _preview.interactable = !_settings.AudioMuted &&
                _settings.Volume(AudioBus.Master) > 0f && _settings.Volume(AudioBus.Ui) > 0f;
        }

        private void Preview() => UiButtons.Play(_preview, SoundCue.UiSettingsPreview);

        private void UpdateMuteLabel() =>
            _muteLabel.text = _texts.Get(_settings.AudioMuted ? "settings.audio.unmute" : "settings.audio.mute");
    }
}
