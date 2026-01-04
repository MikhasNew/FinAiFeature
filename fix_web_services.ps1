$path = "d:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI\Services\WebServices.cs"
$content = Get-Content $path -Raw
$old = 'files = Directory.GetFiles(dir, "*.xml").OrderByDescending(f => f).ToList();'
$new = 'var patterns = (fileTypes != null && fileTypes.Length > 0) ? fileTypes.Select(t => "*" + t) : new string[] { "*.*" }; foreach (var pattern in patterns) { files.AddRange(Directory.GetFiles(dir, pattern)); } files = files.Distinct().OrderByDescending(f => f).ToList();'
$content = $content.Replace($old, $new)
Set-Content $path $content -Encoding UTF8
