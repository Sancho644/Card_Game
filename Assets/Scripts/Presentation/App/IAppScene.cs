namespace Game.Presentation.App
{
    /// <summary>
    /// The composition root of a scene. The application root passes itself in right after the scene loads,
    /// before any Start, so scenes get their services without static state.
    /// </summary>
    public interface IAppScene
    {
        void Enter(AppServices app);
    }
}
