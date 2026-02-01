namespace EfcToXamarinAndroid.Core.Configs
{
    public class YandexAuthConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        // Default instance for easy access if needed, 
        // but preferred to use DI or instance from settings
        public static YandexAuthConfig Default { get; private set; } = new YandexAuthConfig();

        public static void Load(string rootPath)
        {
            var secretsPath = System.IO.Path.Combine(rootPath, "yandex_secrets.json");
            if (System.IO.File.Exists(secretsPath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(secretsPath);
                    var data = Newtonsoft.Json.Linq.JObject.Parse(json);
                    var oauth = data["YandexOAuth"];
                    if (oauth != null)
                    {
                        Default.ClientId = oauth["ClientId"]?.ToString() ?? string.Empty;
                        Default.ClientSecret = oauth["ClientSecret"]?.ToString() ?? string.Empty;
                        System.Console.WriteLine("[YandexAuthConfig] Loaded secrets from yandex_secrets.json");
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"[YandexAuthConfig] Error loading secrets: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads configuration from a stream (for MAUI/mobile apps where file paths don't work)
        /// </summary>
        public static void LoadFromStream(System.IO.Stream stream)
        {
            if (stream == null) return;
            
            try
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var data = Newtonsoft.Json.Linq.JObject.Parse(json);
                    var oauth = data["YandexOAuth"];
                    if (oauth != null)
                    {
                        Default.ClientId = oauth["ClientId"]?.ToString() ?? string.Empty;
                        Default.ClientSecret = oauth["ClientSecret"]?.ToString() ?? string.Empty;
                        System.Console.WriteLine("[YandexAuthConfig] Loaded secrets from stream");
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[YandexAuthConfig] Error loading secrets from stream: {ex.Message}");
            }
        }
    }
}
