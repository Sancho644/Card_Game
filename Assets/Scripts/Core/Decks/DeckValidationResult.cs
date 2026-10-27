using System.Collections.Generic;

namespace Game.Core.Decks
{
    public sealed class DeckValidationResult
    {
        internal DeckValidationResult(IReadOnlyList<DeckError> errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<DeckError> Errors { get; }
        public bool IsValid => Errors.Count == 0;
    }
}
