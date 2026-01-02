namespace MauiAppWithMudBlazor.Components.Models
{
    /// <summary>
    /// Статистика для Слайда 2: Динамика Доходов и Расходов по дням.
    /// </summary>
    public class DynamicsStatisticsDto
    {
        /// <summary>
        /// Подписи по оси X (дни месяца, например: ["01", "02", ...])
        /// </summary>
        public string[] DailyChartLabels { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Серия данных для расходов (Сумма расходов за каждый день).
        /// </summary>
        public double[] DailyExpenseData { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Серия данных для доходов (Сумма доходов за каждый день).
        /// </summary>
        public double[] DailyIncomeData { get; set; } = Array.Empty<double>();

        public List<CategoryStat> TopCategories { get; set; } = new();

        /// <summary>
        /// Сумма по самой крупной категории.
        /// </summary>
        public float MaxCategoryAmount => TopCategories.FirstOrDefault()?.Amount ?? 0f;
    }
}