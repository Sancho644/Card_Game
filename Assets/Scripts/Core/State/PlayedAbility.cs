using System.Linq;
using Game.Core.Cards;
using Game.Core.Effects;

namespace Game.Core.State
{
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
