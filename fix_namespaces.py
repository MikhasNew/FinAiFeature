import os

root_dir = r'd:\repo\NavigationDrawerStarterGemini-master\EfcToXamarinAndroid.UI'
replacements = {
    'MauiAppWithMudBlazor.Components.Models': 'EfcToXamarinAndroid.UI.Components.Models',
    'WebDebugger.Components.Models': 'EfcToXamarinAndroid.UI.Components.Models',
    'MauiAppWithMudBlazor.Components': 'EfcToXamarinAndroid.UI.Components',
    'WebDebugger.Components': 'EfcToXamarinAndroid.UI.Components',
    'MauiAppWithMudBlazor': 'EfcToXamarinAndroid.UI',
    'WebDebugger': 'EfcToXamarinAndroid.UI'
}

for root, dirs, files in os.walk(root_dir):
    for file in files:
        if file.endswith('.razor') or file.endswith('.cs'):
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            new_content = content
            for old, new in replacements.items():
                new_content = new_content.replace(old, new)
            
            if new_content != content:
                print(f"Updating {path}")
                with open(path, 'w', encoding='utf-8') as f:
                    f.write(new_content)
