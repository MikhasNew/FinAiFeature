// EfcToXamarinAndroid.Core/Services/DataService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Parsers;

namespace EfcToXamarinAndroid.Core.Services
{
    public class DataService : IDataService
    {
        public async Task<List<DataItem>> ParseSmsToDataItemsAsync(List<Sms> smsList, List<BankConfiguration> bankConfigurations)
        {
            var parser = new Parser(smsList, bankConfigurations);
            return await parser.GetData();
        }

        public async Task<List<DataItem>> ParsePdfToDataItemsAsync(string filePath, List<BankConfiguration> bankConfigurations)
        {
            var parser = new Parser(filePath, bankConfigurations);
            return await parser.GetDataFromPdf();
        }

        public async Task<List<DataItem>> ParseXmlToDataItemsAsync(string filePath)
        {
            var serializer = new SerializarionToXml();
            return await Task.FromResult(serializer.DeserializeFile(filePath).ToList());
        }

        public async Task<bool> SaveDataItemsToXmlAsync(string filePath)
        {
            var serializer = new SerializarionToXml();
            return await Task.FromResult(serializer.SaveToFile(filePath) != null);
        }
    }
}