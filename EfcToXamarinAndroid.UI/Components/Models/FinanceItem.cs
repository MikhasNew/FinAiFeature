using EfcToXamarinAndroid.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.UI.Components.Models
{
    public class FinanceItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public float Sum { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public string? Icon { get; set; }
        public string? MccDescription { get; set; }
        public int MCC { get; set; }
        public string? UnreachableText { get; set; }
        public OperacionTyps OperationType { get; set; }
        public bool IsNewDataItem { get; set; }
        public float Balance { get; set; }
        public FinanceItem ThisFinanceItem
        {
            get
            {
                return this;
            }
        }

        // ‘орматированные свойства дл€ отображени€
        public string FormattedSum => GetFormattedSum();
        public string FormattedDate => Date.ToString("dd.MM.yyyy");
        public string FormattedTime => Date.ToString("HH:mm:ss");
        public string DisplayTitle => !string.IsNullOrEmpty(Description) ? Description : Description ?? "ќпераци€";
        public string DisplayDescription => !string.IsNullOrEmpty(MccDescription) ? MccDescription : MccDescription ?? "Ѕез описани€";
        public string DisplayIcon => GetIconForOperationType();

        public string AmountColor { get; set; }

        private string GetFormattedSum()
        {
            // ќпредел€ем, €вл€етс€ ли операци€ расходной (оплатой)
            bool isExpense = OperationType == OperacionTyps.OPLATA || OperationType == OperacionTyps.NALICHNYE;

            // ƒл€ расходных операций показываем отрицательное число (даже если в базе оно положительное)
            if (isExpense)
            {
                // ≈сли сумма уже отрицательна€, оставл€ем как есть, иначе делаем отрицательной
                float displaySum = Sum > 0 ? -Sum : Sum;
                AmountColor = "red";
                return $"{displaySum:F2}";
            }
            else
            {
                // ƒл€ приходных операций (зачисление и другие) показываем с плюсом
                float displaySum = Sum < 0 ? -Sum : Sum;
                AmountColor = "green";
                return $"+{displaySum:F2}";
            }
        }
        private string GetIconForOperationType()
        {
            return OperationType.ToString() switch
            {
                "OPLATA" => "@Icons.Material.Filled.Payment",
                "ZACHISLENIE" => "@Icons.Material.Filled.AccountBalance",
                "NALICHNYE" => "@Icons.Material.Filled.AttachMoney",
                "UNREACHABLE" => "@Icons.Material.Filled.QuestionMark",
                _ => "@Icons.Material.Filled.Payment"
            };
        }
    }
}

