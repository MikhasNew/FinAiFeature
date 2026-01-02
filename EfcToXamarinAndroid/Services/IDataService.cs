// EfcToXamarinAndroid.Core/Services/IDataService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;

namespace EfcToXamarinAndroid.Core.Services
{
    public interface IDataService
    {
        Task<List<DataItem>> ParseSmsToDataItemsAsync(List<Sms> smsList, List<BankConfiguration> bankConfigurations);
        Task<List<DataItem>> ParsePdfToDataItemsAsync(string filePath, List<BankConfiguration> bankConfigurations);
        Task<List<DataItem>> ParseXmlToDataItemsAsync(string filePath);
        Task<bool> SaveDataItemsToXmlAsync(string filePath);
    }
}