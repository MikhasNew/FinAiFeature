
namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    public class BankConfiguration
    {
        public string Name { get; set; }
        public string SmsNumber { get; set; }
        public string[] PaymentTemplates { get; set; }
        public string[] DepositTemplates { get; set; }
        public string[] СashTemplates { get; set; }

        public SmsParseRegex SmsParseRegex { get; set; }
        public PdfReportTemplate PdfReportTemplate { get; set; }

    }
}
