namespace EfcToXamarinAndroid.UI.Components.Models
{
    public class PredictionStatisticsDto
    {
        public double CurrentBalance { get; set; }
        public DateTime? PredictedIncomeDate { get; set; }
        public int DaysUntilIncome { get; set; }
        public double RecommendedDailyBudget { get; set; }
        public double ReservedAmount { get; set; } // Optional: user could define fixed costs
    }
}
