using System;
using System.Collections.Generic;
using Game.Core.Events;
using Game.Data.Presentation;
using Game.Presentation.Cards;

namespace Game.Presentation.Match
{
    /// <summary>Turns engine events into event log lines. Events without a line return null.</summary>
    public sealed class EventLogFormatter
    {
        private readonly UiTexts _texts;
        private readonly PlayerNames _names;
        private readonly CardNameLookup _lookup;
        private readonly CardFaceBuilder _faces;
        private readonly Dictionary<Type, Func<GameEvent, string>> _formats = new Dictionary<Type, Func<GameEvent, string>>();

        public EventLogFormatter(UiTexts texts, PlayerNames names, CardNameLookup lookup, CardFaceBuilder faces)
        {
            _texts = texts;
            _names = names;
            _lookup = lookup;
            _faces = faces;

            Add<CoinTossed>(e => Line(UiTextKeys.LogCoin, _names.Of(e.Winner)));
            Add<TurnOrderChosen>(e => Line(UiTextKeys.LogOrder, _names.Of(e.FirstPlayer)));
            Add<TurnStarted>(e => Line(UiTextKeys.LogTurn, _names.Of(e.Player)));
            Add<CreaturePlaced>(e => Line(UiTextKeys.LogPlaced, _names.Of(e.Player), _faces.NameOf(e.Creature.CardId)));
            Add<CreatureMoved>(e => Line(UiTextKeys.LogMoved, _names.Of(e.Player), _lookup.NameOf(e.CreatureId)));
            Add<MainActionSkipped>(e => Line(UiTextKeys.LogSkipped, _names.Of(e.Player), Line(UiTextKeys.SkipReasonPrefix + e.Reason)));
            Add<BattleStarted>(e => Line(UiTextKeys.LogBattle, _lookup.NameOf(e.FieldId), _names.Of(e.FirstActor)));
            Add<FieldOpened>(e => Line(UiTextKeys.LogFieldOpened, _faces.NameOf(e.CardId)));
            Add<AbilityPlayed>(e => Line(UiTextKeys.LogAbility, _names.Of(e.Player), _faces.NameOf(e.Ability.CardId)));
            Add<PlayerReady>(e => Line(UiTextKeys.LogReady, _names.Of(e.Player)));
            Add<ReadinessReset>(e => Line(UiTextKeys.LogReadyReset, _names.Of(e.Player)));
            Add<BattleResolved>(e => Line(UiTextKeys.LogResolved, e.HostPower, e.GuestPower));
            Add<CreatureOut>(e => Line(UiTextKeys.LogOut, _lookup.NameOf(e.CreatureId)));
            Add<CreatureReturnedToHand>(e => Line(UiTextKeys.LogToHand, _lookup.NameOf(e.CreatureId)));
            Add<FieldRemoved>(e => Line(UiTextKeys.LogFieldRemoved, _lookup.NameOf(e.FieldId)));
            Add<WinScored>(e => Line(UiTextKeys.LogWin, _names.Of(e.Player), e.TotalWins));
            Add<BattleRetreated>(e => Line(UiTextKeys.LogRetreat, _names.Of(e.Player)));
            Add<AbilityCancelled>(e => Line(UiTextKeys.LogCancelled, _faces.NameOf(e.Ability.CardId)));
            Add<AbilityDiscarded>(e => Line(UiTextKeys.LogDiscarded, _names.Of(e.Owner), _faces.NameOf(e.Ability.CardId)));
            Add<AbilityReturnedToHand>(e => Line(UiTextKeys.LogAbilityBack, _names.Of(e.Owner), _faces.NameOf(e.Ability.CardId)));
            Add<FieldLocked>(e => Line(UiTextKeys.LogLocked));
            Add<ChoiceRequested>(e => Line(UiTextKeys.LogChoice, _names.Of(e.Player)));
            Add<MatchEnded>(e => Line(UiTextKeys.LogMatchEnd));
        }

        public string Format(GameEvent gameEvent) =>
            _formats.TryGetValue(gameEvent.GetType(), out var format) ? format(gameEvent) : null;

        private void Add<TEvent>(Func<TEvent, string> format) where TEvent : GameEvent =>
            _formats[typeof(TEvent)] = gameEvent => format((TEvent)gameEvent);

        private string Line(string key, params object[] args) => _texts.Format(key, args);
    }
}
