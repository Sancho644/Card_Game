using System.Linq;
using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>Returns one of your creatures that is out of the game to your hand. Target: CardId.</summary>
    public sealed class ReturnCreatureFromOutOfGameEffect : IPlayEffect
    {
        public AbilityTargetKinds Targets => AbilityTargetKinds.Card;

        public RejectReason Validate(PlayContext context)
        {
            var id = context.Targets.CardId;
            return id != null && context.State.Player(context.Caster).OutOfGame.Any(card => card.Id == id)
                ? RejectReason.None
                : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context) => context.ReturnCreatureFromOutOfGame(context.Targets.CardId.Value);
    }
}
