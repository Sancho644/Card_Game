using UnityEngine.SceneManagement;

namespace Game.Presentation.App
{
    public sealed class SceneNavigator : ISceneNavigator
    {
        private readonly string _mainMenu;
        private readonly string _match;

        public SceneNavigator(string mainMenu, string match)
        {
            _mainMenu = mainMenu;
            _match = match;
        }

        public void ToMainMenu() => SceneManager.LoadScene(_mainMenu);

        public void ToMatch() => SceneManager.LoadScene(_match);
    }
}
