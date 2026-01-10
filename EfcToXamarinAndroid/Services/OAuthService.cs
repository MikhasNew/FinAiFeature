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
        private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        // Scope for IMAP access
        private const string Scope = "https://mail.google.com/";

        public string GenerateAuthUrl(string clientId, string redirectUri)
        {
            var p = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "redirect_uri", redirectUri },
                { "response_type", "code" },
                { "scope", Scope },
                { "access_type", "offline" }, // Request refresh token
                { "prompt", "consent" }       // Force consent to ensure refresh token is returned
            };

            var qs = string.Join("&",  p.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            return $"{AuthEndpoint}?{qs}";
        }

        public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string clientId, string clientSecret, string redirectUri)
        {
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

                var response = await httpClient.PostAsync(TokenEndpoint, content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"OAuth Token Error: {json}");
                }

                return JsonConvert.DeserializeObject<OAuthTokenResponse>(json);
            }
        }
        
        // Optional: Refresh token logic
        public async Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret)
        {
             using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });

                var response = await httpClient.PostAsync(TokenEndpoint, content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                     throw new Exception($"OAuth Refresh Error: {json}");
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
