$cyrillicC = [char]0x0421
$latinC = [char]0x0043
$oldName = "$($cyrillicC)ashTemplates"
$newName = "CashTemplates"

$files = @(
    "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid\Configs\ManagerCore\BankConfiguration.cs",
    "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid\Parsers\Parser.cs",
    "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI\Components\Pages\SmsRules.razor",
    "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI\Components\Dialogs\BankRuleDialog.razor",
    "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid\Configs\ConfigBank.json"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Processing $file"
        $content = Get-Content $file -Raw -Encoding UTF8
        if ($content -match $oldName) {
            $content = $content.Replace($oldName, $newName)
            Set-Content $file $content -Encoding UTF8
            Write-Host "  Replaced in $file"
        }
    }
}
