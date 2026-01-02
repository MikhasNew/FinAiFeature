// EfcToXamarinAndroid.Core/Services/IFileService.cs
using System.Threading;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Services
{
    public interface IFileService
    {
        Task<string> PickFolderAsync();
        Task<string> PickFileAsync(string title, string[] fileTypes);
        Task<bool> SaveFileAsync(string content, string fileName, string filePath);
        Task<string> ReadFileAsync(string filePath);
        bool FileExists(string filePath);
        string GetDownloadsPath();
    }
}