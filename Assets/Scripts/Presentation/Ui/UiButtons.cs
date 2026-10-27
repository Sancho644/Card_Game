using System;
using TMPro;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>Small helpers shared by the interface views.</summary>
    public static class UiButtons
    {
        /// <summary>Shows the button with this action, or hides it when the action is null.</summary>
        public static void Bind(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(action != null);
            if (action != null)
                button.onClick.AddListener(() => action());
        }

        public static void SetLabel(Button button, string text)
        {
            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = text;
        }
    }
}
