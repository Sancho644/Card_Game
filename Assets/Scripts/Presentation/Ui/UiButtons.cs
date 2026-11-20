using System;
using Game.Data.Presentation;
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
            button.gameObject.SetActive(action != null);
            Assign(button, action);
        }

        /// <summary>Binds a button without changing its visibility, including a newly generated button.</summary>
        public static void Assign(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            if (action == null)
                return;

            button.onClick.AddListener(() =>
            {
                if (!button.IsInteractable())
                    return;

                var scope = button.GetComponentInParent<UiAudioScope>();
                scope?.Sound?.Play(SoundCue.Button);
                action();
            });
        }

        public static void SetLabel(Button button, string text)
        {
            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = text;
        }
    }
}
