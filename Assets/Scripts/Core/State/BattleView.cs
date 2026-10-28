using System.Collections.Generic;
using System.Linq;

namespace Game.Core.State
{
    public sealed class BattleView
    {
        private readonly bool[] _ready;

        internal BattleView(BattleState battle)
        {
            FieldId = battle.Field.Id;
            Host = battle.Host;
            Guest = battle.Guest;
            CurrentActor = battle.CurrentActor;
            _ready = PlayerIds.All.Select(battle.IsReady).ToArray();
            PlayedAbilities = battle.PlayedAbilities.Select(played => new PlayedAbilityView(played)).ToList();
        }

        public int FieldId { get; }
        public PlayerId Host { get; }
        public PlayerId Guest { get; }
        public PlayerId CurrentActor { get; }

        /// <summary>Abilities played in this battle, in play order, cancelled ones included. Played face up.</summary>
        public IReadOnlyList<PlayedAbilityView> PlayedAbilities { get; }

        public bool IsReady(PlayerId player) => _ready[(int)player];
    }
}
