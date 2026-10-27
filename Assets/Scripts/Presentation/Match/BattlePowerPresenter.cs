using System.Collections.Generic;
using DG.Tweening;
using Game.Core.Battle;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Cards;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Shows each battling creature's current power on its card and, after every change (the field opened,
    /// an ability played or cancelled), what changed it: «+50 Кузница» in green, «−30 Тень» in red.
    /// The numbers come from the engine's own count, so they always match the final result.
    /// </summary>
    public sealed class BattlePowerPresenter
    {
        private readonly CardViewRegistry _registry;
        private readonly FloatingTextSpawner _floatingText;
        private readonly PowerDiff _diff;
        private readonly VisualSettings _visual;
        private readonly TablePresentationSettings _settings;
        private readonly HashSet<int> _badged = new HashSet<int>();
        private BattlePower _shown;

        public BattlePowerPresenter(
            CardViewRegistry registry,
            FloatingTextSpawner floatingText,
            PowerDiff diff,
            VisualSettings visual,
            TablePresentationSettings settings)
        {
            _registry = registry;
            _floatingText = floatingText;
            _diff = diff;
            _visual = visual;
            _settings = settings;
        }

        /// <summary>Queues the update of the power badges to the battle as it stands after the batch.</summary>
        /// <param name="power">The engine's current count, null outside a battle.</param>
        public void Present(BattlePower power, PresentationContext context)
        {
            var previous = _shown != null && power != null && _shown.FieldId == power.FieldId ? _shown : null;
            _shown = power;
            if (power == null)
            {
                context.Enqueue(HideAll);
                return;
            }

            var field = context.View.Field(power.FieldId);
            if (field == null)
                return;

            context.Enqueue(new TweenStep(() => Show(power, previous, field)));
        }

        /// <summary>Forgets what was shown, e.g. for a new match.</summary>
        public void Reset()
        {
            _shown = null;
            HideAll();
        }

        private Tween Show(BattlePower power, BattlePower previous, FieldView field)
        {
            var sequence = DOTween.Sequence();
            var current = new HashSet<int>();
            foreach (var player in PlayerIds.All)
            {
                var creature = field.Occupant(player);
                var view = creature == null ? null : _registry.Find(creature.InstanceId);
                if (view == null)
                    continue;

                current.Add(creature.InstanceId);
                var now = power.Of(player);
                view.SetPower(now.FinalPower);
                if (previous == null)
                    continue;

                var before = previous.Of(player);
                if (before.FinalPower != now.FinalPower)
                {
                    var flash = view.FlashPower(now.FinalPower > before.FinalPower ? _visual.PowerUp : _visual.PowerDown, _settings.PowerPopupSeconds);
                    if (flash != null)
                        sequence.Join(flash);
                }

                var changes = _diff.Changes(before, now, power.FieldId);
                for (var i = 0; i < changes.Count; i++)
                {
                    var change = changes[i];
                    var text = (change.Amount > 0 ? "+" : "−") + System.Math.Abs(change.Amount) + " " + change.Source;
                    var color = change.Amount > 0 ? _visual.PowerUp : _visual.PowerDown;
                    sequence.Join(_floatingText.Show(text, color, view.PowerAnchor, i * 0.45f, _settings.PowerPopupSeconds));
                }
            }

            _badged.ExceptWith(current);
            HideAll();
            _badged.UnionWith(current);
            return sequence;
        }

        private void HideAll()
        {
            foreach (var id in _badged)
            {
                var view = _registry.Find(id);
                if (view != null)
                    view.SetPower(null);
            }

            _badged.Clear();
        }
    }
}
