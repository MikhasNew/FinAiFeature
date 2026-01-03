using WebDebugger.Components;
using MudBlazor.Services;
using EfcToXamarinAndroid.Core.Services;
using EfcToXamarinAndroid.Core.ViewModels;
using WebDebugger.Services;
using EfcToXamarinAndroid.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Seed Mock Data for Debugging
WebDebugger.Services.MockDataHelper.SeedMockData();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

