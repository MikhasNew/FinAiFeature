using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiAppWithMudBlazor.Components.Models
{
    public class TabStatisticsDto
    {
        // 1. Основные метрики
        public double AverageCheck { get; set; }
        public double MaxSum { get; set; }
        public double MinSum { get; set; }

        // 2. График по дням (последние 7-30 дней или весь период)
        public double[] DailyChartData { get; set; } = Array.Empty<double>();
        public string[] DailyChartLabels { get; set; } = Array.Empty<string>();

        // 3. Топ категорий и проценты
        public List<CategoryStat> TopCategories { get; set; } = new();

        // 4. Аномалии (Топ 3 самых крупных операции)
        public List<FinanceItem> Anomalies { get; set; } = new();

        // Новые ключевые метрики для Слайда 1
        /// <summary>Чистый баланс за период (Доход - Расход)</summary>
        public float NetFlow { get; set; }

        /// <summary>Средний расход в день за прошедшие дни месяца</summary>
        public float AverageDailyExpense { get; set; }

        /// <summary>Расчетный ежедневный лимит трат (для выхода в ноль или достижения цели)</summary>
        public float CalculatedDailyLimit { get; set; }


        // Метрики для Слайда 2: Сравнение с прошлым месяцем
        public float LastMonthTotalExpense { get; set; }
        public float LastMonthTotalIncome { get; set; }

        /// <summary>Процент изменения расходов (Тек. месяц vs Прошлый месяц)</summary>
        public float ExpenseChangePercent
        {
            get
            {
                if (LastMonthTotalExpense == 0) return 0; // Избегаем деления на ноль
                return (TotalExpense - LastMonthTotalExpense) / LastMonthTotalExpense;
            }
        }

        /// <summary>Процент изменения доходов (Тек. месяц vs Прошлый месяц)</summary>
        public float IncomeChangePercent
        {
            get
            {
                if (LastMonthTotalIncome == 0) return 0;
                return (TotalIncome - LastMonthTotalIncome) / LastMonthTotalIncome;
            }
        }

        public float TotalIncome { get; set; }
        public float TotalExpense { get; set; }
        public float AverageExpenseCheck { get; set; }
    }
}
