using System.Linq;
using System.Reflection;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.Audio;
using Game.Presentation.Cards;
using Game.Presentation.Menu;
using Game.Presentation.Ui;
using Game.Services.Settings;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Tests
{
    public sealed class UiAudioP1Tests
    {
        private GameObject _root;
        private RecordingSoundPlayer _sound;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("UI P1 test", typeof(RectTransform));
            _sound = new RecordingSoundPlayer();
            _root.AddComponent<UiAudioScope>().Sound = _sound;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_root);

        [TestCase(SoundCue.UiConfirm)]
        [TestCase(SoundCue.UiCancel)]
        [TestCase(SoundCue.UiDenied)]
        [TestCase(SoundCue.UiModalOpen)]
        [TestCase(SoundCue.UiModalClose)]
        [TestCase(SoundCue.UiCardSelect)]
        [TestCase(SoundCue.UiDeckSelect)]
        [TestCase(SoundCue.UiSettingsPreview)]
        public void AUiActionReplacesTheClickAndUsesTheExistingUiSource(SoundCue cue)
        {
            var button = new GameObject("Button", typeof(RectTransform)).AddComponent<Button>();
            button.transform.SetParent(_root.transform, false);
            var calls = 0;
            UiButtons.Assign(button, () => Assert.Fail("The old listener must be replaced."));
            UiButtons.Bind(button, () => calls++, cue);
            Assert.That(_sound.Calls, Is.Empty);
            button.onClick.Invoke();
            Assert.That(calls, Is.EqualTo(1));
            CollectionAssert.AreEqual(new[] { cue }, _sound.Calls);

            var settings = Load<SoundSettings>("Assets/Data/Config/SoundSettings.asset");
            var entry = settings.Find(cue);
            Assert.That(entry.Clip, Is.Not.Null);
            Assert.That(entry.Clip.frequency, Is.EqualTo(44100));
            Assert.That(entry.Clip.channels, Is.EqualTo(1));
            var path = AssetDatabase.GetAssetPath(entry.Clip);
            StringAssert.StartsWith("Assets/Audio/SFX/UI/P1/", path);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            foreach (var platform in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
            {
                Assert.That(importer.ContainsSampleSettingsOverride(platform), Is.True);
                var sample = importer.GetOverrideSampleSettings(platform);
                Assert.That(sample.compressionFormat, Is.EqualTo(AudioCompressionFormat.PCM));
                Assert.That(sample.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
                Assert.That(sample.preloadAudioData, Is.True);
            }

            var player = _root.AddComponent<AudioSourceSoundPlayer>();
            player.Configure(settings, settings.Mixer.FindMatchingGroups("Master/SFX").Single(),
                settings.Mixer.FindMatchingGroups("Master/UI").Single());
            player.Play(cue);
            var index = typeof(AudioSourceSoundPlayer).GetField("_next", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(index.GetValue(player), Is.EqualTo(0), "UI playback must not consume an SFX voice.");
            Assert.That(_root.GetComponents<AudioSource>().Length, Is.EqualTo(5));
        }

        [Test]
        public void HiddenOrDisabledButtonsDoNotRunTheirActionOrSound()
        {
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(_root.transform, false);
            var button = new GameObject("Button", typeof(RectTransform)).AddComponent<Button>();
            button.transform.SetParent(panel.transform, false);
            var activated = 0;
            UiButtons.Assign(button, () => activated++);
            button.onClick.Invoke();
            button.interactable = false;
            button.onClick.Invoke();
            button.interactable = true;
            panel.SetActive(false);
            button.onClick.Invoke();
            Assert.That(activated, Is.EqualTo(1));
            CollectionAssert.AreEqual(new[] { SoundCue.Button }, _sound.Calls);
        }

        [Test]
        public void SliderChangesUpdateSettingsSilentlyAndPreviewRequiresAnExplicitAction()
        {
            var fonts = Load<TMP_FontAsset>("Assets/Art/Fonts/GameFont SDF.asset");
            var style = Load<CollectionSettings>("Assets/Data/Config/Collection.asset");
            var settings = new LocalPlayerSettingsService(new MemorySettingsStore());
            using var ui = new CollectionUi(fonts, style);
            var texts = Load<UiTexts>("Assets/Data/Config/UiTexts.asset");
            new MenuAudioSettings((RectTransform)_root.transform, ui, texts, settings);
            var slider = _root.transform.Find("UiVolume/Slider").GetComponent<Slider>();
            var preview = _root.transform.Find("PreviewAudio").GetComponent<Button>();
            var mute = _root.transform.Find("Mute").GetComponent<Button>();
            for (var i = 80; i > 40; i--)
                slider.value = i;
            Assert.That(settings.Volume(AudioBus.Ui), Is.EqualTo(.41f).Within(.001f));
            Assert.That(_sound.Calls, Is.Empty);
            preview.onClick.Invoke();
            mute.onClick.Invoke();
            Assert.That(settings.AudioMuted, Is.True);
            preview.onClick.Invoke();
            mute.onClick.Invoke();
            slider.value = 0;
            preview.onClick.Invoke();
            CollectionAssert.AreEqual(new[] { SoundCue.UiSettingsPreview }, _sound.Calls);
        }

        [Test]
        public void RenderingAndRebindingCollectionCardsIsSilentUntilSelection()
        {
            var fonts = Load<TMP_FontAsset>("Assets/Art/Fonts/GameFont SDF.asset");
            var style = Load<CollectionSettings>("Assets/Data/Config/Collection.asset");
            var texts = Load<UiTexts>("Assets/Data/Config/UiTexts.asset");
            var database = Load<CardDatabase>("Assets/Data/CardDatabase.asset");
            using var ui = new CollectionUi(fonts, style);
            var view = new CollectionCardView(ui, style, new CardFaceBuilder(database, style.CardVisuals, texts));
            var card = database.Find("creature.stone-beetle");
            view.Create(_root.transform, card, 0, false, false, "", null);
            var active = view.Create(_root.transform, card, 0, false, false, "", () => { });
            Assert.That(_sound.Calls, Is.Empty);
            active.onClick.Invoke();
            CollectionAssert.AreEqual(new[] { SoundCue.UiCardSelect }, _sound.Calls);
        }

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
