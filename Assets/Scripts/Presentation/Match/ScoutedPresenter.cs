using System.Linq;
using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Animation;
using Game.Presentation.Cards;
using Game.Presentation.Ui;

namespace Game.Presentation.Match
{
    /// <summary>Shows what «Разведка» revealed. The event is private, so only the caster ever gets here.</summary>
    public sealed class ScoutedPresenter : IEventPresenter
    {
        private readonly DialogView _dialog;
        private readonly CardFaceBuilder _faces;
        private readonly UiTexts _texts;

        public ScoutedPresenter(DialogView dialog, CardFaceBuilder faces, UiTexts texts)
        {
            _dialog = dialog;
            _faces = faces;
            _texts = texts;
        }

        public void Present(GameEvent gameEvent, PresentationContext context)
        {
            if (!(gameEvent is Scouted scouted))
                return;

            var abilities = string.Join(", ", scouted.OpponentAbilities.Select(card => _faces.NameOf(card.CardId)));
            var body = _texts.Format(UiTextKeys.ScoutAbilities, abilities);
            if (scouted.Field != null)
                body += "\n" + _texts.Format(UiTextKeys.ScoutField, _faces.NameOf(scouted.Field.CardId));
            context.Enqueue(new WaitForPlayerStep(done =>
                _dialog.Show(_texts.Get(UiTextKeys.ScoutTitle), body, _texts.Get(UiTextKeys.Continue), done)));
        }
    }
}
