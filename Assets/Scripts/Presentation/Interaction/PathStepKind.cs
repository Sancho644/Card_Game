namespace Game.Presentation.Interaction
{
    public enum PathStepKind
    {
        /// <summary>The player taps a card on the table (a creature or a field).</summary>
        Card,

        /// <summary>The empty field position on the viewer's side.</summary>
        OwnFieldZone,

        /// <summary>The player picks from a list: a card in a pile, a signature ability or none.</summary>
        Option,
    }
}
