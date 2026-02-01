namespace EfcToXamarinAndroid.Core.Services
{
    /// <summary>
    /// OAuth 2.0 провайдер для Яндекс Почты
    /// </summary>
    public class YandexOAuthProvider : IOAuthProvider
    {
        public string ProviderName => "Yandex";

        public string AuthEndpoint => "https://oauth.yandex.ru/authorize";

        public string TokenEndpoint => "https://oauth.yandex.ru/token";

        // Для доступа к почте через IMAP нужен scope mail:imap_full
        // login:email и login:info дают только доступ к профилю
        public string Scope => "login:email login:info mail:imap_full";
        // Согласно документации Яндекса: imap.yandex.com:993
        public string ImapHost => "imap.yandex.com";

        public int ImapPort => 993;
    }
}
