using EfcToXamarinAndroid.Core.Services;

public class MauiUIService : IUIService
{
    public Task ShowToastAsync(string message)
        => Application.Current.MainPage.DisplayAlert("Сообщение", message, "OK");

    public Task<bool> ShowConfirmationDialogAsync(string title, string message)
        => Application.Current.MainPage.DisplayAlert(title, message, "OK", "Отмена");

    public async Task<string> ShowInputDialogAsync(string title, string message, string defaultValue = "")
    {
        // Быстро: нет стандартного InputDialog — используйте MudDialog на страницах.
        await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        return defaultValue;
    }
}