using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EfcToXamarinAndroid.Core.Parsers
{
    public static class FnsQrParser
    {
        public class FnsQrData
        {
            public DateTime? Date { get; set; }
            public float? Sum { get; set; }
            public string? TransactionCode { get; set; }
        }

        public static FnsQrData Parse(string qrString)
        {
            var data = new FnsQrData();
            if (string.IsNullOrWhiteSpace(qrString)) return data;

            // Check for 24-char hex transaction code directly
            // Format check: 24 hex characters
            if (Regex.IsMatch(qrString, @"^[0-9A-Fa-f]{24}$"))
            {
                data.TransactionCode = qrString.ToUpperInvariant();
                return data;
            }

            // Example format: t=20200810T143600&s=173.00&fn=9282440300681577&i=39540&fp=2155057088&n=1
            
            // Parse Date: t=YYYYMMDDThhmmss
            var dateMatch = Regex.Match(qrString, @"t=(\d{8}T\d{6})");
            if (dateMatch.Success)
            {
                if (DateTime.TryParseExact(dateMatch.Groups[1].Value, "yyyyMMddTHHmmss", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    data.Date = date;
                }
            }

            // Parse Sum: s=123.45
            var sumMatch = Regex.Match(qrString, @"s=([\d\.]+)");
            if (sumMatch.Success)
            {
                if (float.TryParse(sumMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float sum))
                {
                    data.Sum = sum;
                }
            }

            return data;
        }
    }
}
