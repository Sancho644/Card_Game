using UnityEngine;
namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Main Menu", fileName = "MainMenu")]
    public sealed class MainMenuSettings : ScriptableObject
    {
        [SerializeField] private Sprite _background;
        public Sprite Background => _background;
    }
}
