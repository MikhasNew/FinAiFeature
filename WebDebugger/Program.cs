using EfcToXamarinAndroid.UI.Components;
using MudBlazor.Services;
using EfcToXamarinAndroid.Core.Services;
using EfcToXamarinAndroid.Core.ViewModels;
using EfcToXamarinAndroid.UI.Services;
using EfcToXamarinAndroid.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
EfcToXamarinAndroid.Core.Configs.GoogleAuthConfig.Load(builder.Environment.ContentRootPath);
// Also check root for google_secrets if starting from subfolder
EfcToXamarinAndroid.Core.Configs.GoogleAuthConfig.Load(System.IO.Path.Combine(builder.Environment.ContentRootPath, ".."));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Core Services
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<MainViewModel>();


// Web Stubs
builder.Services.AddSingleton<ISmsReader, WebSmsReader>();
builder.Services.AddScoped<IFileService, WebFileService>();
builder.Services.AddScoped<IUIService, WebUIService>();
builder.Services.AddSingleton<IPermissionService, WebPermissionService>();

// Email & Receipt Services
builder.Services.AddSingleton(provider => EfcToXamarinAndroid.Core.Configs.ManagerCore.ConfigurationManager.ConfigManager.BankConfigurationFromJson);
// Assuming ReceiptParser needs a list of ReceiptConfigurations which is inside AppConfiguration now
builder.Services.AddScoped<EfcToXamarinAndroid.Core.Parsers.ReceiptParser>(provider => 
{
    var config = provider.GetRequiredService<EfcToXamarinAndroid.Core.Configs.ManagerCore.AppConfiguration>();
    return new EfcToXamarinAndroid.Core.Parsers.ReceiptParser(config.ReceiptConfigurations);
});
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<OAuthService>();

// Seed Mock Data for Debugging
// EfcToXamarinAndroid.UI.Services.MockDataHelper.SeedMockData();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


