using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Commands;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// Walks the player through a legal command step by step: pick a source card, then targets on the
    /// table, then options from a list. It only narrows the engine's list; it never decides what is legal.
    /// </summary>
    public sealed class ActionSelector
    {
        private readonly ActionPathBuilder _builder = new ActionPathBuilder();
        private IReadOnlyList<ActionPath> _paths = new ActionPath[0];
        private List<ActionPath> _candidates = new List<ActionPath>();
        private int _step;
        private bool _active;

        public event Action Changed;
        public event Action<GameCommand> CommandChosen;

        /// <summary>Options to pick from a list; the receiver answers with <see cref="ChooseOption"/>.</summary>
        public event Action<IReadOnlyList<int>> OptionsRequested;

        public bool IsSelecting => _active;

        /// <summary>The selected source card, or null.</summary>
        public int? Source { get; private set; }

        /// <summary>Cards that start a legal action, shown as selectable while nothing is selected.</summary>
        public IReadOnlyCollection<int> Sources =>
            _paths.Where(path => path.Source != null).Select(path => path.Source.Value).Distinct().ToList();

        /// <summary>Cards that are valid targets at the current step.</summary>
        public IReadOnlyCollection<int> Targets =>
            Pending().Where(path => path.Steps[_step].Kind == PathStepKind.Card)
                .Select(path => path.Steps[_step].Value)
                .Distinct()
                .ToList();

        public void SetActions(IReadOnlyList<GameCommand> commands)
        {
            _paths = _builder.Build(commands);
            Reset(notify: false);
            if (_paths.Count > 0 && _paths.All(path => path.Source == null))
                Begin(null);
            else
                Changed?.Invoke();
        }

        public void ClearActions()
        {
            _paths = new ActionPath[0];
            Reset(notify: true);
        }

        public void TapCard(int instanceId)
        {
            if (_active && Targets.Contains(instanceId))
                Advance(instanceId);
            else if (_active && Source == instanceId)
                Cancel();
            else if (Sources.Contains(instanceId))
                Begin(instanceId);
            else if (_active)
                Cancel();
        }

        public void ChooseOption(int value) => Advance(value);

        public void Cancel()
        {
            Reset(notify: true);
            if (_paths.Count > 0 && _paths.All(path => path.Source == null))
                Begin(null);
        }

        private void Begin(int? source)
        {
            _active = true;
            Source = source;
            _step = 0;
            _candidates = _paths.Where(path => path.Source == source).ToList();
            Resolve();
        }

        private void Advance(int value)
        {
            _candidates = Pending().Where(path => path.Steps[_step].Value == value).ToList();
            _step++;
            Resolve();
        }

        private void Resolve()
        {
            var pending = Pending().ToList();
            var complete = _candidates.Where(path => path.Steps.Count == _step).ToList();

            if (pending.Count == 0)
            {
                var command = complete.Count == 1 ? complete[0].Command : null;
                Reset(notify: true);
                if (command != null)
                    CommandChosen?.Invoke(command);
                return;
            }

            Changed?.Invoke();
            if (pending[0].Steps[_step].Kind == PathStepKind.Option)
                OptionsRequested?.Invoke(pending.Select(path => path.Steps[_step].Value).Distinct().ToList());
        }

        private IEnumerable<ActionPath> Pending() =>
            _active ? _candidates.Where(path => path.Steps.Count > _step) : Enumerable.Empty<ActionPath>();

        private void Reset(bool notify)
        {
            _active = false;
            Source = null;
            _step = 0;
            _candidates = new List<ActionPath>();
            if (notify)
                Changed?.Invoke();
        }
    }
}
