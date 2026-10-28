namespace Game.Core.Effects
{
    /// <summary>«Нерушимая»: after the battle the field is not removed and stays open.</summary>
    public sealed class FieldStaysCondition : FieldCondition
    {
        public override bool KeepsField => true;
    }
}
