using System.Collections.Generic;

namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    /// <summary>
    /// Настройки HTTP-запроса к сервису проверки чеков
    /// </summary>
    public class ReceiptApiConfig
    {
        /// <summary>
        /// URL API-эндпоинта
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// HTTP-метод (GET/POST)
        /// </summary>
        public string Method { get; set; } = "POST";
        
        /// <summary>
        /// Тип тела запроса: "FormData" или "Json"
        /// </summary>
        public string ContentType { get; set; } = "FormData";
        
        /// <summary>
        /// Маппинг параметров: ключ = имя параметра в API, значение = имя группы из QrCodePattern
        /// Пример: { "orig_date": "date", "orig_ui": "code" }
        /// </summary>
        public Dictionary<string, string> ParameterMapping { get; set; } = new();
        
        /// <summary>
        /// Формат даты для отправки в API
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        
        /// <summary>
        /// Дополнительные HTTP-заголовки
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }
    }
}
