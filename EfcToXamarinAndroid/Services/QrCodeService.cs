using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Parsers;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

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
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<Receipt?> GetReceiptByUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var content = response;

                // Если ответ — HTML, извлекаем текст для regex-парсинга (iKassa и др.)
                if (response.TrimStart().StartsWith("<", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(response);
                        var body = doc.DocumentNode.SelectSingleNode("//body");
                        content = body?.InnerText ?? doc.DocumentNode.InnerText ?? response;
                    }
                    catch
                    {
                        /* fallback: парсим сырой ответ */
                    }
                }

                return await ParseRawReceiptDataAsync(content, url);
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
            return await Task.Run(() => _receiptParser.Parse(rawData, identifier, false));
        }

        public async Task<Receipt?> GetReceiptByQrCodeAsync(string qrString, DateTime? dateHint = null)
        {
            // Если строка — URL и есть активная конфигурация с UrlPattern (например, iKassa) — fetch URL и парсим текст
            if (!string.IsNullOrWhiteSpace(qrString) &&
                Uri.TryCreate(qrString, UriKind.Absolute, out var uri) &&
                (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                var urlConfig = _configuration.ReceiptConfigurations?
                    .FirstOrDefault(c => c.IsActive &&
                        !string.IsNullOrEmpty(c.UrlPattern) &&
                        Regex.IsMatch(qrString, c.UrlPattern));
                if (urlConfig != null)
                {
                    return await GetReceiptByUrlAsync(qrString);
                }
            }

            // Старая логика: дефолтная QR-конфигурация (ch.info-center.by) → ApiConfig → JSON
            var config = GetDefaultQrConfiguration();
            if (config == null)
            {
                Console.WriteLine("No default QR configuration found");
                return null;
            }
            return await GetReceiptByQrCodeAsync(qrString, config, dateHint);
        }

        public async Task<Receipt?> GetReceiptByQrCodeAsync(string qrString, ReceiptConfiguration config, DateTime? dateHint = null)
        {
            if (config.ApiConfig == null || string.IsNullOrEmpty(config.ApiConfig.Url))
            {
                Console.WriteLine($"Configuration '{config.Name}' has no API config");
                return null;
            }

            try
            {
                var qrParams = ExtractQrParameters(qrString, config.QrCodePattern);
                
                if (dateHint.HasValue && !qrParams.ContainsKey("date"))
                {
                    qrParams["date"] = dateHint.Value.ToString(config.ApiConfig.DateFormat);
                }

                if (!qrParams.ContainsKey("code") && !string.IsNullOrEmpty(qrString))
                {
                    qrParams["code"] = qrString;
                }

                var jsonResponse = await CallApiAsync(config.ApiConfig, qrParams);
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return null;
                }

                if (config.JsonPaths != null)
                {
                    return ParseJsonResponse(jsonResponse, config.JsonPaths, dateHint);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetReceiptByQrCodeAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<Receipt?> GetReceiptByTransactionCodeAsync(DateTime date, string code)
        {
            return await GetReceiptByQrCodeAsync(code, date);
        }

        public ReceiptConfiguration? GetDefaultQrConfiguration()
        {
            return _configuration.ReceiptConfigurations?.Find(c => 
                c.IsActive && c.IsDefault && c.ApiConfig != null);
        }

        public List<ReceiptConfiguration> GetActiveQrConfigurations()
        {
            return _configuration.ReceiptConfigurations?
                .FindAll(c => c.IsActive && c.ApiConfig != null) ?? new List<ReceiptConfiguration>();
        }

        private Dictionary<string, string> ExtractQrParameters(string qrString, string? pattern)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(qrString))
            {
                return result;
            }

            try
            {
                var match = Regex.Match(qrString, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    foreach (var groupName in match.Groups.Keys)
                    {
                        if (groupName != "0" && !string.IsNullOrEmpty(match.Groups[groupName].Value))
                        {
                            result[groupName] = match.Groups[groupName].Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting QR parameters: {ex.Message}");
            }

            return result;
        }

        private async Task<string?> CallApiAsync(ReceiptApiConfig apiConfig, Dictionary<string, string> parameters)
        {
            try
            {
                if (apiConfig.Headers != null)
                {
                    foreach (var header in apiConfig.Headers)
                    {
                        if (!_httpClient.DefaultRequestHeaders.Contains(header.Key))
                        {
                            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }
                }

                var requestParams = new Dictionary<string, string>();
                foreach (var mapping in apiConfig.ParameterMapping)
                {
                    var apiParamName = mapping.Key;
                    var sourceParamName = mapping.Value;

                    if (parameters.TryGetValue(sourceParamName, out var value))
                    {
                        requestParams[apiParamName] = value;
                    }
                }

                HttpResponseMessage response;

                if (apiConfig.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    var queryString = string.Join("&", 
                        requestParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                    var url = apiConfig.Url + (apiConfig.Url.Contains("?") ? "&" : "?") + queryString;
                    response = await _httpClient.GetAsync(url);
                }
                else
                {
                    HttpContent content;
                    if (apiConfig.ContentType.Equals("Json", StringComparison.OrdinalIgnoreCase))
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestParams);
                        content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    }
                    else
                    {
                        content = new MultipartFormDataContent();
                        foreach (var param in requestParams)
                        {
                            ((MultipartFormDataContent)content).Add(new StringContent(param.Value), param.Key);
                        }
                    }

                    response = await _httpClient.PostAsync(apiConfig.Url, content);
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API call error: {ex.Message}");
                return null;
            }
        }

        private Receipt? ParseJsonResponse(string jsonString, ReceiptJsonPaths paths, DateTime? dateHint)
        {
            try
            {
                var json = JObject.Parse(jsonString);

                if (!string.IsNullOrEmpty(paths.StatusPath) && !string.IsNullOrEmpty(paths.SuccessValue))
                {
                    var status = json.SelectToken(paths.StatusPath)?.ToString();
                    if (status != paths.SuccessValue)
                    {
                        Console.WriteLine($"API returned non-success status: {status}");
                        return null;
                    }
                }

                var receipt = new Receipt
                {
                    RawData = jsonString,
                    ReceiptDate = dateHint ?? DateTime.Now,
                    Items = new List<ReceiptItem>()
                };

                if (!string.IsNullOrEmpty(paths.TotalSum))
                {
                    var sumToken = json.SelectToken(paths.TotalSum);
                    if (sumToken != null)
                    {
                        receipt.TotalSum = ParseFloatFromToken(sumToken, paths.DecimalSeparator);
                    }
                }

                if (!string.IsNullOrEmpty(paths.ShopName))
                {
                    receipt.ShopName = json.SelectToken(paths.ShopName)?.ToString();
                }

                if (!string.IsNullOrEmpty(paths.ShopInn))
                {
                    receipt.ShopInn = json.SelectToken(paths.ShopInn)?.ToString();
                }

                if (!string.IsNullOrEmpty(paths.Currency))
                {
                    receipt.Currency = json.SelectToken(paths.Currency)?.ToString();
                }

                if (!string.IsNullOrEmpty(paths.ReceiptDate))
                {
                    var dateStr = json.SelectToken(paths.ReceiptDate)?.ToString();
                    if (!string.IsNullOrEmpty(dateStr))
                    {
                        receipt.ReceiptDateString = dateStr;
                        
                        if (!string.IsNullOrEmpty(paths.DateFormat))
                        {
                            if (DateTime.TryParseExact(dateStr, paths.DateFormat, 
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                            {
                                receipt.ReceiptDate = parsedDate;
                            }
                        }
                        else if (DateTime.TryParse(dateStr, out var parsedDate2))
                        {
                            receipt.ReceiptDate = parsedDate2;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(paths.AddressTemplate))
                {
                    var address = paths.AddressTemplate;
                    var placeholderRegex = new Regex(@"\{(\w+)\}");
                    address = placeholderRegex.Replace(address, match =>
                    {
                        var fieldName = match.Groups[1].Value;
                        var token = json.SelectToken($"$.message.{fieldName}") ?? json.SelectToken($"$.{fieldName}");
                        return token?.ToString() ?? "";
                    });
                    receipt.Address = address.Trim(' ', ',');
                }
                else if (!string.IsNullOrEmpty(paths.Address))
                {
                    receipt.Address = json.SelectToken(paths.Address)?.ToString();
                }

                if (!string.IsNullOrEmpty(paths.ItemsArray))
                {
                    var itemsToken = json.SelectToken(paths.ItemsArray);
                    JArray? itemsArray = null;

                    if (itemsToken is JValue jValue && jValue.Type == JTokenType.String)
                    {
                        try
                        {
                            itemsArray = JArray.Parse(jValue.ToString());
                        }
                        catch
                        {
                            Console.WriteLine("Failed to parse nested JSON items array");
                        }
                    }
                    else if (itemsToken is JArray arr)
                    {
                        itemsArray = arr;
                    }

                    if (itemsArray != null)
                    {
                        foreach (var itemToken in itemsArray)
                        {
                            var item = new ReceiptItem();

                            if (!string.IsNullOrEmpty(paths.ItemName))
                            {
                                item.Name = itemToken[paths.ItemName]?.ToString();
                            }

                            if (!string.IsNullOrEmpty(paths.ItemQuantity))
                            {
                                var qtyToken = itemToken[paths.ItemQuantity];
                                if (qtyToken != null)
                                {
                                    item.Quantity = (double)ParseFloatFromToken(qtyToken, paths.DecimalSeparator);
                                }
                            }

                            if (!string.IsNullOrEmpty(paths.ItemAmount))
                            {
                                var amtToken = itemToken[paths.ItemAmount];
                                if (amtToken != null)
                                {
                                    item.Sum = ParseFloatFromToken(amtToken, paths.DecimalSeparator);
                                }
                            }

                            if (!string.IsNullOrEmpty(paths.ItemPrice))
                            {
                                var priceToken = itemToken[paths.ItemPrice];
                                if (priceToken != null)
                                {
                                    item.Price = ParseFloatFromToken(priceToken, paths.DecimalSeparator);
                                }
                            }
                            else if (item.Quantity > 0 && item.Sum > 0)
                            {
                                item.Price = (float)(item.Sum / item.Quantity);
                            }

                            receipt.Items.Add(item);
                        }
                    }
                }

                return receipt;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON response: {ex.Message}");
                return null;
            }
        }

        private float ParseFloatFromToken(JToken token, string? decimalSeparator)
        {
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.ToObject<float>();
            }

            var strValue = token.ToString();
            if (string.IsNullOrEmpty(strValue)) return 0f;

            if (!string.IsNullOrEmpty(decimalSeparator))
            {
                strValue = strValue.Replace(decimalSeparator, ".");
            }

            if (float.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }

            return 0f;
        }
    }
}
