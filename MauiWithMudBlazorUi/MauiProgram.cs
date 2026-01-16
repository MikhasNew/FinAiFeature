using Microsoft.Maui.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using EfcToXamarinAndroid.Core.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.AspNetCore.Components.WebView.Maui;

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
            var config = EfcToXamarinAndroid.Core.Configs.ManagerCore.ConfigurationManager.ConfigManager.BankConfigurationFromJson;
            builder.Services.AddSingleton(config);
            builder.Services.AddSingleton<EfcToXamarinAndroid.Core.Parsers.ReceiptParser>(new EfcToXamarinAndroid.Core.Parsers.ReceiptParser(config.ReceiptConfigurations));
            builder.Services.AddSingleton<IQrCodeService, QrCodeService>();

            // Load Google OAuth secrets
            try
            {
                // Warning: Blocking async call on startup, but necessary for config
                using var stream = FileSystem.OpenAppPackageFileAsync("google_secrets.json").Result;
                EfcToXamarinAndroid.Core.Configs.GoogleAuthConfig.LoadFromStream(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MauiProgram] Failed to load google_secrets.json: {ex.Message}");
            }

            // Email & Receipt Services
            builder.Services.AddSingleton<OAuthService>();
            builder.Services.AddSingleton<ReceiptProcessor>();
            builder.Services.AddSingleton<IEmailService, EmailService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // DI: Core services as Singletons (compatible with MainViewModel)
            builder.Services.AddSingleton<IDataService, DataService>();
            builder.Services.AddSingleton<EfcToXamarinAndroid.Core.ViewModels.MainViewModel>();

            // Platform-specific ISmsReader
#if ANDROID
            builder.Services.AddSingleton<ISmsReader, AndroidSmsReader>();
            builder.Services.AddSingleton<Android.Content.Context>(Android.App.Application.Context);
            Microsoft.Maui.Handlers.ViewHandler.ViewMapper.AppendToMapping("CustomScrollConfig", (handler, view) =>
            {
                if (view is IBlazorWebView)
                {
                    var platformWebView = (Android.Webkit.WebView)handler.PlatformView;

                    // ������������� ����� ������� ������ ��������
                    platformWebView.ScrollBarStyle = Android.Views.ScrollbarStyles.InsideOverlay;

                    // ��������� ��������� ������������ ������ (����� CSS ��� �� ���������)
                    platformWebView.VerticalScrollBarEnabled = true;

                    // ������� ����� "��������" ��� ���������� ����� ������
                    platformWebView.OverScrollMode = Android.Views.OverScrollMode.Never;

                    //��������� ���������� ��������� ��� WebView (������ ��� �����)
                    //platformWebView.SetLayerType(Android.Views.LayerType.Software, null);
                }
            });
#else
            builder.Services.AddSingleton<ISmsReader, DummySmsReader>();
#endif
            builder.Services.AddSingleton<IFileService, MauiFileService>();
            builder.Services.AddSingleton<IUIService, MauiUIService>();
            builder.Services.AddSingleton<IPermissionService, MauiPermissionService>();

            SQLitePCL.Batteries_V2.Init();

            return builder.Build();
        }
    }
}
