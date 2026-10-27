using System.Collections.Generic;
using Game.Core.State;

namespace Game.Core.Events
{
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
}
