// EfcToXamarinAndroid.Core/Services/IUIService.cs
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Services
{
    public interface IUIService
    {
        Task ShowToastAsync(string message);
        Task<bool> ShowConfirmationDialogAsync(string title, string message);
        Task<string> ShowInputDialogAsync(string title, string message, string defaultValue = "");
    }
}