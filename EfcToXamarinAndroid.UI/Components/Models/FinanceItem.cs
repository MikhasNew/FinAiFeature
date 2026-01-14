using System;
using EfcToXamarinAndroid.Core; // ����������� ������� enum OperacionTyps �����

namespace EfcToXamarinAndroid.UI.Components.Models
{
    public class FinanceItem
    {
        // ==========================================
        // �������� ������ (Raw Data)
        // ==========================================

        // ���������� backing fields ��� �������, ��������� ������� ������ �� UI
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
                    // ���������� ��� ��������� �������
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
                    _formattedSum = null; // ����� ����
                    // ���� ��������������� ������, �� ����� ���� ���������� ������
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
                    _displayIcon = null; // ����� ������
                    _formattedSum = null; // ����� ������� �� ���� (���� +/-)
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

        // ��������� ������� ��������
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

        // ����������� �������� ��� �������� (����� ������, ���� �� �������� ��� XAML �����)
        public FinanceItem ThisFinanceItem => this;

        // ==========================================
        // ���������������� UI �������� (Lazy Loading)
        // ==========================================

        // 1. ��������������� �����
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

        // 2. ���� �����
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
            // ������ �������� � ���� �����. 
            // ��������� Enum �������� (������), ��� ToString()
            bool isExpense = OperationType == OperacionTyps.OPLATA ||
                             OperationType == OperacionTyps.NALICHNYE;

            if (isExpense)
            {
                float displaySum = _sum > 0 ? -_sum : _sum;
                _amountColor = "red";
                _formattedSum = displaySum.ToString("F2"); // ������� ������ 1 ���
            }
            else
            {
                float displaySum = _sum < 0 ? -_sum : _sum;
                _amountColor = "green";
                _formattedSum = $"+{displaySum:F2}";
            }
        }

        // 3. ���� � �����
        private string? _formattedDate;
        public string FormattedDate => _formattedDate ??= _date.ToString("dd.MM.yyyy");

        private string? _formattedTime;
        public string FormattedTime => _formattedTime ??= _date.ToString("HH:mm:ss");

        // 4. ��������� (���������� ������)
        private string? _displayTitle;
        public string DisplayTitle => _displayTitle ??= string.IsNullOrEmpty(_description) ? "��������" : _description!;

        private string? _displayDescription;
        public string DisplayDescription => _displayDescription ??= string.IsNullOrEmpty(_mccDescription) ? "��� ��������" : _mccDescription!;

        // 5. ������
        private string? _displayIcon;
        public string DisplayIcon
        {
            get
            {
                if (_displayIcon == null)
                {
                    // SWITCH �� Enum, � �� �� ������! ��� � 100 ��� �������.
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

        // 6. ����������� ��� ������ � ����������
        public static int CurrentYear = DateTime.Now.Year;
        private string? _shortFormattedLabel;
      
        public string ShortFormattedLabel
        {
            get
            {
                // �����������: ���� ������ ��� ������������, ���������� � ����� (������� �� ������)
                if (_shortFormattedLabel != null)
                {
                    return _shortFormattedLabel;
                }

                // ������: ��������� ������ ������ ���� ���, ��������� ����� ����������� ��������
                if (_date.Year == CurrentYear)
                {
                    _shortFormattedLabel = $"{_date:HH:mm} � {_date:dd.MM}";
                }
                else
                {
                    _shortFormattedLabel = $"{_date:HH:mm} � {_date:dd.MM.yyyy}";
                }

                return _shortFormattedLabel;
            }
        }
    }
}
