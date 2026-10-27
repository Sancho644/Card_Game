using System.Collections.Generic;
using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>
    /// Something that happened in the match. Presentation is built from events.
    /// An event with <see cref="VisibleOnlyTo"/> set must not be shown to the other player.
    /// </summary>
    public abstract class GameEvent
    {
        protected GameEvent(PlayerId? visibleOnlyTo = null)
        {
            VisibleOnlyTo = visibleOnlyTo;
        }

        public PlayerId? VisibleOnlyTo { get; }

        public bool IsVisibleTo(PlayerId player) => VisibleOnlyTo == null || VisibleOnlyTo == player;
    }

    // ---- Match setup ----

    /// <summary>A player's field cards are put face down on the table.</summary>
    public sealed class FieldsDealt : GameEvent
    {
        public FieldsDealt(PlayerId owner, IReadOnlyList<int> fieldIds)
        {
            Owner = owner;
            FieldIds = fieldIds;
        }

        public PlayerId Owner { get; }
        public IReadOnlyList<int> FieldIds { get; }
    }

    /// <summary>A player's starting hand. Private to that player.</summary>
    public sealed class HandDealt : GameEvent
    {
        public HandDealt(PlayerId owner, IReadOnlyList<CardRef> cards)
            : base(owner)
        {
            Owner = owner;
            Cards = cards;
        }

        public PlayerId Owner { get; }
        public IReadOnlyList<CardRef> Cards { get; }
    }

    public sealed class CoinTossed : GameEvent
    {
        public CoinTossed(PlayerId winner)
        {
            Winner = winner;
        }

        public PlayerId Winner { get; }
    }

    public sealed class TurnOrderChosen : GameEvent
    {
        public TurnOrderChosen(PlayerId chooser, PlayerId firstPlayer)
        {
            Chooser = chooser;
            FirstPlayer = firstPlayer;
        }

        public PlayerId Chooser { get; }
        public PlayerId FirstPlayer { get; }
    }

    // ---- Turns ----

    public sealed class TurnStarted : GameEvent
    {
        public TurnStarted(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }

    public sealed class CreaturePlaced : GameEvent
    {
        public CreaturePlaced(PlayerId player, CardRef creature, int fieldId)
        {
            Player = player;
            Creature = creature;
            FieldId = fieldId;
        }

        public PlayerId Player { get; }
        public CardRef Creature { get; }
        public int FieldId { get; }
    }

    /// <summary>A challenge: the creature moved to a field where an opponent's creature stands.</summary>
    public sealed class CreatureMoved : GameEvent
    {
        public CreatureMoved(PlayerId player, int creatureId, int fromFieldId, int toFieldId)
        {
            Player = player;
            CreatureId = creatureId;
            FromFieldId = fromFieldId;
            ToFieldId = toFieldId;
        }

        public PlayerId Player { get; }
        public int CreatureId { get; }
        public int FromFieldId { get; }
        public int ToFieldId { get; }
    }

    public sealed class MainActionSkipped : GameEvent
    {
        public MainActionSkipped(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }

    public sealed class TurnEnded : GameEvent
    {
        public TurnEnded(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }

    // ---- Battle ----

    public sealed class BattleStarted : GameEvent
    {
        public BattleStarted(int fieldId, PlayerId host, PlayerId guest, PlayerId firstActor)
        {
            FieldId = fieldId;
            Host = host;
            Guest = guest;
            FirstActor = firstActor;
        }

        public int FieldId { get; }
        public PlayerId Host { get; }
        public PlayerId Guest { get; }
        public PlayerId FirstActor { get; }
    }

    public sealed class BattleTurnPassed : GameEvent
    {
        public BattleTurnPassed(PlayerId actor)
        {
            Actor = actor;
        }

        public PlayerId Actor { get; }
    }

    /// <summary>The field card is turned face up; its content becomes public.</summary>
    public sealed class FieldOpened : GameEvent
    {
        public FieldOpened(int fieldId, string cardId)
        {
            FieldId = fieldId;
            CardId = cardId;
        }

        public int FieldId { get; }
        public string CardId { get; }
    }

    public sealed class AbilityPlayed : GameEvent
    {
        public AbilityPlayed(PlayerId player, CardRef ability)
        {
            Player = player;
            Ability = ability;
        }

        public PlayerId Player { get; }
        public CardRef Ability { get; }
    }

    public sealed class PlayerReady : GameEvent
    {
        public PlayerReady(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }

    public sealed class ReadinessReset : GameEvent
    {
        public ReadinessReset(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }

    public sealed class BattleResolved : GameEvent
    {
        public BattleResolved(int fieldId, int hostPower, int guestPower, PlayerId? winner)
        {
            FieldId = fieldId;
            HostPower = hostPower;
            GuestPower = guestPower;
            Winner = winner;
        }

        public int FieldId { get; }
        public int HostPower { get; }
        public int GuestPower { get; }

        /// <summary>Null on a draw.</summary>
        public PlayerId? Winner { get; }
    }

    public sealed class CreatureOut : GameEvent
    {
        public CreatureOut(PlayerId owner, int creatureId)
        {
            Owner = owner;
            CreatureId = creatureId;
        }

        public PlayerId Owner { get; }
        public int CreatureId { get; }
    }

    public sealed class CreatureReturnedToHand : GameEvent
    {
        public CreatureReturnedToHand(PlayerId owner, int creatureId)
        {
            Owner = owner;
            CreatureId = creatureId;
        }

        public PlayerId Owner { get; }
        public int CreatureId { get; }
    }

    public sealed class FieldRemoved : GameEvent
    {
        public FieldRemoved(int fieldId)
        {
            FieldId = fieldId;
        }

        public int FieldId { get; }
    }

    public sealed class WinScored : GameEvent
    {
        public WinScored(PlayerId player, int totalWins)
        {
            Player = player;
            TotalWins = totalWins;
        }

        public PlayerId Player { get; }
        public int TotalWins { get; }
    }

    /// <summary>The battle ended without a power count because a player withdrew the creature.</summary>
    public sealed class BattleRetreated : GameEvent
    {
        public BattleRetreated(int fieldId, PlayerId player)
        {
            FieldId = fieldId;
            Player = player;
        }

        public int FieldId { get; }
        public PlayerId Player { get; }
    }

    // ---- Ability effects ----

    public sealed class AbilityCancelled : GameEvent
    {
        public AbilityCancelled(PlayerId owner, CardRef ability)
        {
            Owner = owner;
            Ability = ability;
        }

        public PlayerId Owner { get; }
        public CardRef Ability { get; }
    }

    /// <summary>An ability went from a hand to the discard pile without being played.</summary>
    public sealed class AbilityDiscarded : GameEvent
    {
        public AbilityDiscarded(PlayerId owner, CardRef ability)
        {
            Owner = owner;
            Ability = ability;
        }

        public PlayerId Owner { get; }
        public CardRef Ability { get; }
    }

    /// <summary>A used ability came back from the discard pile to the hand.</summary>
    public sealed class AbilityReturnedToHand : GameEvent
    {
        public AbilityReturnedToHand(PlayerId owner, CardRef ability)
        {
            Owner = owner;
            Ability = ability;
        }

        public PlayerId Owner { get; }
        public CardRef Ability { get; }
    }

    public sealed class FieldLocked : GameEvent
    {
        public FieldLocked(int fieldId)
        {
            FieldId = fieldId;
        }

        public int FieldId { get; }
    }

    /// <summary>The scouting player sees the opponent's abilities and one closed field. Private to that player.</summary>
    public sealed class Scouted : GameEvent
    {
        public Scouted(PlayerId player, IReadOnlyList<CardRef> opponentAbilities, CardRef field)
            : base(player)
        {
            Player = player;
            OpponentAbilities = opponentAbilities;
            Field = field;
        }

        public PlayerId Player { get; }
        public IReadOnlyList<CardRef> OpponentAbilities { get; }

        /// <summary>The revealed field card, or null if the opponent had no closed field.</summary>
        public CardRef Field { get; }
    }

    public sealed class ChoiceRequested : GameEvent
    {
        public ChoiceRequested(PlayerId player, ChoicePurpose purpose, IReadOnlyList<CardRef> options)
        {
            Player = player;
            Purpose = purpose;
            Options = options;
        }

        public PlayerId Player { get; }
        public ChoicePurpose Purpose { get; }
        public IReadOnlyList<CardRef> Options { get; }
    }

    // ---- Match end ----

    public sealed class MatchEnded : GameEvent
    {
        public MatchEnded(PlayerId? winner)
        {
            Winner = winner;
        }

        /// <summary>Null on a draw.</summary>
        public PlayerId? Winner { get; }
    }
}
