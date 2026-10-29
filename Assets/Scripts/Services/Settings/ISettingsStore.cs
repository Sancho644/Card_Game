namespace Game.Services.Settings
{
    /// <summary>Where settings are kept between launches: device storage in the game, memory in tests.</summary>
    public interface ISettingsStore
    {
        int GetInt(string key, int fallback);
        void SetInt(string key, int value);
        void Save();
    }
}
