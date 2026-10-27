using Game.Core.Events;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>Adds a log line for each event at its turn in the animation queue.</summary>
    public sealed class LogPresenter : IEventPresenter
    {
        private readonly EventLogFormatter _formatter;
        private readonly EventLogView _log;

        public LogPresenter(EventLogFormatter formatter, EventLogView log)
        {
            _formatter = formatter;
            _log = log;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            var line = _formatter.Format(gameEvent);
            if (line != null)
                context.Enqueue(() => _log.Add(line));
        }
    }
}
