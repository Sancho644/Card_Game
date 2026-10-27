using System.Collections.Generic;
using System.Linq;
using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>
    /// Shows the battle result and its consequences, with the power count step by step one button away,
    /// and waits until the player continues.
    /// </summary>
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
            var consequences = Consequences(context);
            var summary = _summary.BuildShort(result, battle.Host, consequences);
            var details = _summary.BuildDetails(
                result,
                battle.Host,
                field.Occupant(battle.Host).InstanceId,
                field.Occupant(battle.Guest).InstanceId,
                consequences);

            context.Enqueue(new WaitForPlayerStep(done =>
            {
                void ShowSummary() => _dialog.Show(
                    _summary.Title, summary, _texts.Get(UiTextKeys.Continue), done, _texts.Get(UiTextKeys.BreakdownDetails), ShowDetails);

                void ShowDetails() => _dialog.Show(
                    _summary.Title, details, _texts.Get(UiTextKeys.Continue), done, _texts.Get(UiTextKeys.BreakdownBack), ShowSummary);

                ShowSummary();
            }));
        }

        private static IReadOnlyList<GameEvent> Consequences(PresentationContext context) =>
            context.Batch.Skip(context.Index + 1)
                .TakeWhile(e => e is CreatureOut || e is CreatureReturnedToHand || e is FieldRemoved || e is WinScored)
                .ToList();
    }
}
