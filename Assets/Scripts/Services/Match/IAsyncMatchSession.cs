using System;
using Game.Core.Commands;
namespace Game.Services.Match
{
    public interface IAsyncMatchSession : IMatchSession
    {
        void SubmitAsync(GameCommand command,Action<CommandResult> completed);
    }
}
