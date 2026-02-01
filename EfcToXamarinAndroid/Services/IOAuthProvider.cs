namespace EfcToXamarinAndroid.Core.Services
{
    /// <summary>
    /// Интерфейс для OAuth 2.0 провайдеров (Google, Yandex и т.д.)
    /// </summary>
    public interface IOAuthProvider
    {
        /// <summary>
        /// Название провайдера (Google, Yandex и т.д.)
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// URL для авторизации пользователя
        /// </summary>
        string AuthEndpoint { get; }

        /// <summary>
        /// URL для обмена authorization code на токены
        /// </summary>
        string TokenEndpoint { get; }

        /// <summary>
        /// Права доступа (scopes) для почтового доступа
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// IMAP сервер для этого провайдера
        /// </summary>
        string ImapHost { get; }

        /// <summary>
        /// IMAP порт для этого провайдера
        /// </summary>
        int ImapPort { get; }
    }
}
