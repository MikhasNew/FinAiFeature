using System.Threading.Tasks;
using Android.App;
using EfcToXamarinAndroid.Core.Services;
using Xamarin.Essentials;
using PermissionStatus = EfcToXamarinAndroid.Core.Services.PermissionStatus;

namespace NavigationDrawerStarter.Platform.Droid
{
    public class AndroidPermissionService : IPermissionService
    {
        private readonly Activity _activity;

        public AndroidPermissionService(Activity activity)
        {
            _activity = activity;
        }

        public async Task<PermissionStatus> CheckSmsPermissionAsync()
        {
            var s = await Permissions.CheckStatusAsync<Permissions.Sms>();
            return Map(s);
        }

        public async Task<PermissionStatus> RequestSmsPermissionAsync()
        {
            var s = await Permissions.RequestAsync<Permissions.Sms>();
            return Map(s);
        }

        public async Task<PermissionStatus> CheckStorageReadPermissionAsync()
        {
            var s = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            return Map(s);
        }

        public async Task<PermissionStatus> RequestStorageReadPermissionAsync()
        {
            var s = await Permissions.RequestAsync<Permissions.StorageRead>();
            return Map(s);
        }

        public async Task<PermissionStatus> CheckStorageWritePermissionAsync()
        {
            var s = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            return Map(s);
        }

        public async Task<PermissionStatus> RequestStorageWritePermissionAsync()
        {
            var s = await Permissions.RequestAsync<Permissions.StorageWrite>();
            return Map(s);
        }

        private static PermissionStatus Map(Xamarin.Essentials.PermissionStatus status) =>
            status switch
            {
                Xamarin.Essentials.PermissionStatus.Granted => PermissionStatus.Granted,
                Xamarin.Essentials.PermissionStatus.Denied => PermissionStatus.Denied,
                Xamarin.Essentials.PermissionStatus.Restricted => PermissionStatus.Restricted,
                _ => PermissionStatus.Denied
            };
    }
}