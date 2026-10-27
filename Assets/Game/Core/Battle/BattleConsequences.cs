using System;
using System.Collections.Generic;
using Game.Core.State;

namespace Game.Core.Battle
{
    public enum CreatureFate
    {
        StaysOnField,
        ReturnsToHand,
        OutOfGame,
    }

    /// <summary>What happens after a battle. Built from the rules table, then adjusted by effects.</summary>
    public sealed class BattleConsequences
    {
        private readonly CreatureFate[] _fates = new CreatureFate[PlayerIds.All.Count];
        private readonly List<ChoiceRequest> _choices = new List<ChoiceRequest>();

        internal BattleConsequences(PlayerId host, PlayerId? winner, bool fieldWasOpen)
        {
            Host = host;
            Winner = winner;
            FieldWasOpen = fieldWasOpen;
        }

        public PlayerId Host { get; }
        public PlayerId Guest => Host.Opponent();
        public PlayerId? Winner { get; }
        public PlayerId? Loser => Winner?.Opponent();
        public bool IsDraw => Winner == null;
        public bool FieldWasOpen { get; }
        public bool FieldRemoved { get; set; }
        public PlayerId? WinScoredBy { get; set; }

        /// <summary>Choices players make once the battle is over, e.g. a card to take back.</summary>
        public IReadOnlyList<ChoiceRequest> Choices => _choices;

        public CreatureFate FateOf(PlayerId player) => _fates[(int)player];

        public void SetFate(PlayerId player, CreatureFate fate) => _fates[(int)player] = fate;

        public void RequestChoice(ChoiceRequest request) =>
            _choices.Add(request ?? throw new ArgumentNullException(nameof(request)));
    }

    /// <summary>A choice for a player after the battle; options are the player's cards matching the filter.</summary>
    public sealed class ChoiceRequest
    {
        public ChoiceRequest(PlayerId player, ChoicePurpose purpose, Func<CardInstance, bool> filter)
        {
            Player = player;
            Purpose = purpose;
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public PlayerId Player { get; }
        public ChoicePurpose Purpose { get; }
        public Func<CardInstance, bool> Filter { get; }
    }
}
