using Game.Core.State;

namespace Game.Core.Events
{
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
}
