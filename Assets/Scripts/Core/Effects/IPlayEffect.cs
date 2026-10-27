using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>Changes the match at the moment the ability is played.</summary>
    public interface IPlayEffect : IAbilityEffect
    {
        /// <summary>The targets this effect reads; the engine offers only these when listing legal actions.</summary>
        AbilityTargetKinds Targets { get; }

        /// <summary>Checks targets and preconditions without changing anything.</summary>
        RejectReason Validate(PlayContext context);

        void Apply(PlayContext context);
    }
}
