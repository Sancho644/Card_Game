using System;

namespace Game.Services.Settings
{
    /// <summary>Player settings kept in a local store on this device.</summary>
    public sealed class LocalPlayerSettingsService : IPlayerSettingsService
    {
        private const string FastAnimationsKey = "settings.fast-animations";

        private readonly ISettingsStore _store;

        public LocalPlayerSettingsService(ISettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public event Action Changed;

        public bool FastAnimations
        {
            get => _store.GetInt(FastAnimationsKey, 0) != 0;
            set
            {
                if (value == FastAnimations)
                    return;
                _store.SetInt(FastAnimationsKey, value ? 1 : 0);
                _store.Save();
                Changed?.Invoke();
            }
        }
    }
}
