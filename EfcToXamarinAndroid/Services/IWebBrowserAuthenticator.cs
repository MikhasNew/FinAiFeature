using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Services
{
    public interface IWebBrowserAuthenticator
    {
        Task<IDictionary<string, string>> AuthenticateAsync(Uri url, Uri callbackUrl);
    }
}
