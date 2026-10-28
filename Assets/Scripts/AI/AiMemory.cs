using System.Collections.Generic;
using System.Linq;
using Game.Core.Events;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>
    /// The opponent's cards the AI's player has seen during the match, by instance id: from the events it
    /// may see and from its views. Lets the AI know a creature that went back to the opponent's hand and
    /// what scouting revealed.
    /// </summary>
    public sealed class AiMemory
    {
        private readonly PlayerId _opponent;
        private readonly Dictionary<int, string> _opponentCards = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _fields = new Dictionary<int, string>();

        public AiMemory(PlayerId player)
        {
            _opponent = player.Opponent();
        }

        /// <summary>The opponent's creatures and abilities seen so far: instance id → card id.</summary>
        public IReadOnlyDictionary<int, string> OpponentCards => _opponentCards;

        /// <summary>Field cards seen so far, of both players: field id → card id.</summary>
        public IReadOnlyDictionary<int, string> Fields => _fields;

        public void Observe(IEnumerable<GameEvent> visibleEvents)
        {
            foreach (var gameEvent in visibleEvents)
            {
                switch (gameEvent)
                {
                    case CreaturePlaced placed:
                        RememberCard(placed.Player, placed.Creature);
                        break;
                    case AbilityPlayed played:
                        RememberCard(played.Player, played.Ability);
                        break;
                    case AbilityDiscarded discarded:
                        RememberCard(discarded.Owner, discarded.Ability);
                        break;
                    case AbilityReturnedToHand returned:
                        RememberCard(returned.Owner, returned.Ability);
                        break;
                    case ChoiceRequested choice:
                        foreach (var option in choice.Options)
                            RememberCard(choice.Player, option);
                        break;
                    case FieldOpened opened:
                        _fields[opened.FieldId] = opened.CardId;
                        break;
                    case Scouted scouted:
                        foreach (var ability in scouted.OpponentAbilities)
                            RememberCard(_opponent, ability);
                        if (scouted.Field != null)
                            _fields[scouted.Field.InstanceId] = scouted.Field.CardId;
                        break;
                }
            }
        }

        /// <summary>Everything public in a view: creatures on the table, piles, open fields, abilities in the battle.</summary>
        public void ObserveView(PlayerView view)
        {
            foreach (var field in view.Fields)
            {
                if (field.CardId != null)
                    _fields[field.Id] = field.CardId;
                RememberCard(_opponent, field.Occupant(_opponent));
            }

            foreach (var card in view.Discard(_opponent).Concat(view.OutOfGame(_opponent)))
                RememberCard(_opponent, card);
            if (view.Battle != null)
            {
                foreach (var played in view.Battle.PlayedAbilities)
                    RememberCard(played.Player, played.Card);
            }
        }

        private void RememberCard(PlayerId owner, CardRef card)
        {
            if (owner == _opponent && card != null)
                _opponentCards[card.InstanceId] = card.CardId;
        }
    }
}
