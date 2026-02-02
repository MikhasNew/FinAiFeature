using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;

namespace EfcToXamarinAndroid.Core.Parsers
{
    public class ReceiptParser
    {
        private readonly List<ReceiptConfiguration> _configurations;

        public ReceiptParser(List<ReceiptConfiguration> configurations)
        {
            _configurations = configurations;
        }

        public Receipt? Parse(string content, string sourceIdentifier, bool isEmail)
        {
            // Find suitable configuration
            var config = _configurations.Find(c => 
                (isEmail && c.SenderEmail == sourceIdentifier) || 
                (!isEmail && !string.IsNullOrEmpty(c.UrlPattern) && Regex.IsMatch(sourceIdentifier, c.UrlPattern)));

            // Fallback: try to find a generic configuration (SenderEmail == "*")
            if (config == null && isEmail)
            {
                config = _configurations.Find(c => c.SenderEmail == "*");
            }

            if (config == null || config.ParseRegex == null)
            {
                return null;
            }

            return ParseWithConfig(content, config.ParseRegex);
        }

        private Receipt ParseWithConfig(string content, ReceiptParseRegex regex)
        {
            var receipt = new Receipt
            {
                RawData = content, // Store raw content for debugging/reverification
                Items = new List<ReceiptItem>()
            };

            var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";

            // Parse Receipt Level Data
            if (!string.IsNullOrEmpty(regex.ShopName))
            {
                receipt.ShopName = GetRegexValue(content, regex.ShopName);
            }

            if (!string.IsNullOrEmpty(regex.ShopInn))
                receipt.ShopInn = GetRegexValue(content, regex.ShopInn);

            if (!string.IsNullOrEmpty(regex.Address))
                receipt.Address = GetRegexValue(content, regex.Address);

            if (!string.IsNullOrEmpty(regex.ReceiptDate))
            {
                string dateStr = GetRegexValue(content, regex.ReceiptDate);
                if (TryParseAnyDate(dateStr, out DateTime date))
                {
                    receipt.ReceiptDate = date;
                }
            }

            if (!string.IsNullOrEmpty(regex.TotalSum))
            {
                // Support comma and dot
                string val = GetRegexValue(content, regex.TotalSum).Replace(",", ".");
                // Clean up any non-numeric chars if necessary, but TryParse covers most
                // Regex usually extracts just the number, but let's be safe
                receipt.TotalSum = float.TryParse(val, NumberStyles.Any, ci, out float sum) ? sum : 0;
            }

            if (!string.IsNullOrEmpty(regex.Currency))
                receipt.Currency = GetRegexValue(content, regex.Currency);

            if (!string.IsNullOrEmpty(regex.RegNumber))
                receipt.RegNumber = GetRegexValue(content, regex.RegNumber);

            if (!string.IsNullOrEmpty(regex.CardNumber))
                receipt.CardNumber = GetRegexValue(content, regex.CardNumber);

            if (!string.IsNullOrEmpty(regex.EripPayerNumber))
                receipt.EripPayerNumber = GetRegexValue(content, regex.EripPayerNumber);

            if (!string.IsNullOrEmpty(regex.OrderNumber))
                receipt.OrderNumber = GetRegexValue(content, regex.OrderNumber);

            if (!string.IsNullOrEmpty(regex.Subject))
                receipt.Subject = GetRegexValue(content, regex.Subject);

            // Parse Items
            if (!string.IsNullOrEmpty(regex.ItemLine))
            {
                var matches = Regex.Matches(content, regex.ItemLine);
                foreach (Match match in matches)
                {
                    var itemBlock = match.Value;
                    var item = new ReceiptItem();

                    if (!string.IsNullOrEmpty(regex.ItemName))
                        item.Name = GetRegexValue(itemBlock, regex.ItemName);

                    if (!string.IsNullOrEmpty(regex.ItemQuantity))
                        item.Quantity = double.TryParse(GetRegexValue(itemBlock, regex.ItemQuantity).Replace(",", "."), NumberStyles.Any, ci, out double q) ? q : 0;

                    if (!string.IsNullOrEmpty(regex.ItemPrice))
                        item.Price = float.TryParse(GetRegexValue(itemBlock, regex.ItemPrice).Replace(",", "."), NumberStyles.Any, ci, out float p) ? p : 0;

                    if (!string.IsNullOrEmpty(regex.ItemSum))
                        item.Sum = float.TryParse(GetRegexValue(itemBlock, regex.ItemSum).Replace(",", "."), NumberStyles.Any, ci, out float s) ? s : 0;

                    if (!string.IsNullOrEmpty(regex.ItemCode))
                        item.Code = GetRegexValue(itemBlock, regex.ItemCode);

                    receipt.Items.Add(item);
                }
            }

            return receipt;
        }

        private static string GetRegexValue(string input, string pattern)
        {
            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // Return the first successful capturing group (skipping group 0 which is the whole match)
                if (match.Groups.Count > 1)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        if (match.Groups[i].Success)
                        {
                            return match.Groups[i].Value.Trim();
                        }
                    }
                }
                // Fallback to whole match if no groups defined
                return match.Value.Trim();
            }
            return string.Empty;
        }

        private static bool TryParseAnyDate(string input, out DateTime value)
        {
             string[] formats = new[]
            {
                "dd.MM.yyyy HH:mm:ss","dd.MM.yyyy HH:mm","dd.MM.yyyy",
                "dd/MM/yyyy HH:mm:ss","dd/MM/yyyy HH:mm","dd/MM/yyyy",
                "yyyy-MM-dd HH:mm:ss","yyyy-MM-dd HH:mm","yyyy-MM-dd",
                "MM/dd/yyyy HH:mm:ss","MM/dd/yyyy HH:mm","MM/dd/yyyy",
            };
            var styles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;
            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, styles, out value)) return true;
            if (DateTime.TryParse(input, new CultureInfo("ru-RU"), styles, out value)) return true;
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, styles, out value)) return true;
            return DateTime.TryParse(input, CultureInfo.CurrentCulture, styles, out value);
        }
    }
}
