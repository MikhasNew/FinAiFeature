$path = "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI\Components\Layout\MainLayout.razor"
$lines = Get-Content $path
$newLines = @()
foreach ($line in $lines) {
    $newLines += $line
    if ($line -like "*VM.ImportDataAsync*") {
        $newLines += '            <MudMenuItem Icon="@Icons.Material.Filled.PictureAsPdf" OnClick="@(() => VM.ImportPdfAsync())">Import from PDF</MudMenuItem>'
    }
}
$newLines | Set-Content $path -Encoding UTF8
