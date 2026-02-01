using EfcToXamarinAndroid.Core.Services;
using Microsoft.Maui.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiAppWithMudBlazor.Services
{
    public class MauiWebBrowserAuthenticator : IWebBrowserAuthenticator
    {
        public async Task<IDictionary<string, string>> AuthenticateAsync(Uri url, Uri callbackUrl)
        {
            try
            {
                var result = await WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions
                {
                    Url = url,
                    CallbackUrl = callbackUrl,
                    PrefersEphemeralWebBrowserSession = true
                });

                return result?.Properties;
            }
            catch (TaskCanceledException)
            {
                // User canceled the auth flow
                return null;
            }
        }
    }
}
