namespace EfcToXamarinAndroid.Core.Configs
{
    public class GoogleAuthConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        // Default instance for easy access if needed, 
        // but preferred to use DI or instance from settings
        public static GoogleAuthConfig Default { get; private set; } = new GoogleAuthConfig();

        public static void Load(string rootPath)
        {
            var secretsPath = System.IO.Path.Combine(rootPath, "google_secrets.json");
            if (System.IO.Path.Exists(secretsPath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(secretsPath);
                    var data = Newtonsoft.Json.Linq.JObject.Parse(json);
                    var oauth = data["GoogleOAuth"];
                    if (oauth != null)
                    {
                        Default.ClientId = oauth["ClientId"]?.ToString() ?? string.Empty;
                        Default.ClientSecret = oauth["ClientSecret"]?.ToString() ?? string.Empty;
                        System.Console.WriteLine("[GoogleAuthConfig] Loaded secrets from google_secrets.json");
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"[GoogleAuthConfig] Error loading secrets: {ex.Message}");
                }
            }
        }
    }
}
