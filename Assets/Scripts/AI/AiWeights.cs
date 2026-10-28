namespace Game.AI
{
    /// <summary>
    /// How much the AI values each feature of a position, in "points". Filled from AiConfig;
    /// the defaults are the "normal" profile.
    /// </summary>
    public sealed class AiWeights
    {
        /// <summary>The match is won or lost.</summary>
        public double MatchWin { get; set; } = 1000;

        /// <summary>One battle win.</summary>
        public double WinPoint { get; set; } = 100;

        /// <summary>A creature still in play is worth its power times this.</summary>
        public double CreaturePerPower { get; set; } = 0.1;

        public double CommonAbility { get; set; } = 6;
        public double EpicAbility { get; set; } = 8;
        public double LegendaryAbility { get; set; } = 12;

        /// <summary>
        /// An ability that brings back a spent ability is worth this share of the best one it could bring back
        /// (at least a common one): it has no other use, and the return still needs a battle on your turn.
        /// </summary>
        public double ReturnedCardShare { get; set; } = 0.5;

        /// <summary>A field card not removed from the table.</summary>
        public double FieldCard { get; set; } = 4;

        /// <summary>One of your field cards is locked for its next battle.</summary>
        public double LockedField { get; set; } = 3;

        /// <summary>A creature waiting on a field gets this per point of the field's value for its element.</summary>
        public double FieldAffinityPerPower { get; set; } = 0.1;

        /// <summary>A choice after the battle, e.g. taking an ability back.</summary>
        public double ChoiceAfterBattle { get; set; } = 6;

        /// <summary>A power difference of this size gives about 73% to win the battle.</summary>
        public double MarginScale { get; set; } = 40;

        /// <summary>Margin added for the side that wins on equal power; half a power step makes equality decisive.</summary>
        public double EqualPowerEdge { get; set; } = 5;

        /// <summary>Share of your in-hand ability power expected to be used in the battle.</summary>
        public double OwnReserveShare { get; set; } = 0.5;

        /// <summary>Share of the opponent's in-hand ability power expected to be used against you.</summary>
        public double OpponentReserveShare { get; set; } = 0.5;

        /// <summary>Learning one unknown opponent card.</summary>
        public double InformationPerCard { get; set; } = 1.5;
    }
}
