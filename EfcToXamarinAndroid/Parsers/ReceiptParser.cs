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
                receipt.ShopName = Regex.Match(content, regex.ShopName).Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(receipt.ShopName)) 
                    receipt.ShopName = Regex.Match(content, regex.ShopName).Value.Trim();
            }

            if (!string.IsNullOrEmpty(regex.ShopInn))
                receipt.ShopInn = Regex.Match(content, regex.ShopInn).Value.Trim();

            if (!string.IsNullOrEmpty(regex.Address))
                receipt.Address = Regex.Match(content, regex.Address).Value.Trim();

            if (!string.IsNullOrEmpty(regex.ReceiptDate))
            {
                string dateStr = Regex.Match(content, regex.ReceiptDate).Value;
                if (TryParseAnyDate(dateStr, out DateTime date))
                {
                    receipt.ReceiptDate = date;
                }
            }

            if (!string.IsNullOrEmpty(regex.TotalSum))
            {
               receipt.TotalSum = float.TryParse(Regex.Match(content, regex.TotalSum).Value.Replace(",", "."), NumberStyles.Any, ci, out float sum) ? sum : 0;
            }

            // Parse Items
            if (!string.IsNullOrEmpty(regex.ItemLine))
            {
                var matches = Regex.Matches(content, regex.ItemLine);
                foreach (Match match in matches)
                {
                    var itemBlock = match.Value;
                    var item = new ReceiptItem();

                    if (!string.IsNullOrEmpty(regex.ItemName))
                        item.Name = Regex.Match(itemBlock, regex.ItemName).Groups[1].Value.Trim();
                        if (string.IsNullOrEmpty(item.Name)) item.Name = Regex.Match(itemBlock, regex.ItemName).Value.Trim();

                    if (!string.IsNullOrEmpty(regex.ItemQuantity))
                        item.Quantity = double.TryParse(Regex.Match(itemBlock, regex.ItemQuantity).Value.Replace(",", "."), NumberStyles.Any, ci, out double q) ? q : 0;

                    if (!string.IsNullOrEmpty(regex.ItemPrice))
                        item.Price = float.TryParse(Regex.Match(itemBlock, regex.ItemPrice).Value.Replace(",", "."), NumberStyles.Any, ci, out float p) ? p : 0;

                    if (!string.IsNullOrEmpty(regex.ItemSum))
                        item.Sum = float.TryParse(Regex.Match(itemBlock, regex.ItemSum).Value.Replace(",", "."), NumberStyles.Any, ci, out float s) ? s : 0;

                    if (!string.IsNullOrEmpty(regex.ItemCode))
                        item.Code = Regex.Match(itemBlock, regex.ItemCode).Value.Trim();

                    receipt.Items.Add(item);
                }
            }

            return receipt;
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
