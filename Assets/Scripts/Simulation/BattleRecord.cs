using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Simulation
{
    /// <summary>One battle of a simulated match.</summary>
    public sealed class BattleRecord
    {
        internal BattleRecord(int fieldId, string fieldCardId, PlayerId host)
        {
            FieldId = fieldId;
            FieldCardId = fieldCardId;
            Host = host;
        }

        public int FieldId { get; }
        public string FieldCardId { get; }
        public PlayerId Host { get; }
        public PlayerId Guest => Host.Opponent();

        /// <summary>Who acted first in the battle: normally the creature placed earlier.</summary>
        public PlayerId FirstActor { get; internal set; }

        /// <summary>How equal final power was decided, if it was.</summary>
        public TieBreak TieBreak { get; internal set; }
        public string HostCreatureId { get; internal set; }
        public string GuestCreatureId { get; internal set; }
        public Element HostElement { get; internal set; }
        public Element GuestElement { get; internal set; }

        /// <summary>The field was open when the battle ended.</summary>
        public bool FieldOpen { get; internal set; }

        /// <summary>The battle ended by «Отступление»: no winner and no draw.</summary>
        public bool Retreated { get; internal set; }

        public PlayerId? Winner { get; internal set; }
        public bool IsDraw => !Retreated && Winner == null;

        public string CreatureOf(PlayerId player) => player == Host ? HostCreatureId : GuestCreatureId;
        public Element ElementOf(PlayerId player) => player == Host ? HostElement : GuestElement;
    }
}
