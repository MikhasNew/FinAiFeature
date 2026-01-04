$path = "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI\Components\Layout\MainLayout.razor"
$lines = Get-Content $path
$newLines = @()
$skipSection = $false

foreach ($line in $lines) {
    if ($line -like "*VM.ExportDataAsync*") { continue }
    if ($line -like "*VM.ImportDataAsync*") { continue }
    if ($line -like "*VM.ImportPdfAsync*") { continue }
    if ($line -like "*VM.ClearDatabaseAsync*") { continue }
    
    $newLines += $line
}
$newLines | Set-Content $path -Encoding UTF8
