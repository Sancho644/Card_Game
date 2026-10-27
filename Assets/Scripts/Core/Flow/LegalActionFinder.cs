using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Effects;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>
    /// Lists every command a player may send right now: builds the candidates and keeps those the
    /// engine's own validation accepts. Clients and AI use this list instead of checking rules themselves.
    /// </summary>
    internal sealed class LegalActionFinder
    {
        private readonly MatchState _state;
        private readonly Func<GameCommand, RejectReason> _validate;

        public LegalActionFinder(MatchState state, Func<GameCommand, RejectReason> validate)
        {
            _state = state;
            _validate = validate;
        }

        public IReadOnlyList<GameCommand> Find(PlayerId player) =>
            Candidates(player).Where(command => _validate(command) == RejectReason.None).ToList();

        private IEnumerable<GameCommand> Candidates(PlayerId player)
        {
            switch (_state.Phase)
            {
                case MatchPhase.ChoosingTurnOrder:
                    return new GameCommand[] { new ChooseTurnOrder(player, true), new ChooseTurnOrder(player, false) };
                case MatchPhase.AwaitingChoice:
                    return _state.PendingChoice.Options.Select(option => (GameCommand)new ChooseCard(player, option.Id));
                case MatchPhase.Turn:
                case MatchPhase.Battle:
                    return MainActions(player)
                        .Concat(AbilityPlays(player))
                        .Concat(new GameCommand[] { new OpenField(player), new Ready(player), new EndTurn(player) });
                default:
                    return Enumerable.Empty<GameCommand>();
            }
        }

        private IEnumerable<GameCommand> MainActions(PlayerId player)
        {
            var hand = _state.Player(player).Hand;
            var signatures = new int?[] { null }
                .Concat(hand.Where(IsSignatureAbility).Select(card => (int?)card.Id))
                .ToList();
            var fields = _state.Fields.Where(field => !field.IsRemoved).ToList();

            foreach (var creature in hand.Where(card => card.Type == CardType.Creature))
            {
                foreach (var field in fields)
                {
                    foreach (var signature in signatures)
                        yield return new PlaceCreature(player, creature.Id, field.Id, signature);
                }
            }

            foreach (var creature in fields.Select(field => field.OccupantOf(player)).Where(card => card != null))
            {
                foreach (var field in fields)
                {
                    foreach (var signature in signatures)
                        yield return new Challenge(player, creature.Id, field.Id, signature);
                }
            }
        }

        private IEnumerable<GameCommand> AbilityPlays(PlayerId player)
        {
            foreach (var card in _state.Player(player).Hand.Where(card => card.Type == CardType.Ability))
            {
                var kinds = ((AbilityCard)card.Definition).Effects
                    .OfType<IPlayEffect>()
                    .Aggregate(AbilityTargetKinds.None, (all, effect) => all | effect.Targets);
                foreach (var targets in TargetOptions(player, kinds))
                    yield return new PlayAbility(player, card.Id, targets);
            }
        }

        private IEnumerable<AbilityTargets> TargetOptions(PlayerId player, AbilityTargetKinds kinds)
        {
            yield return AbilityTargets.None;
            if (kinds == AbilityTargetKinds.None)
                yield break;

            var creatures = Option(kinds, AbilityTargetKinds.Creature, () => _state.Fields
                .SelectMany(field => PlayerIds.All.Select(field.OccupantOf))
                .Where(card => card != null)
                .Select(card => card.Id));
            var fields = Option(kinds, AbilityTargetKinds.Field, () => _state.Fields
                .Where(field => !field.IsRemoved)
                .Select(field => field.Id));
            var cards = Option(kinds, AbilityTargetKinds.Card, () =>
            {
                var own = _state.Player(player);
                return own.Hand.Concat(own.Discard).Concat(own.OutOfGame).Select(card => card.Id);
            });

            foreach (var creature in creatures)
            {
                foreach (var field in fields)
                {
                    foreach (var card in cards)
                    {
                        if (creature != null || field != null || card != null)
                            yield return new AbilityTargets(creature, field, card);
                    }
                }
            }
        }

        private static IReadOnlyList<int?> Option(AbilityTargetKinds kinds, AbilityTargetKinds kind, Func<IEnumerable<int>> ids) =>
            (kinds & kind) == 0 ? new int?[] { null } : ids().Select(id => (int?)id).ToList();

        private static bool IsSignatureAbility(CardInstance card) =>
            card.Definition is AbilityCard ability && ability.IsSignature;
    }
}
