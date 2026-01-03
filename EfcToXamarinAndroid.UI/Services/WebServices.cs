using EfcToXamarinAndroid.Core.Services;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;

using EfcToXamarinAndroid.Core.Repository;
using EfcToXamarinAndroid.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;


namespace EfcToXamarinAndroid.UI.Services
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
        private readonly Microsoft.JSInterop.IJSRuntime _js;
        private readonly MudBlazor.IDialogService _dialogService;

        public WebFileService(Microsoft.JSInterop.IJSRuntime js, MudBlazor.IDialogService dialogService)
        {
            _js = js;
            _dialogService = dialogService;
        }

        public async Task<string> PickFolderAsync() 
        {
            Console.WriteLine("Entering PickFolderAsync");
            try
            {
                var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
                
                var parameters = new MudBlazor.DialogParameters<EfcToXamarinAndroid.UI.Components.Dialogs.PathInputDialog>
                {
                    { x => x.Title, "Выбор папки для экспорта" },
                    { x => x.Message, "Введите путь к папке (для симуляции выбор папки в MAUI):" },
                    { x => x.Value, defaultPath }
                };

                var dialog = await _dialogService.ShowAsync<EfcToXamarinAndroid.UI.Components.Dialogs.PathInputDialog>("Выбор пути", parameters);
                var result = await dialog.Result;

                if (result != null && !result.Canceled)
                {
                    return result.Data?.ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PickFolderAsync: {ex.Message}");
                return null;
            }
        }
        public async Task<string> PickFileAsync(string title, string[] fileTypes) 
        {
            Console.WriteLine("Entering PickFileAsync");
            try
            {
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
                var lastFile = "";
                List<string> files = new();

                if (Directory.Exists(dir))
                {
                    files = Directory.GetFiles(dir, "*.xml").OrderByDescending(f => f).ToList();
                    lastFile = files.FirstOrDefault() ?? "";
                }
                
                var parameters = new MudBlazor.DialogParameters<EfcToXamarinAndroid.UI.Components.Dialogs.PathInputDialog>
                {
                    { x => x.Title, title },
                    { x => x.Message, "Введите путь к файлу или выберите из списка:" },
                    { x => x.Value, lastFile },
                    { x => x.Files, files }
                };

                var dialog = await _dialogService.ShowAsync<EfcToXamarinAndroid.UI.Components.Dialogs.PathInputDialog>("Выбор файла", parameters);
                var result = await dialog.Result;

                if (result != null && !result.Canceled)
                {
                    return result.Data?.ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PickFileAsync: {ex.Message}");
                return null;
            }
        }
        public Task<bool> SaveFileAsync(string content, string fileName, string filePath) 
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(filePath, content);
                return Task.FromResult(true);
            }
            catch { return Task.FromResult(false); }
        }
        public Task<string> ReadFileAsync(string filePath) => File.Exists(filePath) ? Task.FromResult(File.ReadAllText(filePath)) : Task.FromResult("");
        public bool FileExists(string filePath) => File.Exists(filePath);
        public string GetDownloadsPath() 
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }

    public class WebUIService : IUIService
    {
        private readonly MudBlazor.ISnackbar _snackbar;
        private readonly MudBlazor.IDialogService _dialogService;

        public WebUIService(MudBlazor.ISnackbar snackbar, MudBlazor.IDialogService dialogService)
        {
            _snackbar = snackbar;
            _dialogService = dialogService;
        }

        public Task ShowToastAsync(string message) 
        { 
            _snackbar.Add(message, MudBlazor.Severity.Info);
            return Task.CompletedTask; 
        }

        public async Task<bool> ShowConfirmationDialogAsync(string title, string message) 
        {
            var result = await _dialogService.ShowMessageBox(title, message, yesText: "Да", noText: "Нет");
            return result ?? false;
        }

        public async Task<string> ShowInputDialogAsync(string title, string message, string defaultValue = "") 
        {
            // Simple input dialog using MudBlazor would require a custom component, 
            // but for debugging we can return default or use JS prompt
            return defaultValue;
        }
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

