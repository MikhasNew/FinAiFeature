using System.Collections.Generic;
using EfcToXamarinAndroid.Core.Enums;

namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    public class AppConfiguration
    {
        public List<BankConfiguration> Banks { get; set; }
        public DisplayPeriod DefaultPeriod { get; set; } = DisplayPeriod.Auto;
    }
}