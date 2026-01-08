using System.ComponentModel;

namespace EfcToXamarinAndroid.Core.Enums
{
    public enum DisplayPeriod
    {
        [Description("Авто")]
        Auto = 0,
        [Description("Неделя")]
        Week = 1,
        [Description("Месяц")]
        Month = 2,
        [Description("Полгода")]
        HalfYear = 3,
        [Description("Год")]
        Year = 4,
        [Description("Все время")]
        All = 5
    }
}
