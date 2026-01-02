using EfcToXamarinAndroid.Core.Services;

using PermissionStatus = EfcToXamarinAndroid.Core.Services.PermissionStatus;

public class MauiPermissionService : IPermissionService
{
    public async Task<PermissionStatus> CheckSmsPermissionAsync()
        => Map(await Permissions.CheckStatusAsync<Permissions.Sms>());

    public async Task<PermissionStatus> RequestSmsPermissionAsync()
        => Map(await Permissions.RequestAsync<Permissions.Sms>());

    public async Task<PermissionStatus> CheckStorageReadPermissionAsync()
        => Map(await Permissions.CheckStatusAsync<Permissions.StorageRead>());

    public async Task<PermissionStatus> RequestStorageReadPermissionAsync()
        => Map(await Permissions.RequestAsync<Permissions.StorageRead>());

    public async Task<PermissionStatus> CheckStorageWritePermissionAsync()
    {
#if ANDROID
        // На Android 13+ WRITE_EXTERNAL_STORAGE игнорируется: используйте scoped storage.
        return Map(await Permissions.CheckStatusAsync<Permissions.StorageWrite>());
#else
        return Map(await Permissions.CheckStatusAsync<Permissions.StorageWrite>());
#endif
    }

    public async Task<PermissionStatus> RequestStorageWritePermissionAsync()
    {
#if ANDROID
        return Map(await Permissions.RequestAsync<Permissions.StorageWrite>());
#else
        return Map(await Permissions.RequestAsync<Permissions.StorageWrite>());
#endif
    }

    private static PermissionStatus Map(Microsoft.Maui.ApplicationModel.PermissionStatus s)
        => s switch
        {
            Microsoft.Maui.ApplicationModel.PermissionStatus.Granted => PermissionStatus.Granted,
            Microsoft.Maui.ApplicationModel.PermissionStatus.Restricted => PermissionStatus.Restricted,
            _ => PermissionStatus.Denied
        };
}