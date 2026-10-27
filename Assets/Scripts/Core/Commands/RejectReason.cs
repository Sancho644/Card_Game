using Game.Core.Events;

namespace Game.Core.Commands
{
    public enum RejectReason
    {
        None,
        UnknownCommand,
        MatchOver,
        WrongPhase,
        NotYourTurn,
        NotCoinTossWinner,
        CardNotInHand,
        NotACreature,
        NotAnAbility,
        FieldNotFound,
        FieldRemoved,
        AlreadyOnField,
        MainActionAlreadyDone,
        MainActionRequired,
        HandHasCreatures,
        CreatureNotOnField,
        NoOpponentCreature,
        NotFieldOwner,
        FieldAlreadyOpen,
        TimingNotAllowed,
        LegendaryLimitReached,
        SignatureCreatureNotInBattle,
        BlockedByFieldCondition,
        BlockedByAbility,
        InvalidTarget,
        FieldLocked,
        SignatureNeedsBattle,
        NotAChoiceOption,
    }
}
