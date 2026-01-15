namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    public class ReceiptConfiguration
    {
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Активна ли эта конфигурация
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Является ли конфигурацией по умолчанию для QR-кодов
        /// </summary>
        public bool IsDefault { get; set; } = false;
        
        // --- Идентификация источника ---
        
        /// <summary>
        /// Email отправителя (для email-чеков)
        /// </summary>
        public string? SenderEmail { get; set; }
        
        /// <summary>
        /// Regex-паттерн для URL (для web-чеков)
        /// </summary>
        public string? UrlPattern { get; set; }
        
        /// <summary>
        /// Regex-паттерн для QR-строки с именованными группами.
        /// Группы используются для извлечения параметров (date, code, sum и т.д.)
        /// </summary>
        public string? QrCodePattern { get; set; }
        
        // --- Настройки API ---
        
        /// <summary>
        /// Конфигурация HTTP-запроса к сервису проверки чеков
        /// </summary>
        public ReceiptApiConfig? ApiConfig { get; set; }
        
        // --- Парсинг контента ---
        
        /// <summary>
        /// Regex-шаблоны для парсинга текста/HTML (существующий функционал)
        /// </summary>
        public ReceiptParseRegex? ParseRegex { get; set; }
        
        /// <summary>
        /// JSONPath пути для парсинга JSON-ответа API
        /// </summary>
        public ReceiptJsonPaths? JsonPaths { get; set; }
    }
}

