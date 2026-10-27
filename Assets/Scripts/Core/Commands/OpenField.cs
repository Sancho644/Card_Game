using Game.Core.State;

namespace Game.Core.Commands
{
    /// <summary>The host turns the battle's field card face up.</summary>
    public sealed class OpenField : GameCommand
    {
        public OpenField(PlayerId player)
            : base(player)
        {
        }
    }
}
