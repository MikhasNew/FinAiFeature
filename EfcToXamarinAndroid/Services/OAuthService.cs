using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EfcToXamarinAndroid.Core.Services
{
    public class OAuthService
    {
        private readonly Dictionary<string, IOAuthProvider> _providers;

        public OAuthService()
        {
            // Регистрируем всех доступных провайдеров
            _providers = new Dictionary<string, IOAuthProvider>(StringComparer.OrdinalIgnoreCase)
            {
                { "Google", new GoogleOAuthProvider() },
                { "Yandex", new YandexOAuthProvider() }
            };
        }

        /// <summary>
        /// Получить провайдера по имени
        /// </summary>
        public IOAuthProvider GetProvider(string providerName)
        {
            if (_providers.TryGetValue(providerName, out var provider))
            {
                return provider;
            }

            throw new ArgumentException($"Неизвестный OAuth провайдер: {providerName}. Доступные: {string.Join(", ", _providers.Keys)}");
        }

        /// <summary>
        /// Получить список всех доступных провайдеров
        /// </summary>
        public IEnumerable<string> GetAvailableProviders()
        {
            return _providers.Keys;
        }

        public string GenerateAuthUrl(string providerName, string clientId, string redirectUri)
        {
            var provider = GetProvider(providerName);

            var p = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "redirect_uri", redirectUri },
                { "response_type", "code" },
                { "scope", provider.Scope },
                { "access_type", "offline" }, // Request refresh token
                { "prompt", "consent" }       // Force consent to ensure refresh token is returned
            };

            var qs = string.Join("&",  p.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            return $"{provider.AuthEndpoint}?{qs}";
        }

        public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string providerName, string code, string clientId, string clientSecret, string redirectUri)
        {
            var provider = GetProvider(providerName);

            // Debug logging (uncomment for troubleshooting)
            // Console.WriteLine($"[OAuthService] Exchanging code for token:");
            // Console.WriteLine($"  Provider: {providerName}");
            // Console.WriteLine($"  TokenEndpoint: {provider.TokenEndpoint}");
            // Console.WriteLine($"  ClientId: {clientId}");
            // Console.WriteLine($"  RedirectUri: {redirectUri}");
            // Console.WriteLine($"  Code (first 20 chars): {code?.Substring(0, Math.Min(20, code?.Length ?? 0))}...");

            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });

                var response = await httpClient.PostAsync(provider.TokenEndpoint, content);
                var json = await response.Content.ReadAsStringAsync();

                // Console.WriteLine($"[OAuthService] Response status: {response.StatusCode}");
                // Console.WriteLine($"[OAuthService] Response body: {json}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"OAuth Token Error ({providerName}): {json}");
                }

                return JsonConvert.DeserializeObject<OAuthTokenResponse>(json);
            }
        }
        
        // Optional: Refresh token logic
        public async Task<OAuthTokenResponse> RefreshTokenAsync(string providerName, string refreshToken, string clientId, string clientSecret)
        {
            var provider = GetProvider(providerName);

             using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });

                var response = await httpClient.PostAsync(provider.TokenEndpoint, content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                     throw new Exception($"OAuth Refresh Error ({providerName}): {json}");
                }

                return JsonConvert.DeserializeObject<OAuthTokenResponse>(json);
            }
        }
    }

    public class OAuthTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
