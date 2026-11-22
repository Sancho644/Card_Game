using System.Linq;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Audio;
using Game.Presentation.Match;
using Game.Services.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class MusicP0Tests
    {
        private const string SettingsPath = "Assets/Data/Config/SoundSettings.asset";
        private const string MusicFolder = "Assets/Audio/Music/";

        [TestCase("music.menu.loop", true, 90f, 150f)]
        [TestCase("music.match.loop", true, 120f, 180f)]
        [TestCase("music.result.victory", false, 2f, 4f)]
        [TestCase("music.result.defeat", false, 2f, 4f)]
        public void AssignedMusicUsesStereoAndTheIntendedPlatformLoadModes(string id, bool loop, float minimum, float maximum)
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoundSettings>(SettingsPath);
            var clip = id switch
            {
                "music.menu.loop" => settings.MenuMusic,
                "music.match.loop" => settings.MatchMusic,
                "music.result.victory" => settings.Find(SoundCue.Victory).Clip,
                _ => settings.Find(SoundCue.Defeat).Clip
            };

            Assert.IsNotNull(clip);
            Assert.AreEqual(MusicFolder + id + ".wav", AssetDatabase.GetAssetPath(clip));
            Assert.AreEqual(2, clip.channels);
            Assert.That(clip.length, Is.InRange(minimum, maximum));

            var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
            Assert.IsFalse(importer.forceToMono);
            var loadType = loop ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            foreach (var platform in new[] { "Standalone", "Android" })
            {
                Assert.IsTrue(importer.ContainsSampleSettingsOverride(platform));
                var sample = importer.GetOverrideSampleSettings(platform);
                Assert.AreEqual(AudioCompressionFormat.Vorbis, sample.compressionFormat);
                Assert.AreEqual(loadType, sample.loadType);
                Assert.AreEqual(!loop, sample.preloadAudioData);
                if (platform == "Android")
                {
                    Assert.AreEqual(AudioSampleRateSetting.OverrideSampleRate, sample.sampleRateSetting);
                    Assert.AreEqual(44100u, sample.sampleRateOverride);
                }
            }
        }

        [Test]
        public void MenuToMatchAndBackReleasesTheOutgoingSource()
        {
            var music = new MusicFade<string>();
            var original = music.Play("menu", 0f);
            var match = music.Play("match", .65f);
            music.Advance(.325f);
            Assert.AreEqual(.5f, music.Weight(original));
            Assert.AreEqual(.5f, music.Weight(match));
            music.Advance(.325f);
            Assert.IsNull(music.Track(original));

            var menu = music.Play("menu", .65f);
            music.Advance(.65f);
            Assert.IsNull(music.Track(match));
            Assert.AreEqual("menu", music.Track(menu));
            Assert.AreEqual(1f, music.Weight(menu));
        }

        [Test]
        public void TwoFastReversalsContinueTheLastRequestFromCurrentWeights()
        {
            var music = new MusicFade<string>();
            var menu = music.Play("menu", 0f);
            var match = music.Play("match", 1f);
            music.Advance(.2f);
            Assert.AreEqual(menu, music.Play("menu", 1f));
            Assert.That(music.Weight(menu), Is.EqualTo(.8f).Within(.0001f));
            music.Advance(.25f);
            Assert.AreEqual(match, music.Play("match", 1f));
            Assert.That(music.Weight(match), Is.EqualTo(.15f).Within(.0001f));
            music.Advance(.5f);
            Assert.AreEqual(match, music.Play("match", 1f));
            music.Advance(.5f);
            Assert.IsNull(music.Track(menu));
            Assert.AreEqual(1f, music.Weight(match));
        }

        [TestCase(PlayerId.Player1, SoundCue.Victory)]
        [TestCase(PlayerId.Player2, SoundCue.Defeat)]
        public void RepeatedResultDoesNotRestartTheRealMusicSource(PlayerId winner, SoundCue cue)
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoundSettings>(SettingsPath);
            var root = new GameObject("Result music test");
            try
            {
                var music = root.AddComponent<MusicPlayer>();
                var group = settings.Mixer.FindMatchingGroups("Master/Music").Single();
                music.Configure(group);
                var musicSources = root.GetComponents<AudioSource>();
                var sound = root.AddComponent<AudioSourceSoundPlayer>();
                sound.Configure(settings, music: music);

                var result = new MatchResultAudio();
                result.PlayOnce(winner, PlayerId.Player1, false, sound);
                var source = musicSources.Single(item => item.clip != null);
                Assert.AreEqual(settings.Find(cue).Clip, source.clip);
                Assert.AreEqual(group, source.outputAudioMixerGroup);
                Assert.IsFalse(source.loop);
                Assert.AreEqual(1f, source.pitch);

                music.Stop(0f);
                result.PlayOnce(winner, PlayerId.Player1, false, sound);
                Assert.IsTrue(musicSources.All(item => item.clip == null));
                Assert.IsTrue(root.GetComponents<AudioSource>().Except(musicSources).All(item => item.clip == null));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void MissingMusicAndTeardownClearOwnedSources()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoundSettings>(SettingsPath);
            var root = new GameObject("Music ownership test");
            AudioSource[] sources;
            try
            {
                var music = root.AddComponent<MusicPlayer>();
                music.Configure(null);
                sources = root.GetComponents<AudioSource>();
                music.Play(settings.MenuMusic, fadeSeconds: 0f);
                music.Play(null, fadeSeconds: 0f);
                Assert.IsTrue(sources.All(item => item.clip == null));

                music.Play(settings.MenuMusic, fadeSeconds: 0f);
                music.Play(settings.MatchMusic);
                Assert.IsTrue(sources.All(item => item.clip != null));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            Assert.IsTrue(sources.All(item => item == null));
        }

        [Test]
        public void MusicMuteAndMasterVolumeDoNotOverwriteEffectAndUiLevels()
        {
            var settings = new LocalPlayerSettingsService(new MemorySettingsStore());
            settings.SetVolume(AudioBus.Master, .8f);
            settings.SetVolume(AudioBus.Music, .36f);
            settings.SetVolume(AudioBus.Sfx, .75f);
            settings.SetVolume(AudioBus.Ui, .5f);
            var mixer = new AudioMixerLevels(null);
            mixer.Apply(settings);
            Assert.That(mixer.SourceGain(AudioBus.Music, routed: false), Is.EqualTo(.288f).Within(.0001f));
            settings.SetVolume(AudioBus.Music, 0f);
            mixer.Apply(settings);
            Assert.AreEqual(0f, mixer.SourceGain(AudioBus.Music, routed: false));
            Assert.That(mixer.SourceGain(AudioBus.Sfx, routed: false), Is.EqualTo(.6f).Within(.0001f));
            Assert.That(mixer.SourceGain(AudioBus.Ui, routed: false), Is.EqualTo(.4f).Within(.0001f));

            settings.AudioMuted = true;
            mixer.Apply(settings);
            foreach (var bus in new[] { AudioBus.Music, AudioBus.Sfx, AudioBus.Ui })
                Assert.AreEqual(0f, mixer.SourceGain(bus, routed: false));
            settings.AudioMuted = false;
            mixer.Apply(settings);
            Assert.That(mixer.SourceGain(AudioBus.Sfx, routed: false), Is.EqualTo(.6f).Within(.0001f));
            Assert.That(mixer.SourceGain(AudioBus.Ui, routed: false), Is.EqualTo(.4f).Within(.0001f));
        }
    }
}
