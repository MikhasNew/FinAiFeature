using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiAppWithMudBlazor.Components.Models
{
    /// <summary>
    /// Статистика для Слайда 1: Чистый денежный поток (Баланс месяца)
    /// </summary>
    public class FlowStatisticsDto
    {
        public float TotalIncome { get; set; }
        public float TotalExpense { get; set; }

        /// <summary>
        /// Чистый баланс за период: Доход - Расход
        /// </summary>
        public float NetFlow => TotalIncome - TotalExpense;

        /// <summary>
        /// Текстовое представление периода (например: "01 дек - 31 дек")
        /// </summary>
        public string CurrentPeriodDisplay { get; set; } = string.Empty;

        /// <summary>
        /// Сколько можно тратить в день до конца месяца (для текущего незавершенного месяца).
        /// </summary>
        public float CalculatedDailyLimit { get; set; }

        /// <summary>
        /// Является ли текущий период прошедшим (для отключения логики DailyLimit).
        /// </summary>
        public bool IsPeriodInPast { get; set; }
    }
}

