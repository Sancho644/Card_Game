namespace Game.Core.Commands
{
    /// <summary>What an ability is aimed at. Each effect reads only the targets it needs.</summary>
    public sealed class AbilityTargets
    {
        public static readonly AbilityTargets None = new AbilityTargets();

        public AbilityTargets(int? creatureId = null, int? fieldId = null, int? cardId = null)
        {
            CreatureId = creatureId;
            FieldId = fieldId;
            CardId = cardId;
        }

        public int? CreatureId { get; }
        public int? FieldId { get; }

        /// <summary>A card in a hand or a pile, e.g. an ability to return from the discard pile.</summary>
        public int? CardId { get; }
    }
}
