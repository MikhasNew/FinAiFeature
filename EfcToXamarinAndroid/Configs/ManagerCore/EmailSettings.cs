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
    }
}
