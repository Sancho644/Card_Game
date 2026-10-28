using System;
using Game.AI;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>The AI's valuation of a position, editable in the inspector. See <see cref="AiWeights"/>.</summary>
    [Serializable]
    public sealed class AiWeightsData
    {
        [SerializeField] private float _matchWin = 1000;
        [SerializeField] private float _winPoint = 100;
        [SerializeField, Tooltip("Creature value = power × this")] private float _creaturePerPower = 0.1f;
        [SerializeField] private float _commonAbility = 6;
        [SerializeField] private float _epicAbility = 8;
        [SerializeField] private float _legendaryAbility = 12;
        [SerializeField] private float _fieldCard = 4;
        [SerializeField] private float _lockedField = 3;
        [SerializeField] private float _fieldAffinityPerPower = 0.1f;
        [SerializeField] private float _choiceAfterBattle = 6;
        [SerializeField, Min(1), Tooltip("Power margin giving about 73% to win a battle")] private float _marginScale = 40;
        [SerializeField, Min(0), Tooltip("Margin for the side that wins on equal power")] private float _equalPowerEdge = 5;
        [SerializeField, Range(0, 1)] private float _ownReserveShare = 0.5f;
        [SerializeField, Range(0, 1)] private float _opponentReserveShare = 0.5f;
        [SerializeField] private float _informationPerCard = 1.5f;

        public AiWeights ToWeights() => new AiWeights
        {
            MatchWin = _matchWin,
            WinPoint = _winPoint,
            CreaturePerPower = _creaturePerPower,
            CommonAbility = _commonAbility,
            EpicAbility = _epicAbility,
            LegendaryAbility = _legendaryAbility,
            FieldCard = _fieldCard,
            LockedField = _lockedField,
            FieldAffinityPerPower = _fieldAffinityPerPower,
            ChoiceAfterBattle = _choiceAfterBattle,
            MarginScale = _marginScale,
            EqualPowerEdge = _equalPowerEdge,
            OwnReserveShare = _ownReserveShare,
            OpponentReserveShare = _opponentReserveShare,
            InformationPerCard = _informationPerCard,
        };
    }
}
