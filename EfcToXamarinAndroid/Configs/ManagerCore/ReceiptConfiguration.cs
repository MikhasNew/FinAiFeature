namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    public class ReceiptConfiguration
    {
        public string Name { get; set; }
        
        // Identifiers to select this configuration
        public string? SenderEmail { get; set; } // For email parsing
        public string? UrlPattern { get; set; } // For QR/Web parsing
        
        public ReceiptParseRegex ParseRegex { get; set; }
    }
}
