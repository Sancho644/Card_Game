using Game.Presentation.Audio;
using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>Supplies the shared UI player to buttons under this canvas.</summary>
    public sealed class UiAudioScope : MonoBehaviour
    {
        public ISoundPlayer Sound { get; set; }
    }
}
