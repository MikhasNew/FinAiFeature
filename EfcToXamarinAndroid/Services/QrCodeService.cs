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

        private async Task<Receipt?> ParseRawReceiptDataAsync(string rawData, string identifier)
        {
            // The logic here is mostly handled by ReceiptParser which works on the raw content
            // We pass the URL as the identifier to help select the right configuration
            return await Task.Run(() => _receiptParser.Parse(rawData, identifier, false));
        }
    }
}
