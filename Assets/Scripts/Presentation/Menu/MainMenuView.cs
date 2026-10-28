using System;
using Game.Presentation.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Menu
{
    /// <summary>The main menu: play against the AI, play two at one screen, choose the deck and the difficulty.</summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _versusAi;
        [SerializeField] private Button _hotSeat;
        [SerializeField] private Button _deck;
        [SerializeField] private Button _difficulty;

        public void Show(
            string title,
            string versusAiLabel,
            string hotSeatLabel,
            string deckLabel,
            string difficultyLabel,
            Action versusAi,
            Action hotSeat,
            Action chooseDeck,
            Action chooseDifficulty)
        {
            _title.text = title;
            UiButtons.SetLabel(_versusAi, versusAiLabel);
            UiButtons.Bind(_versusAi, versusAi);
            UiButtons.SetLabel(_hotSeat, hotSeatLabel);
            UiButtons.Bind(_hotSeat, hotSeat);
            UiButtons.SetLabel(_deck, deckLabel);
            UiButtons.Bind(_deck, chooseDeck);
            UiButtons.SetLabel(_difficulty, difficultyLabel);
            UiButtons.Bind(_difficulty, chooseDifficulty);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
