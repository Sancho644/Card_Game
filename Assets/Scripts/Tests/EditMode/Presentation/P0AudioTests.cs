using System;
using System.Linq;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    public sealed class P0AudioTests
    {
        private const string SettingsPath = "Assets/Data/Config/SoundSettings.asset";

        [Test]
        public void SerializedCueNumbersStayStable()
        {
            var previous = new[]
            {
                "CardDeal", "CardPick", "CardDrop", "CardPlace", "FieldFlip", "BattleStart", "AbilityPlay",
                "AbilityDiscard", "PowerUp", "PowerDown", "BattleWin", "CreatureOut", "FieldRemove", "Banner",
                "TurnStart", "Victory", "Defeat", "Draw", "Button"
            };
            for (var index = 0; index < previous.Length; index++)
                Assert.AreEqual(previous[index], Enum.GetName(typeof(SoundCue), index));
            Assert.AreEqual(19, (int)SoundCue.FieldPlace);
            Assert.AreEqual(20, (int)SoundCue.BattleLose);
        }

        [TestCase(SoundCue.CardDeal, "Cards/card-deal", 3, .08f, .16f)]
        [TestCase(SoundCue.CardPick, "Cards/card-pick", 1, .06f, .12f)]
        [TestCase(SoundCue.CardPlace, "Cards/card-place-creature", 2, .10f, .22f)]
        [TestCase(SoundCue.FieldPlace, "Cards/card-place-field", 1, .18f, .32f)]
        [TestCase(SoundCue.FieldFlip, "Cards/card-field-reveal", 1, .20f, .42f)]
        [TestCase(SoundCue.AbilityDiscard, "Cards/card-discard", 1, .08f, .19f)]
        [TestCase(SoundCue.BattleStart, "Battle/battle-start", 1, .45f, .85f)]
        [TestCase(SoundCue.PowerUp, "Battle/battle-power-up", 1, .13f, .27f)]
        [TestCase(SoundCue.PowerDown, "Battle/battle-power-down", 1, .13f, .27f)]
        [TestCase(SoundCue.BattleWin, "Battle/battle-win", 1, .35f, .70f)]
        [TestCase(SoundCue.BattleLose, "Battle/battle-lose", 1, .35f, .70f)]
        [TestCase(SoundCue.CreatureOut, "Battle/battle-creature-out", 1, .20f, .40f)]
        [TestCase(SoundCue.FieldRemove, "Battle/battle-field-remove", 1, .24f, .45f)]
        [TestCase(SoundCue.TurnStart, "Battle/turn-local", 1, .25f, .50f)]
        [TestCase(SoundCue.AbilityPlay, "Abilities/ability-common", 2, .16f, .35f)]
        [TestCase(SoundCue.Button, "UI/ui-click", 1, .05f, .11f)]
        public void AssignedClipsUseShortMonoPcmOnBothPlatforms(SoundCue cue, string name, int count, float min, float max)
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoundSettings>(SettingsPath);
            var entry = settings.Find(cue);
            Assert.That(entry.Variants?.Length ?? 0, Is.EqualTo(count - 1));
            for (var index = 0; index < count; index++)
            {
                var clip = index == 0 ? entry.Clip : entry.Variants[index - 1];
                var suffix = index == 0 ? "" : $"-{index + 1:D2}";
                var path = "Assets/Audio/SFX/" + name + suffix + ".wav";
                Assert.IsNotNull(clip, path);
                Assert.AreEqual(path, AssetDatabase.GetAssetPath(clip));
                Assert.AreEqual(1, clip.channels);
                Assert.AreEqual(44100, clip.frequency);
                Assert.That(clip.length, Is.InRange(min, max));
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                Assert.IsTrue(importer.forceToMono);
                foreach (var group in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
                {
                    Assert.IsTrue(importer.ContainsSampleSettingsOverride(group), group.ToString());
                    var sample = importer.GetOverrideSampleSettings(group);
                    Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, sample.loadType);
                    Assert.AreEqual(AudioCompressionFormat.PCM, sample.compressionFormat);
                    Assert.AreEqual(AudioSampleRateSetting.PreserveSampleRate, sample.sampleRateSetting);
                    Assert.IsTrue(sample.preloadAudioData);
                }
            }
        }

        [Test]
        public void EachCueHasOneEntryAndDrawStaysUnassigned()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoundSettings>(SettingsPath);
            using var serialized = new SerializedObject(settings);
            var entries = serialized.FindProperty("_sounds");
            var assigned = Enumerable.Range(0, entries.arraySize)
                .Select(index => entries.GetArrayElementAtIndex(index).FindPropertyRelative("Cue").intValue);
            CollectionAssert.AreEquivalent(Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>().Select(cue => (int)cue), assigned);
            Assert.IsNull(settings.Find(SoundCue.Draw).Clip);
        }

        [Test]
        public void PlayerUsesSeparateEffectsAndButtonMixerRoutes()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoundSettings>(SettingsPath);
            var sfx = settings.Mixer.FindMatchingGroups("Master/SFX").Single();
            var ui = settings.Mixer.FindMatchingGroups("Master/UI").Single();
            var root = new GameObject("P0 audio test");
            try
            {
                var player = root.AddComponent<AudioSourceSoundPlayer>();
                player.Configure(settings, sfx, ui);
                var sources = root.GetComponents<AudioSource>();
                Assert.AreEqual(5, sources.Length);
                Assert.AreEqual(4, sources.Count(source => source.outputAudioMixerGroup == sfx));
                Assert.AreEqual(1, sources.Count(source => source.outputAudioMixerGroup == ui));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(PlayerId.Player1, false, SoundCue.BattleWin)]
        [TestCase(PlayerId.Player2, false, SoundCue.BattleLose)]
        [TestCase(PlayerId.Player2, true, SoundCue.BattleWin)]
        [TestCase(null, false, SoundCue.BattleDraw)]
        public void BattleOutcomeFollowsTheViewer(PlayerId? winner, bool hotSeat, SoundCue? cue) =>
            Assert.AreEqual(cue, BattleOutcomeAudio.ResultCue(winner, PlayerId.Player1, hotSeat));
    }
}
