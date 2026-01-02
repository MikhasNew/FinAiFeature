using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using EfcToXamarinAndroid.Core.Services;
using Microsoft.Maui.Storage;

public class MauiFileService : IFileService
{
    private readonly IFolderPicker folderPicker;

    public MauiFileService(IFolderPicker folderPicker)
    {
        this.folderPicker = folderPicker;   
    }
    public async Task<string> PickFolderAsync()
    {
        var result = await folderPicker.PickAsync();
        if (result.IsSuccessful)
        {
            return result.Folder.Path;
        }
        else
            return null;    
    }

    public async Task<string> PickFileAsync(string title, string[] fileTypes)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = title,
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, fileTypes },
                    { DevicePlatform.iOS, fileTypes },
                    { DevicePlatform.WinUI, fileTypes }
                })
            });
            return result?.FullPath;
        }
        catch { return null; }
    }

    public async Task<bool> SaveFileAsync(string content, string fileName, string filePath)
    {
        try { await File.WriteAllTextAsync(filePath, content); return true; }
        catch { return false; }
    }

    public async Task<string> ReadFileAsync(string filePath)
    {
        try { return await File.ReadAllTextAsync(filePath); }
        catch { return null; }
    }

    public bool FileExists(string filePath) => File.Exists(filePath);

    public string GetDownloadsPath()
    {
#if ANDROID
       
            // Попробуем получить путь к Downloads
            var publicDir = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads);
            return publicDir.AbsolutePath;
       

#else
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
    }
}