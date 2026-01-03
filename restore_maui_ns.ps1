$root = "d:\repo\NavigationDrawerStarterGemini-master\MauiWithMudBlazorUi"
$files = Get-ChildItem -Path $root -Recurse -Include *.razor, *.cs, *.xaml, *.xaml.cs, *.csproj
foreach ($file in $files) {
    if ($file.FullName -like "*\obj\*" -or $file.FullName -like "*\bin\*") { continue }
    
    $content = Get-Content $file.FullName -Raw
    # Change back the top-level namespace but keep the UI references
    $newContent = $content -replace 'namespace EfcToXamarinAndroid.UI', 'namespace MauiAppWithMudBlazor'
    $newContent = $newContent -replace 'using EfcToXamarinAndroid.UI;', 'using MauiAppWithMudBlazor;'
    $newContent = $newContent -replace 'MauiWinUIApplication', 'MauiWinUIApplication' # No change here, just placeholder
    
    # Specific fix for MauiProgram and other boot files
    $newContent = $newContent -replace 'EfcToXamarinAndroid.UI.Platforms', 'MauiAppWithMudBlazor.Platforms'
    $newContent = $newContent -replace 'EfcToXamarinAndroid.UI.Services', 'MauiAppWithMudBlazor.Services'
    
    if ($newContent -ne $content) {
        Write-Host "Restoring $($file.FullName)"
        Set-Content $file.FullName $newContent
    }
}
