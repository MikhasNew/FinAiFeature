$root = "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI"
$files = Get-ChildItem -Path $root -Recurse -Include *.razor, *.cs
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content -replace 'MauiAppWithMudBlazor\.Components\.Models', 'EfcToXamarinAndroid.UI.Components.Models'
    $newContent = $newContent -replace 'WebDebugger\.Components\.Models', 'EfcToXamarinAndroid.UI.Components.Models'
    $newContent = $newContent -replace 'MauiAppWithMudBlazor\.Components', 'EfcToXamarinAndroid.UI.Components'
    $newContent = $newContent -replace 'WebDebugger\.Components', 'EfcToXamarinAndroid.UI.Components'
    $newContent = $newContent -replace 'MauiAppWithMudBlazor', 'EfcToXamarinAndroid.UI'
    $newContent = $newContent -replace 'WebDebugger', 'EfcToXamarinAndroid.UI'
    
    if ($newContent -ne $content) {
        Write-Host "Updating $($file.FullName)"
        Set-Content $file.FullName $newContent
    }
}
