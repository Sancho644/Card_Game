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

    /// <summary>Main action: put a creature from hand on any field where you have no creature yet.</summary>
    public sealed class PlaceCreature : GameCommand
    {
        public PlaceCreature(PlayerId player, int creatureId, int fieldId)
            : base(player)
        {
            CreatureId = creatureId;
            FieldId = fieldId;
        }

        public int CreatureId { get; }
        public int FieldId { get; }
    }

    /// <summary>Main action with no creatures in hand: move your creature to a field with an opponent's creature.</summary>
    public sealed class Challenge : GameCommand
    {
        public Challenge(PlayerId player, int creatureId, int targetFieldId)
            : base(player)
        {
            CreatureId = creatureId;
            TargetFieldId = targetFieldId;
        }

        public int CreatureId { get; }
        public int TargetFieldId { get; }
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
        public PlayAbility(PlayerId player, int abilityId)
            : base(player)
        {
            AbilityId = abilityId;
        }

        public int AbilityId { get; }
    }

    /// <summary>The player has nothing more to do in this battle.</summary>
    public sealed class Ready : GameCommand
    {
        public Ready(PlayerId player)
            : base(player)
        {
        }
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
