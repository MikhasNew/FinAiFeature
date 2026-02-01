using System;

namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    public class EmailSettings
    {
        public string? ImapHost { get; set; }
        public int ImapPort { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool UseSsl { get; set; } = true;
        public string? FolderToScan { get; set; } = "INBOX";
        public string? SubjectFilter { get; set; }
        public string? SenderFilter { get; set; }

        // OAuth2 Settings
        public bool UseOAuth { get; set; } = false;
        
        /// <summary>
        /// OAuth провайдер: "Google" или "Yandex"
        /// </summary>
        public string OAuthProvider { get; set; } = "Google";
        
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
    }
}
