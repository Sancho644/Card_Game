using System.Collections.Generic;
using Game.Core.Cards;

namespace Game.Core.State
{
    public sealed class BattleState
    {
        private readonly bool[] _ready = new bool[PlayerIds.All.Count];
        private readonly List<PlayedAbility> _playedAbilities = new List<PlayedAbility>();

        internal BattleState(FieldSlot field, PlayerId firstActor)
        {
            Field = field;
            CurrentActor = firstActor;
        }

        public FieldSlot Field { get; }

        /// <summary>Owner of the field card the battle is on.</summary>
        public PlayerId Host => Field.Owner;

        public PlayerId Guest => Host.Opponent();
        public PlayerId CurrentActor { get; internal set; }

        /// <summary>Abilities in the order they were played; resolution applies them in this order.</summary>
        public IReadOnlyList<PlayedAbility> PlayedAbilities => _playedAbilities;

        public bool IsReady(PlayerId player) => _ready[(int)player];

        internal void SetReady(PlayerId player, bool ready) => _ready[(int)player] = ready;

        internal void AddPlayedAbility(PlayedAbility ability) => _playedAbilities.Add(ability);
    }

    public sealed class PlayedAbility
    {
        internal PlayedAbility(PlayerId player, CardInstance instance)
        {
            Player = player;
            Instance = instance;
        }

        public PlayerId Player { get; }
        public CardInstance Instance { get; }
        public AbilityCard Card => (AbilityCard)Instance.Definition;
    }
}
