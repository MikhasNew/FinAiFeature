using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using EfcToXamarinAndroid.Core.Services;

namespace NavigationDrawerStarter.Platform.Droid
{
    public class AndroidUIService : IUIService
    {
        private readonly Activity _activity;

        public AndroidUIService(Activity activity)
        {
            _activity = activity;
        }

        public Task ShowToastAsync(string message)
        {
            _activity.RunOnUiThread(() =>
            {
                Toast.MakeText(_activity, message, ToastLength.Short).Show();
            });
            return Task.CompletedTask;
        }

        public Task<bool> ShowConfirmationDialogAsync(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            _activity.RunOnUiThread(() =>
            {
                var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(_activity);
                builder.SetTitle(title);
                builder.SetMessage(message);
                builder.SetCancelable(false);
                builder.SetPositiveButton("OK", (c, e) => tcs.TrySetResult(true));
                builder.SetNegativeButton("Отмена", (c, e) => tcs.TrySetResult(false));
                builder.Create()?.Show();
            });

            return tcs.Task;
        }

        public Task<string> ShowInputDialogAsync(string title, string message, string defaultValue = "")
        {
            var tcs = new TaskCompletionSource<string>();

            _activity.RunOnUiThread(() =>
            {
                var editText = new Android.Widget.EditText(_activity) { Text = defaultValue };

                var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(_activity);
                builder.SetTitle(title);
                builder.SetMessage(message);
                builder.SetView(editText);
                builder.SetPositiveButton("OK", (c, e) => tcs.TrySetResult(editText.Text));
                builder.SetNegativeButton("Отмена", (c, e) => tcs.TrySetResult(null));
                builder.Create()?.Show();
            });

            return tcs.Task;
        }
    }
}