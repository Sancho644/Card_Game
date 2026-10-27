using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Effects;

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

        /// <summary>All abilities played in this battle, in play order, cancelled ones included.</summary>
        public IReadOnlyList<PlayedAbility> PlayedAbilities => _playedAbilities;

        /// <summary>Abilities in force: played and not cancelled.</summary>
        public IEnumerable<PlayedAbility> ActiveAbilities => _playedAbilities.Where(played => !played.IsCancelled);

        /// <summary>An ability in force cancelled the field condition.</summary>
        public bool IsConditionCancelled => ActiveAbilities.Any(played => played.Has<IConditionCanceller>());

        /// <summary>Whether the field condition acts on this player's creature.</summary>
        public bool ConditionAppliesTo(PlayerId player) =>
            Field.IsOpen &&
            !IsConditionCancelled &&
            !ActiveAbilities.Any(played => played.Player == player && played.Has<IConditionImmunity>());

        public bool IsReady(PlayerId player) => _ready[(int)player];

        public int CountPlayed(PlayerId player, Func<AbilityCard, bool> filter) =>
            _playedAbilities.Count(played => played.Player == player && filter(played.Card));

        /// <summary>Whether an ability in force forbids this player to play the ability.</summary>
        public bool IsForbiddenByAbilities(AbilityCard ability, PlayerId player) =>
            ActiveAbilities.Any(played => played.Card.Effects
                .OfType<IAbilityRestriction>()
                .Any(restriction => restriction.Forbids(ability, player, played.Player)));

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
        public bool IsCancelled { get; internal set; }

        internal bool Has<TEffect>() where TEffect : IAbilityEffect => Card.Effects.OfType<TEffect>().Any();
    }
}
