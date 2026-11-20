using System.Collections.Generic;
using DG.Tweening;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Audio;
using Game.Presentation.Match;
using Game.Presentation.Ui;
using Game.Services.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Tests
{
    public sealed class AudioRuntimeTests
    {
        [TestCase(0f, -80f)]
        [TestCase(.5f, -6.0206f)]
        [TestCase(1f, 0f)]
        [TestCase(-1f, -80f)]
        [TestCase(10f, 0f)]
        [TestCase(float.NaN, -80f)]
        public void DecibelConversionIsFinite(float level, float expected) =>
            Assert.That(AudioMixerLevels.ToDecibels(level), Is.EqualTo(expected).Within(.001f));

        [Test]
        public void SettingsClampAndPersistAllBusesWithoutResettingOnMute()
        {
            var store = new MemorySettingsStore();
            var settings = new LocalPlayerSettingsService(store);

            foreach (var bus in new[] { AudioBus.Master, AudioBus.Music, AudioBus.Sfx, AudioBus.Ui })
                Assert.AreEqual(1f, settings.Volume(bus));

            settings.SetVolume(AudioBus.Master, .64f);
            settings.SetVolume(AudioBus.Music, .25f);
            settings.SetVolume(AudioBus.Sfx, -10f);
            settings.SetVolume(AudioBus.Ui, 10f);
            settings.SetVolume(AudioBus.Master, float.NaN);
            Assert.AreEqual(0, store.Saves);
            settings.AudioMuted = true;
            Assert.AreEqual(1, store.Saves);

            var reloaded = new LocalPlayerSettingsService(store);
            Assert.IsTrue(reloaded.AudioMuted);
            reloaded.AudioMuted = false;
            Assert.AreEqual(.64f, reloaded.Volume(AudioBus.Master));
            Assert.AreEqual(.25f, reloaded.Volume(AudioBus.Music));
            Assert.AreEqual(0f, reloaded.Volume(AudioBus.Sfx));
            Assert.AreEqual(1f, reloaded.Volume(AudioBus.Ui));
        }

        [Test]
        public void MixerHasIndependentBusLevelsAndUsesMasterExactlyOnce()
        {
            var settings = new LocalPlayerSettingsService(new MemorySettingsStore());
            settings.SetVolume(AudioBus.Master, .5f);
            settings.SetVolume(AudioBus.Music, .25f);
            settings.SetVolume(AudioBus.Ui, .75f);
            var parameters = new Dictionary<string, float>();
            var levels = new AudioMixerLevels((name, db) => { parameters[name] = db; return true; });
            levels.Apply(settings);

            Assert.AreEqual(AudioMixerLevels.ToDecibels(.5f), parameters["MasterVolume"]);
            Assert.AreEqual(AudioMixerLevels.ToDecibels(.25f), parameters["MusicVolume"]);
            Assert.AreEqual(AudioMixerLevels.ToDecibels(.75f), parameters["UiVolume"]);
            Assert.AreEqual(0f, parameters["SfxVolume"]);
            Assert.AreEqual(1f, levels.SourceGain(AudioBus.Music, routed: true));
            Assert.AreEqual(.125f, levels.SourceGain(AudioBus.Music, routed: false));

            settings.AudioMuted = true;
            levels.Apply(settings);
            Assert.AreEqual(-80f, parameters["MasterVolume"]);
            Assert.AreEqual(AudioMixerLevels.ToDecibels(.25f), parameters["MusicVolume"]);
            Assert.AreEqual(0f, levels.SourceGain(AudioBus.Sfx, routed: true));
        }

        [Test]
        public void MissingMixerParameterUsesSourceGainOnlyForThatParameter()
        {
            var settings = new LocalPlayerSettingsService(new MemorySettingsStore());
            settings.SetVolume(AudioBus.Master, .5f);
            settings.SetVolume(AudioBus.Sfx, .2f);
            var levels = new AudioMixerLevels((name, _) => name != "SfxVolume");
            levels.Apply(settings);

            Assert.AreEqual(.2f, levels.SourceGain(AudioBus.Sfx, routed: true));
            Assert.AreEqual(.1f, levels.SourceGain(AudioBus.Sfx, routed: false));
        }

        [Test]
        public void ProjectMixerHasAllRoutesAndExposedParameters()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoundSettings>("Assets/Data/Config/SoundSettings.asset");
            Assert.AreEqual("Assets/Audio/Mixers/CardGameAudioMixer.mixer", AssetDatabase.GetAssetPath(settings.Mixer));
            foreach (var route in new[] { "Master/Music", "Master/SFX", "Master/UI" })
                Assert.That(settings.Mixer.FindMatchingGroups(route), Has.Length.EqualTo(1), route);
            foreach (var parameter in new[] { "MasterVolume", "MusicVolume", "SfxVolume", "UiVolume" })
                Assert.IsTrue(settings.Mixer.GetFloat(parameter, out _), parameter);
        }

        [Test]
        public void MissingAudioClipsAreSafeAndConfigurationDoesNotGrowThePools()
        {
            var root = new GameObject("Audio test");
            var settings = ScriptableObject.CreateInstance<SoundSettings>();
            try
            {
                var effects = root.AddComponent<AudioSourceSoundPlayer>();
                var music = root.AddComponent<MusicPlayer>();
                Assert.DoesNotThrow(() => effects.Play(SoundCue.CardDeal));
                music.Configure(null);
                music.Configure(null);
                effects.Configure(settings, music: music);
                effects.Configure(settings, music: music);
                Assert.That(root.GetComponents<AudioSource>(), Has.Length.EqualTo(7));
                Assert.DoesNotThrow(() => effects.Play(SoundCue.CardDeal));
                Assert.DoesNotThrow(() => effects.Play(SoundCue.Button));
                Assert.DoesNotThrow(() => effects.Play(SoundCue.Victory));
                Assert.DoesNotThrow(() => music.Play(null));
                Assert.DoesNotThrow(() => music.Stop(0f));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void BattleOutcomeSpacesOneAttackPerKindWithinTheExistingAnimation()
        {
            var sound = new RecordingSoundPlayer();
            var consequences = new GameEvent[]
            {
                new CreatureOut(PlayerId.Player1, 10),
                new CreatureOut(PlayerId.Player2, 20),
                new FieldRemoved(30), new FieldRemoved(30)
            };
            var timeline = DOTween.Sequence().SetUpdate(UpdateType.Manual).Pause();
            try
            {
                timeline.AppendInterval(3f);
                BattleOutcomeAudio.Schedule(timeline, true, consequences, sound);
                Assert.AreEqual(3f, timeline.Duration());
                timeline.Play();
                DOTween.ManualUpdate(.1f, .1f);
                CollectionAssert.AreEqual(new[] { SoundCue.BattleWin }, sound.Calls);
                DOTween.ManualUpdate(1.05f, 1.05f);
                CollectionAssert.AreEqual(new[] { SoundCue.BattleWin, SoundCue.CreatureOut }, sound.Calls);
                DOTween.ManualUpdate(1f, 1f);
                CollectionAssert.AreEqual(new[] { SoundCue.BattleWin, SoundCue.CreatureOut, SoundCue.FieldRemove }, sound.Calls);
            }
            finally
            {
                timeline.Kill();
            }
        }

        [Test]
        public void DrawnBattleOnlySoundsItsActualConsequences()
        {
            var sound = new RecordingSoundPlayer();
            var timeline = DOTween.Sequence().SetUpdate(UpdateType.Manual).Pause();
            try
            {
                timeline.AppendInterval(3f);
                BattleOutcomeAudio.Schedule(timeline, false, new GameEvent[] { new CreatureOut(PlayerId.Player1, 10) }, sound);
                timeline.Play();
                DOTween.ManualUpdate(2.5f, 2.5f);
                CollectionAssert.AreEqual(new[] { SoundCue.CreatureOut }, sound.Calls);
            }
            finally
            {
                timeline.Kill();
            }
        }

        [TestCase(PlayerId.Player1, false, SoundCue.Victory)]
        [TestCase(PlayerId.Player2, false, SoundCue.Defeat)]
        [TestCase(PlayerId.Player2, true, SoundCue.Victory)]
        [TestCase(null, false, SoundCue.Draw)]
        public void ResultPlaysOncePerMatch(PlayerId? winner, bool hotSeat, SoundCue cue)
        {
            var sound = new RecordingSoundPlayer();
            var match = new MatchResultAudio();
            match.PlayOnce(winner, PlayerId.Player1, hotSeat, sound);
            match.PlayOnce(winner, PlayerId.Player1, hotSeat, sound);
            CollectionAssert.AreEqual(new[] { cue }, sound.Calls);
            new MatchResultAudio().PlayOnce(winner, PlayerId.Player1, hotSeat, sound);
            CollectionAssert.AreEqual(new[] { cue, cue }, sound.Calls);
        }

        [Test]
        public void RebindingButtonDoesNotAccumulateSoundOrPlayWhenDisabled()
        {
            var root = new GameObject("UI test");
            var sound = new RecordingSoundPlayer();
            try
            {
                root.AddComponent<UiAudioScope>().Sound = sound;
                var item = new GameObject("Confirm", typeof(RectTransform), typeof(Button));
                item.transform.SetParent(root.transform);
                var button = item.GetComponent<Button>();
                var actions = 0;
                UiButtons.Assign(button, () => Assert.Fail("Old handler"));
                UiButtons.Bind(button, () => actions++);
                button.onClick.Invoke();
                button.interactable = false;
                button.onClick.Invoke();
                Assert.AreEqual(1, actions);
                CollectionAssert.AreEqual(new[] { SoundCue.Button }, sound.Calls);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RepeatedMusicRequestKeepsFadeAndDoesNotUseSecondSlot()
        {
            var fade = new MusicFade<string>();
            var slot = fade.Play("menu", 1f);
            fade.Advance(.5f);
            Assert.AreEqual(slot, fade.Play("menu", 1f));
            Assert.IsNull(fade.Track(1 - slot));
            fade.Advance(.5f);
            Assert.AreEqual(1f, fade.Weight(slot));
        }

        [Test]
        public void CrossfadeKeepsPriorTrackUntilReplacementWinsAndStopClearsIt()
        {
            var fade = new MusicFade<string>();
            var menu = fade.Play("menu", 0f);
            var match = fade.Play("match", 1f);
            Assert.AreNotEqual(menu, match);
            fade.Advance(.5f);
            Assert.AreEqual(.5f, fade.Weight(menu));
            Assert.AreEqual(.5f, fade.Weight(match));
            fade.Advance(.5f);
            Assert.IsNull(fade.Track(menu));
            Assert.AreEqual(1f, fade.Weight(match));
            fade.Stop(.5f);
            fade.Advance(.5f);
            Assert.IsNull(fade.Track(match));
            Assert.AreEqual(0f, fade.Weight(match));
        }
    }
}
