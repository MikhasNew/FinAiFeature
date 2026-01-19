using Android.Webkit;

namespace MauiAppWithMudBlazor.Platforms.Android
{
    /// <summary>
    /// Custom WebChromeClient that automatically grants permission requests
    /// (e.g., Camera for QR scanning) made by web content within BlazorWebView.
    /// </summary>
    public class PermissionManagingWebChromeClient : WebChromeClient
    {
        public override void OnPermissionRequest(PermissionRequest? request)
        {
            if (request == null) return;

            // Grant all requested permissions (Camera, Microphone, etc.)
            // In a production app, you might want to filter this list
            // to only grant specific permissions like PermissionRequest.ResourceVideoCapture
            request.Grant(request.GetResources());
        }
    }
}
