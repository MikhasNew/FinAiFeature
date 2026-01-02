using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using EfcToXamarinAndroid.Core.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;


#if ANDROID
using MauiAppWithMudBlazor.Platforms.Android.Services;
#endif
using MauiAppWithMudBlazor.Services;


namespace MauiAppWithMudBlazor
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                  .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Register the FolderPicker as a singleton
            builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            // DI: ядро
            builder.Services.AddScoped<IDataService, DataService>();
            // UI и платформенные реализации добавим ниже (см. раздел 3)
            // ViewModel как сервис
            builder.Services.AddSingleton<EfcToXamarinAndroid.Core.ViewModels.MainViewModel>();


            // ISmsReader: Android — реальная реализация; прочие — заглушка
            
#if ANDROID
            builder.Services.AddSingleton<ISmsReader, AndroidSmsReader>();
            builder.Services.AddSingleton<Android.Content.Context>(Android.App.Application.Context);
            builder.Services.AddSingleton<EfcToXamarinAndroid.Core.Services.ISmsReader, MauiAppWithMudBlazor.Platforms.Android.Services.AndroidSmsReader>();

#else
     builder.Services.AddSingleton<ISmsReader, DummySmsReader>();
     builder.Services.AddSingleton<EfcToXamarinAndroid.Core.Services.ISmsReader, MauiAppWithMudBlazor.Services.DummySmsReader>();
#endif
            builder.Services.AddSingleton<IFileService, MauiFileService>();
            builder.Services.AddSingleton<IUIService, MauiUIService>();
            builder.Services.AddSingleton<IPermissionService, MauiPermissionService>();

            SQLitePCL.Batteries_V2.Init();

            return builder.Build();
        }
    }
}
