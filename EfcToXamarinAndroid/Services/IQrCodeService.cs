using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Services
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Получает данные чека по URL из QR-кода
        /// </summary>
        Task<Receipt?> GetReceiptByUrlAsync(string url);
        
        /// <summary>
        /// Парсит данные чека из сырого HTML или JSON
        /// </summary>
        Task<Receipt?> ParseRawReceiptDataAsync(string rawData);
    }
}
