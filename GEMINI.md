# Project: Financial Reporting Android App

## Project Overview

ты можешь в дальнейшем общаться со мной только на русском языке.

This project is a financial reporting application for Android. It is designed to help users track their financial data by importing and parsing data from SMS messages and PDF files. The application uses a local SQLite database to store the data.

The solution is composed of three main projects:

1.  **`NavigationDrawerStarter`**: The main Xamarin.Android application. It provides the user interface and platform-specific services for Android.
2.  **`EfcToXamarinAndroid.Core`**: A .NET 8 class library that contains the core business logic, data models, and services. This includes data parsing, database interactions with Entity Framework Core, and view models.
3.  **`MauiWithMudBlazorUi`**: A .NET MAUI application that uses MudBlazor for its UI components. It seems to be a newer or experimental user interface for the application, sharing the same core logic as the Xamarin.Android app.

## Building and Running

To build and run this project, you will need Visual Studio with the .NET Multi-platform App UI development workload installed.

1.  Open the `NavigationDrawerStarter.sln` file in Visual Studio.
2.  To run the Xamarin.Android application:
    *   Set `NavigationDrawerStarter` as the startup project.
    *   Choose an Android emulator or a connected Android device.
    *   Press the "Run" button.
3.  To run the .NET MAUI application:
    *   Set `MauiWithMudBlazorUi` as the startup project.
    *   Choose an Android emulator or a connected Android device.
    *   Press the "Run" button.

## Development Conventions

*   The core business logic is kept separate from the UI projects in the `EfcToXamarinAndroid.Core` project.
*   The application uses an MVVM (Model-View-ViewModel) architecture, with the `MainViewModel` in the `EfcToXamarinAndroid.Core` project orchestrating the application's logic.
*   Platform-specific services are implemented in the respective UI projects (e.g., `AndroidSmsReader` in the `NavigationDrawerStarter` project and `MauiPermissionService` in the `MauiWithMudBlazorUi` project) and consumed through interfaces defined in the core project.
*   Configuration for banks and MCC codes is stored in JSON files (`ConfigBank.json`, `ConfigMcc.json`) and embedded as resources in the `EfcToXamarinAndroid.Core` project.
