using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>
    /// What a play effect may read and do. Effects change the match only through these operations,
    /// so every change is validated by the engine and reported as an event.
    /// </summary>
    public sealed class PlayContext
    {
        private readonly MatchContext _match;

        internal PlayContext(
            MatchContext match,
            PlayerId caster,
            CardInstance ability,
            AbilityTargets targets,
            BattleState battle,
            bool isBeforeBattle)
        {
            _match = match;
            Caster = caster;
            Ability = ability;
            Targets = targets ?? AbilityTargets.None;
            Battle = battle;
            IsBeforeBattle = isBeforeBattle;
        }

        public PlayerId Caster { get; }
        public PlayerId Opponent => Caster.Opponent();
        public CardInstance Ability { get; }
        public AbilityTargets Targets { get; }
        public MatchState State => _match.State;

        /// <summary>The battle the ability is played in, or null outside a battle.</summary>
        public BattleState Battle { get; }

        /// <summary>The ability is played together with placing its creature, before the battle starts.</summary>
        public bool IsBeforeBattle { get; }

        public void OpenBattleField() => _match.Battles.OpenField();

        public void CancelLastOpponentAbility(System.Func<AbilityCard, bool> filter) =>
            _match.Battles.CancelLastAbility(Opponent, filter);

        public void TakeFirstBattleTurn() => Battle.CurrentActor = Caster;

        public void MoveCreature(PlayerId owner, int creatureId, int fieldId) =>
            _match.Battles.MoveCreature(owner, creatureId, fieldId);

        public void Retreat() => _match.Battles.Retreat(Caster);

        public void ReturnCreatureFromOutOfGame(int creatureId)
        {
            var player = State.Player(Caster);
            var creature = player.OutOfGame.First(card => card.Id == creatureId);
            player.MoveOutOfGameToHand(creature);
            _match.Events.Emit(new CreatureReturnedToHand(Caster, creature.Id));
        }

        public void ReturnAbilityFromDiscard(int abilityId)
        {
            var player = State.Player(Caster);
            var ability = player.Discard.First(card => card.Id == abilityId);
            player.MoveDiscardToHand(ability);
            _match.Events.Emit(new AbilityReturnedToHand(Caster, CardRef.From(ability)));
        }

        public void DiscardFromHand(PlayerId owner, int abilityId)
        {
            var player = State.Player(owner);
            var ability = player.FindInHand(abilityId);
            player.RemoveFromHand(ability);
            player.AddToDiscard(ability);
            _match.Events.Emit(new AbilityDiscarded(owner, CardRef.From(ability)));
        }

        public void DiscardRandomAbility(PlayerId owner)
        {
            var abilities = AbilitiesInHand(owner);
            if (abilities.Count > 0)
                DiscardFromHand(owner, abilities[_match.Random.Next(abilities.Count)].Id);
        }

        public void LockField(int fieldId)
        {
            State.FindField(fieldId).IsLocked = true;
            _match.Events.Emit(new FieldLocked(fieldId));
        }

        /// <summary>Shows the caster the opponent's abilities in hand and, if given, one field card.</summary>
        public void RevealToCaster(FieldSlot field) =>
            _match.Events.Emit(new Scouted(
                Caster,
                AbilitiesInHand(Opponent).Select(CardRef.From).ToList(),
                field == null ? null : CardRef.From(field.Card)));

        public IReadOnlyList<CardInstance> AbilitiesInHand(PlayerId owner) =>
            State.Player(owner).Hand.Where(card => card.Type == CardType.Ability).ToList();
    }
}
