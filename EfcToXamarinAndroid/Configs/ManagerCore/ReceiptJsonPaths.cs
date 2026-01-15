namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    /// <summary>
    /// JSONPath пути для извлечения данных из JSON-ответа API.
    /// Использует формат JSONPath (поддерживается Newtonsoft.Json).
    /// </summary>
    public class ReceiptJsonPaths
    {
        /// <summary>
        /// Путь к полю статуса, например: "$.status"
        /// </summary>
        public string? StatusPath { get; set; }
        
        /// <summary>
        /// Значение успешного статуса, например: "success"
        /// </summary>
        public string? SuccessValue { get; set; }
        
        // --- Основные поля чека ---
        
        /// <summary>
        /// Путь к общей сумме, например: "$.message.total_amount"
        /// </summary>
        public string? TotalSum { get; set; }
        
        /// <summary>
        /// Путь к названию магазина, например: "$.message.name_to"
        /// </summary>
        public string? ShopName { get; set; }
        
        /// <summary>
        /// Путь к ИНН/УНП, например: "$.message.unp"
        /// </summary>
        public string? ShopInn { get; set; }
        
        /// <summary>
        /// Путь к дате чека, например: "$.message.issued_at"
        /// </summary>
        public string? ReceiptDate { get; set; }
        
        /// <summary>
        /// Путь к валюте, например: "$.message.currency"
        /// </summary>
        public string? Currency { get; set; }
        
        /// <summary>
        /// Путь к адресу (может быть составным из нескольких полей)
        /// </summary>
        public string? Address { get; set; }
        
        /// <summary>
        /// Шаблон для составного адреса, использует плейсхолдеры {field}
        /// Например: "{name_np}, {street_to}, {house_to}"
        /// </summary>
        public string? AddressTemplate { get; set; }
        
        // --- Позиции чека ---
        
        /// <summary>
        /// Путь к массиву позиций, например: "$.message.positions"
        /// Если значение — строка JSON, будет автоматически распарсено
        /// </summary>
        public string? ItemsArray { get; set; }
        
        /// <summary>
        /// Имя поля названия товара внутри элемента массива
        /// </summary>
        public string? ItemName { get; set; }
        
        /// <summary>
        /// Имя поля количества внутри элемента массива
        /// </summary>
        public string? ItemQuantity { get; set; }
        
        /// <summary>
        /// Имя поля суммы позиции внутри элемента массива
        /// </summary>
        public string? ItemAmount { get; set; }
        
        /// <summary>
        /// Имя поля цены за единицу внутри элемента массива
        /// </summary>
        public string? ItemPrice { get; set; }
        
        /// <summary>
        /// Формат даты в ответе API (для парсинга), например: "dd/MM/yyyy, HH:mm:ss"
        /// </summary>
        public string? DateFormat { get; set; }
    }
}
