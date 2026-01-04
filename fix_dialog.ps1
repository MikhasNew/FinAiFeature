$path = "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI\Components\Dialogs\BankRuleDialog.razor"
$content = Get-Content $path -Raw
$content = $content.Replace("[CascadingParameter] MudDialogInstance MudDialog", "[CascadingParameter] IMudDialogInstance MudDialog")
Set-Content $path $content -Encoding UTF8
