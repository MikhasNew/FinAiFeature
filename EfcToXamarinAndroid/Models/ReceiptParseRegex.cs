namespace EfcToXamarinAndroid.Core
{
    public class ReceiptParseRegex
    {
        // Regex patterns for Receipt level
        public string? ShopName { get; set; }
        public string? ShopInn { get; set; }
        public string? Address { get; set; }
        public string? ReceiptDate { get; set; }
        public string? TotalSum { get; set; }
        public string? Currency { get; set; }
        public string? RegNumber { get; set; }
        public string? CardNumber { get; set; }
        public string? EripPayerNumber { get; set; }
        public string? OrderNumber { get; set; }
        public string? Subject { get; set; }
        
        // Regex pattern to identify an individual item line/block
        // This regex should match one item entry.
        public string? ItemLine { get; set; }
        
        // Regex patterns to extract details from the matched ItemLine
        public string? ItemName { get; set; }
        public string? ItemQuantity { get; set; }
        public string? ItemPrice { get; set; }
        public string? ItemSum { get; set; }
        public string? ItemCode { get; set; }
    }
}
