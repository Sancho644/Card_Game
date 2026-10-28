using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Game.Core.Battle;

namespace Game.Simulation
{
    /// <summary>A short human-readable report of one or more simulations. A developer tool, so the text lives here.</summary>
    public sealed class TextReportWriter
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        private readonly Func<string, string> _cardName;

        /// <param name="cardName">Display name of a card id, for the card table.</param>
        public TextReportWriter(Func<string, string> cardName)
        {
            _cardName = cardName ?? (id => id);
        }

        public string WriteMain(SimulationSummary summary)
        {
            var text = new StringBuilder();
            Header(text, summary);

            text.AppendLine("Партии (доли побед — среди партий с победителем, в скобках 95% интервал Уилсона)");
            var first = summary.FirstPlayerWins;
            Line(text, "Победы первого игрока", $"{WithInterval(first)} из {first.Trials}");
            Line(text, "Победы второго игрока", Percent(1 - first.Share));
            Line(text, "Ничьи в партиях", Percent(summary.MatchDraws) + " завершённых партий");
            Line(text, "Средняя длина, ходов", Number(summary.AverageTurns));
            Line(text, "Средняя длина, битв", Number(summary.AverageBattles));
            text.AppendLine();

            text.AppendLine("Битвы");
            Line(text, "Победы хозяина, карта открыта", $"{Percent(summary.HostWins(true))} из {summary.DecidedBattles(true)} битв с победителем");
            Line(text, "Победы гостя, карта открыта", Percent(1 - summary.HostWins(true)));
            Line(text, "Победы хозяина, карта закрыта", $"{Percent(summary.HostWins(false))} из {summary.DecidedBattles(false)} битв с победителем");
            Line(text, "Победы гостя, карта закрыта", Percent(1 - summary.HostWins(false)));
            Line(text, "Победы ходящего в битве первым", Percent(summary.FirstActorWins));
            Line(text, "Решено равенством силы", $"{summary.EqualPowerBattles(TieBreak.EarlierPlacement)} битв (выставлен раньше), " +
                                                   $"{summary.EqualPowerBattles(TieBreak.Ability)} («Перевес») из {summary.Battles.Count}");
            Line(text, "Ничьи в битвах", Percent(summary.BattleDraws));
            Line(text, "Отступления", Percent(summary.Retreats));
            Line(text, "Битвы на открытой карте", Percent(summary.OpenFieldBattles));
            Line(text, "Открытий карт поля за партию", Number(summary.OpeningsPerMatch));
            Line(text, "Пропуски основного действия", Percent(summary.SkipsPerTurn) + " ходов");
            Line(text, "Вызовы", Percent(summary.ChallengesPerTurn) + " ходов");
            text.AppendLine();

            text.AppendLine("Стихии существ (доля побед в битвах)");
            foreach (var (element, battles, wins) in summary.ElementWins())
                Line(text, element.ToString(), $"{Percent(wins)} в {battles} битвах");
            text.AppendLine();

            text.AppendLine("Карты: в колодах (партий) · использована (доля партий) · использований за партию · " +
                            "победы колод с картой среди партий с победителем [95% интервал]");
            text.AppendLine("  * — интервал не включает 50%");
            foreach (var card in summary.Cards.OrderByDescending(card => card.WinShare.Share))
            {
                var wins = card.WinShare;
                text.AppendLine(string.Format(Culture, "  {0,-28} {1,5} · {2,6} · {3,5} · {4} {5}",
                    _cardName(card.CardId), card.DeckGames, Percent(card.UsedShare), Number(card.UsesPerGame),
                    WithInterval(wins), wins.ExcludesHalf ? "*" : ""));
            }

            Problems(text, summary);
            return text.ToString();
        }

        /// <summary>A strength check of two profiles: who wins how often, with seats alternating.</summary>
        public string WriteStrength(SimulationSummary summary)
        {
            var text = new StringBuilder();
            Header(text, summary);
            var first = summary.Options.Player1Profile.Id;
            var second = summary.Options.Player2Profile.Id;
            var firstWins = summary.ProfileWins(first);
            text.AppendLine("  Доли побед — среди партий с победителем, в скобках 95% интервал Уилсона");
            Line(text, $"Победы «{first}»", $"{WithInterval(firstWins)} из {firstWins.Trials}");
            Line(text, $"Победы «{second}»", WithInterval(summary.ProfileWins(second)));
            Line(text, "Ничьи", Percent(summary.MatchDraws) + " завершённых партий");
            Line(text, "Средняя длина, ходов", Number(summary.AverageTurns));
            Problems(text, summary);
            return text.ToString();
        }

        private static void Header(StringBuilder text, SimulationSummary summary)
        {
            var options = summary.Options;
            text.AppendLine($"== {options.Name} ==");
            text.AppendLine($"Партий: {summary.Records.Count}, завершено: {summary.Completed.Count}, " +
                            $"зависло: {summary.Hung.Count()}, ошибок: {summary.Failed.Count()}");
            text.AppendLine($"Seed: {options.Seed}, колоды: {DeckSource(options)}, " +
                            $"ИИ: {options.Player1Profile.Id} / {options.Player2Profile.Id}" +
                            (options.AlternateSeats ? " (места чередуются)" : "") +
                            $", лимит ходов: {options.TurnLimit}, порядок ходов: монета 50/50");
            text.AppendLine();
        }

        private static string DeckSource(SimulationOptions options)
        {
            if (options.Decks.Count > 0)
                return options.Decks.Count + " заданных";
            return options.DeckMode == SimulationDeckMode.Synergy
                ? $"с синергией (существа под бонусы своих карт поля, {Percent(options.SynergyRandomShare)} слотов существ — случайно)"
                : "случайные допустимые";
        }

        private static void Problems(StringBuilder text, SimulationSummary summary)
        {
            foreach (var record in summary.Hung)
                text.AppendLine($"  Зависла партия #{record.Index}, seed {record.Seed}, ходов {record.Turns}");
            foreach (var record in summary.Failed)
                text.AppendLine($"  Ошибка в партии #{record.Index}, seed {record.Seed}: {record.Error}");
            text.AppendLine();
        }

        private static void Line(StringBuilder text, string label, string value) =>
            text.AppendLine(string.Format(Culture, "  {0,-34} {1}", label + ":", value));

        private static string Percent(double share) => share.ToString("P1", Culture);

        private static string WithInterval(ShareEstimate estimate) =>
            string.Format(Culture, "{0,7} [{1:0.0}–{2:0.0}]", Percent(estimate.Share), estimate.Low * 100, estimate.High * 100);

        private static string Number(double value) => value.ToString("0.0", Culture);
    }
}
