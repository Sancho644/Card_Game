using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>
    /// A player's intent. Commands are plain data so they can travel over the network later;
    /// the engine validates every command and trusts nothing.
    /// </summary>
    public abstract class GameCommand
    {
        protected GameCommand(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }

    /// <summary>The coin toss winner decides whether to move first.</summary>
    public sealed class ChooseTurnOrder : GameCommand
    {
        public ChooseTurnOrder(PlayerId player, bool goFirst)
            : base(player)
        {
            GoFirst = goFirst;
        }

        public bool GoFirst { get; }
    }

    /// <summary>
    /// Main action: put a creature from hand on any field where you have no creature yet.
    /// If this starts a battle, the creature's signature ability may be played with it, before the battle.
    /// </summary>
    public sealed class PlaceCreature : GameCommand
    {
        public PlaceCreature(PlayerId player, int creatureId, int fieldId, int? signatureAbilityId = null)
            : base(player)
        {
            CreatureId = creatureId;
            FieldId = fieldId;
            SignatureAbilityId = signatureAbilityId;
        }

        public int CreatureId { get; }
        public int FieldId { get; }
        public int? SignatureAbilityId { get; }
    }

    /// <summary>Main action with no creatures in hand: move your creature to a field with an opponent's creature.</summary>
    public sealed class Challenge : GameCommand
    {
        public Challenge(PlayerId player, int creatureId, int targetFieldId, int? signatureAbilityId = null)
            : base(player)
        {
            CreatureId = creatureId;
            TargetFieldId = targetFieldId;
            SignatureAbilityId = signatureAbilityId;
        }

        public int CreatureId { get; }
        public int TargetFieldId { get; }

        /// <summary>The moved creature's signature ability, played before the battle.</summary>
        public int? SignatureAbilityId { get; }
    }

    /// <summary>The host turns the battle's field card face up.</summary>
    public sealed class OpenField : GameCommand
    {
        public OpenField(PlayerId player)
            : base(player)
        {
        }
    }

    public sealed class PlayAbility : GameCommand
    {
        public PlayAbility(PlayerId player, int abilityId, AbilityTargets targets = null)
            : base(player)
        {
            AbilityId = abilityId;
            Targets = targets ?? AbilityTargets.None;
        }

        public int AbilityId { get; }
        public AbilityTargets Targets { get; }
    }

    /// <summary>What an ability is aimed at. Each effect reads only the targets it needs.</summary>
    public sealed class AbilityTargets
    {
        public static readonly AbilityTargets None = new AbilityTargets();

        public AbilityTargets(int? creatureId = null, int? fieldId = null, int? cardId = null)
        {
            CreatureId = creatureId;
            FieldId = fieldId;
            CardId = cardId;
        }

        public int? CreatureId { get; }
        public int? FieldId { get; }

        /// <summary>A card in a hand or a pile, e.g. an ability to return from the discard pile.</summary>
        public int? CardId { get; }
    }

    /// <summary>The player has nothing more to do in this battle.</summary>
    public sealed class Ready : GameCommand
    {
        public Ready(PlayerId player)
            : base(player)
        {
        }
    }

    /// <summary>Answers a pending choice (see MatchState.PendingChoice).</summary>
    public sealed class ChooseCard : GameCommand
    {
        public ChooseCard(PlayerId player, int cardId)
            : base(player)
        {
            CardId = cardId;
        }

        public int CardId { get; }
    }

    /// <summary>Closes the "after" window. Without a main action it counts as a skip, allowed only if none was possible.</summary>
    public sealed class EndTurn : GameCommand
    {
        public EndTurn(PlayerId player)
            : base(player)
        {
        }
    }
}
