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
        }

        public int FieldId { get; }
        public PlayerId Host { get; }
        public PlayerId Guest { get; }
        public PlayerId CurrentActor { get; }

        public bool IsReady(PlayerId player) => _ready[(int)player];
    }
}
