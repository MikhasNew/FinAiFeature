using EfcToXamarinAndroid.Core.Services;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;

using EfcToXamarinAndroid.Core.Repository;
using EfcToXamarinAndroid.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;


namespace WebDebugger.Services
{
    public class WebSmsReader : ISmsReader
    {
        public event EventHandler<Sms>? SmsReceived;
        public Task<List<Sms>> GetAllSmsAsync(List<BankConfiguration> addressFilter) => Task.FromResult(new List<Sms>());
        public void StartListening(List<BankConfiguration> addressFilter) { }
        public void StopListening() { }
    }

    public class WebFileService : IFileService
    {
        public Task<string> PickFolderAsync() => Task.FromResult("");
        public Task<string> PickFileAsync(string title, string[] fileTypes) => Task.FromResult("");
        public Task<bool> SaveFileAsync(string content, string fileName, string filePath) => Task.FromResult(true);
        public Task<string> ReadFileAsync(string filePath) => Task.FromResult("");
        public bool FileExists(string filePath) => false;
        public string GetDownloadsPath() => "";
    }

    public class WebUIService : IUIService
    {
        public Task ShowToastAsync(string message) { Console.WriteLine($"Toast: {message}"); return Task.CompletedTask; }
        public Task<bool> ShowConfirmationDialogAsync(string title, string message) => Task.FromResult(true);
        public Task<string> ShowInputDialogAsync(string title, string message, string defaultValue = "") => Task.FromResult(defaultValue);
    }

    public class WebPermissionService : IPermissionService
    {
        public Task<PermissionStatus> CheckSmsPermissionAsync() => Task.FromResult(PermissionStatus.Granted);
        public Task<PermissionStatus> RequestSmsPermissionAsync() => Task.FromResult(PermissionStatus.Granted);
        public Task<PermissionStatus> CheckStorageReadPermissionAsync() => Task.FromResult(PermissionStatus.Granted);
        public Task<PermissionStatus> RequestStorageReadPermissionAsync() => Task.FromResult(PermissionStatus.Granted);
        public Task<PermissionStatus> CheckStorageWritePermissionAsync() => Task.FromResult(PermissionStatus.Granted);
        public Task<PermissionStatus> RequestStorageWritePermissionAsync() => Task.FromResult(PermissionStatus.Granted);
    }

    public static class MockDataHelper
    {
        public static void SeedMockData()
        {
            if (DatesRepositorio.DataItems.Any()) return;

            var random = new Random();
            var now = DateTime.Now;
            var items = new List<DataItem>();

            // Р”РѕР±Р°РІР»СЏРµРј РґРѕС…РѕРґС‹
            for (int i = 0; i < 5; i++)
            {
                items.Add(new DataItem(OperacionTyps.ZACHISLENIE, now.AddDays(-i * 2))
                {
                    Id = i + 1,
                    Sum = 50000 + random.Next(1000, 10000),
                    Title = "Salary",
                    Descripton = "Monthly Salary Deposit",
                    Balance = 150000 + i * 5000
                });
            }

            // Р”РѕР±Р°РІР»СЏРµРј СЂР°СЃС…РѕРґС‹ (РћРїР»Р°С‚Р°)
            string[] categories = { "Supermarket", "Fuel", "Restaurants", "Electronics", "Pharmacy" };
            for (int i = 0; i < 20; i++)
            {
                var cat = categories[random.Next(categories.Length)];
                items.Add(new DataItem(OperacionTyps.OPLATA, now.AddDays(-random.Next(0, 30)).AddHours(-random.Next(0, 24)))
                {
                    Id = i + 10,
                    Sum = random.Next(100, 5000),
                    Title = cat,
                    Descripton = $"Purchase at {cat}",
                    MccDeskription = cat,
                    MCC = 5000 + random.Next(1, 100),
                    Balance = 100000 - i * 1000
                });
            }

            // Р”РѕР±Р°РІР»СЏРµРј РЅР°Р»РёС‡РЅС‹Рµ
            for (int i = 0; i < 3; i++)
            {
                items.Add(new DataItem(OperacionTyps.NALICHNYE, now.AddDays(-random.Next(1, 10)))
                {
                    Id = i + 40,
                    Sum = 5000,
                    Title = "ATM",
                    Descripton = "Cash withdrawal",
                    Balance = 80000 - i * 5000
                });
            }

            DatesRepositorio.DataItems.Clear();
            DatesRepositorio.DataItems.AddRange(items);
            
            // В Core проекте статистика обычно считается при LoadFinanceItemsAsync в VM.
            // Мы убедимся, что VM будет обновлен.
        }
    }

    public static class L10n
    {
        public static string TitleNetBalance = "\u0427\u0438\u0441\u0442\u044B\u0439 \u0431\u0430\u043B\u0430\u043D\u0441";
        public static string LabelIncome = "\u0414\u043E\u0445\u043E\u0434";
        public static string LabelExpense = "\u0420\u0430\u0441\u0445\u043E\u0434";
        public static string LabelPerDay = "\u0412 \u0434\u0435\u043D\u044C";
        public static string TitleTopCategories = "\u0422\u043E\u043F \u043A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u0439";
        public static string LabelTransactions = "\u0422\u0440\u0430\u043D\u0437\u0430\u043A\u0446\u0438\u0438";
    }
}
