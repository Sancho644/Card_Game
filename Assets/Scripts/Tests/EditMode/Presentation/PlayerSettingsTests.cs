using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Services.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public class PlayerSettingsTests
    {
        [Test]
        public void FastAnimations_OffByDefault_SavedAndReportedWhenChanged()
        {
            var store = new MemorySettingsStore();
            var settings = new LocalPlayerSettingsService(store);
            var changes = 0;
            settings.Changed += () => changes++;

            Assert.IsFalse(settings.FastAnimations);
            settings.FastAnimations = true;
            settings.FastAnimations = true;

            Assert.AreEqual(1, changes, "Setting the same value again is not a change.");
            Assert.AreEqual(1, store.Saves);
            Assert.IsTrue(new LocalPlayerSettingsService(store).FastAnimations, "Kept for the next launch.");
        }

        [Test]
        public void TimeScale_MultipliesDesignerSpeed_FastAnimations_AndTapFastForward()
        {
            var animation = ScriptableObject.CreateInstance<AnimationSettings>();
            var data = new SerializedObject(animation);
            data.FindProperty("_speed").floatValue = 1.5f;
            data.FindProperty("_fastSpeed").floatValue = 2f;
            data.FindProperty("_tapFastForward").floatValue = 4f;
            data.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1.5f, AnimationSpeed.TimeScale(animation, fast: false, fastForward: false));
            Assert.AreEqual(3f, AnimationSpeed.TimeScale(animation, fast: true, fastForward: false));
            Assert.AreEqual(12f, AnimationSpeed.TimeScale(animation, fast: true, fastForward: true));
        }
    }
}
