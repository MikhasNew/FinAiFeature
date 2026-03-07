using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace EfcToXamarinAndroid.Core.Configs.ManagerCore
{
    public sealed class ConfigurationManager
    {
        /// <summary>
        /// holds a reference to the single created instance, if any.
        /// </summary>
        private static readonly Lazy<ConfigurationManager> lazy = new Lazy<ConfigurationManager>(() => new ConfigurationManager());

        public static event EventHandler ConfigurationManagerChanged;
        /// <summary>
        /// Getting reference to the single created instance, creating one if necessary.
        /// </summary>
        public static ConfigurationManager ConfigManager { get; } = lazy.Value;
        private string localFileName = System.IO.Path.
            Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ConfigBank.json");
        public AppConfiguration BankConfigurationFromJson { get; set; }
        private ConfigurationManager()
        {
            BankConfigurationFromJson = this.Read();
        }

        private AppConfiguration Read()
        {
            AppConfiguration configs;

            if (!File.Exists(localFileName))
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "EfcToXamarinAndroid.Core.Configs.ConfigBank.json";
                string jsonFile = "";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    jsonFile = reader.ReadToEnd();
                }
                configs = JsonConvert.DeserializeObject<AppConfiguration>(jsonFile);
                Write(configs);
            }
            else
            {
                string jsonFile = "";
                using (StreamReader reader = new StreamReader(localFileName, Encoding.UTF8))
                {
                    jsonFile = reader.ReadToEnd();
                }
                configs = JsonConvert.DeserializeObject<AppConfiguration>(jsonFile);

                // Ensure default QR configuration exists (migration logic)
                var needsSave = false;
                if (configs.ReceiptConfigurations == null)
                {
                    configs.ReceiptConfigurations = new System.Collections.Generic.List<ReceiptConfiguration>();
                    needsSave = true;
                }

                if (!configs.ReceiptConfigurations.Exists(c => c.Name == "Belarus (ch.info-center.by)"))
                {
                    // Load default configs from resource to get the new one
                    var assembly = Assembly.GetExecutingAssembly();
                    string resourceName = "EfcToXamarinAndroid.Core.Configs.ConfigBank.json";
                    try 
                    {
                         using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                         using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                         {
                             var resourceJson = reader.ReadToEnd();
                             var resourceConfigs = JsonConvert.DeserializeObject<AppConfiguration>(resourceJson);
                             var defaultConfig = resourceConfigs?.ReceiptConfigurations?.Find(c => c.Name == "Belarus (ch.info-center.by)");
                             
                             if (defaultConfig != null)
                             {
                                 configs.ReceiptConfigurations.Insert(0, defaultConfig);
                                 needsSave = true;
                             }
                         }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error migrating config: {ex.Message}");
                    }
                }

                if (needsSave)
                {
                    Write(configs);
                }
            }

            return configs;
        }
        private void Write(AppConfiguration appConfiguration)
        {
            var serializer = new JsonSerializer();
            var directory = Path.GetDirectoryName(localFileName);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var sw = new StreamWriter(localFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, appConfiguration);
            }
        }
        public void Save()
        {
            Write(BankConfigurationFromJson);
            ConfigurationManagerChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefault()
        {
            if (File.Exists(localFileName))
            {
                File.Delete(localFileName);
            }
            BankConfigurationFromJson = Read();
            ConfigurationManagerChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}