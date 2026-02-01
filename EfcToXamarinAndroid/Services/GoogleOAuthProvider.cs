namespace EfcToXamarinAndroid.Core.Services
{
    /// <summary>
    /// OAuth 2.0 провайдер для Google Gmail
    /// </summary>
    public class GoogleOAuthProvider : IOAuthProvider
    {
        public string ProviderName => "Google";

        public string AuthEndpoint => "https://accounts.google.com/o/oauth2/v2/auth";

        public string TokenEndpoint => "https://oauth2.googleapis.com/token";

        public string Scope => "https://mail.google.com/";

        public string ImapHost => "imap.gmail.com";

        public int ImapPort => 993;
    }
}
