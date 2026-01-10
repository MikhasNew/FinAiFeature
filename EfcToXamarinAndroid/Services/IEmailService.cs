using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Подключается к почтовому серверу и ищет чеки
        /// </summary>
        Task<List<Receipt>> FetchReceiptsAsync();
        
        /// <summary>
        /// Проверяет настройки подключения
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Скачивает и обрабатывает (сохраняет/линкует) чеки из почты
        /// </summary>
        Task SyncReceiptsAsync();
    }
}
