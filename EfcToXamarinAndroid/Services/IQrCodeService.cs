using System.Threading.Tasks;
using System;

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

        /// <summary>
        /// Получает чек по коду транзакции и дате (через ch.info-center.by)
        /// </summary>
        Task<Receipt?> GetReceiptByTransactionCodeAsync(DateTime date, string code);
    }
}
