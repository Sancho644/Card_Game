namespace Game.Presentation.App
{
    /// <summary>Switches between the application's scenes.</summary>
    public interface ISceneNavigator
    {
        void ToMainMenu();

        void ToMatch();
    }
}
