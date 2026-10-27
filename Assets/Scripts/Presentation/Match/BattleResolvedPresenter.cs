using System.Collections.Generic;
using System.Linq;
using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>Shows the power breakdown and the consequences, and waits until the player continues.</summary>
    public sealed class BattleResolvedPresenter : IEventPresenter
    {
        private readonly BattleSummaryBuilder _summary;
        private readonly DialogView _dialog;
        private readonly UiTexts _texts;

        public BattleResolvedPresenter(BattleSummaryBuilder summary, DialogView dialog, UiTexts texts)
        {
            _summary = summary;
            _dialog = dialog;
            _texts = texts;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is BattleResolved result) || context.PreviousView?.Battle == null)
                return;

            var battle = context.PreviousView.Battle;
            var field = context.PreviousView.Field(battle.FieldId);
            var body = _summary.Build(
                result,
                battle.Host,
                field.Occupant(battle.Host).InstanceId,
                field.Occupant(battle.Guest).InstanceId,
                Consequences(context));
            context.Enqueue(new WaitForPlayerStep(done =>
                _dialog.Show(_summary.Title, body, _texts.Get(UiTextKeys.Continue), done)));
        }

        private static IReadOnlyList<GameEvent> Consequences(PresentationContext context) =>
            context.Batch.Skip(context.Index + 1)
                .TakeWhile(e => e is CreatureOut || e is CreatureReturnedToHand || e is FieldRemoved || e is WinScored)
                .ToList();
    }
}
