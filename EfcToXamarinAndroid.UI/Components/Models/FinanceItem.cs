using System;
using EfcToXamarinAndroid.Core; // для доступа к enum OperacionTyps здесь

namespace EfcToXamarinAndroid.UI.Components.Models
{
    public class FinanceItem
    {
        // ==========================================
        // Данные модели (Raw Data)
        // ==========================================

        // Внутренние backing fields для свойств, требующих сброса кешей UI
        private DateTime _date;
        private float _sum;
        private OperacionTyps _operationType;
        private string? _description;
        private string? _mccDescription;


        public int Id { get; set; }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    // Сбрасываем все зависимые строки
                    _formattedDate = null;
                    _formattedTime = null;
                    _shortFormattedLabel = null;
                }
            }
        }

        public float Sum
        {
            get => _sum;
            set
            {
                if (Math.Abs(_sum - value) > 0.001f)
                {
                    _sum = value;
                    _formattedSum = null; // сброс кеша
                    // Цвет пересчитывать стоило, но здесь цвет вычисляется вместе
                }
            }
        }

        public OperacionTyps OperationType
        {
            get => _operationType;
            set
            {
                if (_operationType != value)
                {
                    _operationType = value;
                    _displayIcon = null; // сброс иконки
                    _formattedSum = null; // сброс формата (знак +/-)
                }
            }
        }

        public string? Description
        {
            get => _description;
            set
            {
                _description = value;
                _displayTitle = null;
            }
        }

        public string? MccDescription
        {
            get => _mccDescription;
            set
            {
                _mccDescription = value;
                _displayDescription = null;
            }
        }

        // Остальные простые свойства
        public string? Title { get; set; }
        public string? Icon { get; set; }
        public int MCC { get; set; }
        public string? UnreachableText { get; set; }
        public bool IsNewDataItem { get; set; }
        public float Balance { get; set; }

        /// <summary>
        /// QR-код чека, ожидающий повторной проверки
        /// </summary>
        public string? PendingQrCode { get; set; }

        // Утилитарное свойство для привязки (костыль старый, если он остался для XAML биндов)
        public FinanceItem ThisFinanceItem => this;

        // ==========================================
        // Оптимизированные UI свойства (Lazy Loading)
        // ==========================================

        // 1. Форматированная сумма
        private string? _formattedSum;
        public string FormattedSum
        {
            get
            {
                if (_formattedSum == null)
                {
                    CalculateSumAndColor();
                }
                return _formattedSum!;
            }
        }

        // 2. Цвет суммы
        private string? _amountColor;
        public string AmountColor
        {
            get
            {
                if (_amountColor == null)
                {
                    CalculateSumAndColor();
                }
                return _amountColor!;
            }
        }

        private void CalculateSumAndColor()
        {
            // Логика перенесена с вью модели. 
            // Используем Enum напрямую (быстрее), чем ToString()
            bool isExpense = OperationType == OperacionTyps.OPLATA ||
                             OperationType == OperacionTyps.NALICHNYE;

            if (isExpense)
            {
                float displaySum = _sum > 0 ? -_sum : _sum;
                _amountColor = "red";
                _formattedSum = displaySum.ToString("F2"); // Оставляем только 2 знака
            }
            else
            {
                float displaySum = _sum < 0 ? -_sum : _sum;
                _amountColor = "green";
                _formattedSum = $"+{displaySum:F2}";
            }
        }

        // 3. Дата и время
        private string? _formattedDate;
        public string FormattedDate => _formattedDate ??= _date.ToString("dd.MM.yyyy");

        private string? _formattedTime;
        public string FormattedTime => _formattedTime ??= _date.ToString("HH:mm:ss");

        // 4. Заголовки (безопасные строки)
        private string? _displayTitle;
        public string DisplayTitle => _displayTitle ??= string.IsNullOrEmpty(_description) ? "Без описания" : _description!;

        private string? _displayDescription;
        public string DisplayDescription => _displayDescription ??= string.IsNullOrEmpty(_mccDescription) ? "Без описания категории" : _mccDescription!;

        // 5. Иконка
        private string? _displayIcon;
        public string DisplayIcon
        {
            get
            {
                if (_displayIcon == null)
                {
                    // SWITCH по Enum, а не по строке! Это в 100 раз быстрее.
                    _displayIcon = OperationType switch
                    {
                        OperacionTyps.OPLATA => "@Icons.Material.Filled.Payment",
                        OperacionTyps.ZACHISLENIE => "@Icons.Material.Filled.AccountBalance",
                        OperacionTyps.NALICHNYE => "@Icons.Material.Filled.AttachMoney",
                        OperacionTyps.UNREACHABLE => "@Icons.Material.Filled.QuestionMark",
                        _ => "@Icons.Material.Filled.Payment"
                    };
                }
                return _displayIcon;
            }
        }

        // 6. Специальный лейбл для списка в приложении
        public static int CurrentYear = DateTime.Now.Year;
        private string? _shortFormattedLabel;
      
        public string ShortFormattedLabel
        {
            get
            {
                // Оптимизация: если строка уже сформирована, возвращаем её сразу (аллокаций не будет)
                if (_shortFormattedLabel != null)
                {
                    return _shortFormattedLabel;
                }

                // Логика: без года если текущий год, иначе полная дата
                if (_date.Year == CurrentYear)
                {
                    _shortFormattedLabel = $"{_date:HH:mm} • {_date:dd.MM}";
                }
                else
                {
                    _shortFormattedLabel = $"{_date:HH:mm} • {_date:dd.MM.yyyy}";
                }

                return _shortFormattedLabel;
            }
        }
    }
}
