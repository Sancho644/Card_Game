using System;

namespace Game.Services.Settings
{
    /// <summary>
    /// The player's own settings kept on the device: animation speed now, later sound volume and the like.
    /// Changes are saved at once.
    /// </summary>
    public interface IPlayerSettingsService
    {
        /// <summary>Animations play faster than normal.</summary>
        bool FastAnimations { get; set; }

        /// <summary>Raised after any setting changed.</summary>
        event Action Changed;
    }
}
