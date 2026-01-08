// EfcToXamarinAndroid.Core/Services/IPermissionService.cs
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Services
{
    public enum PermissionStatus
    {
        Denied,
        Granted,
        Restricted
    }

    public interface IPermissionService
    {
        Task<PermissionStatus> CheckSmsPermissionAsync();
        Task<PermissionStatus> RequestSmsPermissionAsync();
        Task<PermissionStatus> CheckStorageReadPermissionAsync();
        Task<PermissionStatus> RequestStorageReadPermissionAsync();
        Task<PermissionStatus> CheckStorageWritePermissionAsync();
        Task<PermissionStatus> RequestStorageWritePermissionAsync();
    }
}