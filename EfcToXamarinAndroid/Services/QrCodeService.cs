using System;
using System.Net.Http;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Parsers;

namespace EfcToXamarinAndroid.Core.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly AppConfiguration _configuration;
        private readonly ReceiptParser _receiptParser;
        private readonly HttpClient _httpClient;

        public QrCodeService(AppConfiguration configuration, ReceiptParser receiptParser)
        {
            _configuration = configuration;
            _receiptParser = receiptParser;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public async Task<Receipt?> GetReceiptByUrlAsync(string url)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(url);
                return await ParseRawReceiptDataAsync(html, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching receipt from URL {url}: {ex.Message}");
                return null;
            }
        }

        public async Task<Receipt?> ParseRawReceiptDataAsync(string rawData)
        {
            return await ParseRawReceiptDataAsync(rawData, "manual_input");
        }

        public async Task<Receipt?> ParseRawReceiptDataAsync(string rawData, string identifier)
        {
            // The logic here is mostly handled by ReceiptParser which works on the raw content
            // We pass the URL as the identifier to help select the right configuration
            return await Task.Run(() => _receiptParser.Parse(rawData, identifier, false));
        }

        public async Task<Receipt?> GetReceiptByTransactionCodeAsync(DateTime date, string code)
        {
            try
            {
                var content = new MultipartFormDataContent();

                // »зменили им€ параметра на то, которое требует сервер
                content.Add(new StringContent(date.ToString("yyyy-MM-dd")), "orig_date");

                content.Add(new StringContent(code), "orig_ui");

                var response = await _httpClient.PostAsync("https://ch.info-center.by/ajax/check1.php", content);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                // “еперь jsonString должен содержать {"status":"success", ...}
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<CheckApiResponse>(jsonString);


                if (apiResponse?.Message == null || apiResponse.Status != "success")
                {
                    return null;
                }

                var msg = apiResponse.Message;

                var receipt = new Receipt
                {
                    ReceiptDate = date,
                    TotalSum = (float)msg.TotalAmount,
                    ShopName = msg.NameTo ?? msg.NameSpd,
                    ShopInn = msg.Unp,
                    Address = $"{msg.NameNp}, {msg.StreetTo}, {msg.HouseTo}",
                    ReceiptDateString = msg.IssuedAt, 
                    RawData = jsonString,
                    Currency = msg.Currency,
                    Items = new System.Collections.Generic.List<ReceiptItem>()
                };

                // Try to parse the date from the response if possible, to be more precise
                if (DateTime.TryParseExact(msg.IssuedAt, "dd/MM/yyyy, HH:mm:ss", 
                    System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var issuedDate))
                {
                    receipt.ReceiptDate = issuedDate;
                }

                // Parse positions (nested JSON string)
                if (!string.IsNullOrEmpty(msg.Positions))
                {
                    try 
                    {
                        var positions = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<CheckApiPosition>>(msg.Positions);
                        if (positions != null)
                        {
                            foreach (var pos in positions)
                            {
                                if (float.TryParse(pos.Amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float amount) &&
                                    float.TryParse(pos.ProductCount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float quantity))
                                {
                                     receipt.Items.Add(new ReceiptItem
                                     {
                                         Name = pos.ProductName,
                                         Quantity = quantity,
                                         Sum = amount,
                                         Price = quantity != 0 ? amount / quantity : 0
                                     });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing positions JSON: {ex.Message}");
                    }
                }

                return receipt;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching receipt by transaction code: {ex.Message}");
                return null;
            }
        }

        private class CheckApiResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("status")]
            public string Status { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("message")]
            public CheckApiMessage Message { get; set; }
        }

        private class CheckApiMessage
        {
            [System.Text.Json.Serialization.JsonPropertyName("total_amount")]
            public double TotalAmount { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("unp")]
            public string Unp { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("issued_at")]
            public string IssuedAt { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("currency")]
            public string Currency { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("positions")]
            public string Positions { get; set; } // Nested JSON string

            [System.Text.Json.Serialization.JsonPropertyName("name_spd")]
            public string NameSpd { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("name_to")]
            public string NameTo { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("name_np")]
            public string NameNp { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("street_to")]
            public string StreetTo { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("house_to")]
            public string HouseTo { get; set; }
        }

        private class CheckApiPosition
        {
            [System.Text.Json.Serialization.JsonPropertyName("product_name")]
            public string ProductName { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("product_count")]
            public string ProductCount { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("amount")]
            public string Amount { get; set; }
        }
    }
}
