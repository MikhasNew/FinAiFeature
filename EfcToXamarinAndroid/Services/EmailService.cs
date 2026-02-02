using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Parsers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

namespace EfcToXamarinAndroid.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppConfiguration _configuration;
        private readonly ReceiptParser _receiptParser;
        private readonly ReceiptProcessor _receiptProcessor;
        private readonly OAuthService _oauthService;

        public EmailService(AppConfiguration configuration, ReceiptParser receiptParser, OAuthService oauthService, ReceiptProcessor receiptProcessor)
        {
            _configuration = configuration;
            _receiptParser = receiptParser;
            _oauthService = oauthService;
            _receiptProcessor = receiptProcessor;
        }

        public async Task SyncReceiptsAsync()
        {
            var receipts = await FetchReceiptsAsync();
            if (receipts.Count > 0)
            {
                await _receiptProcessor.ProcessReceiptsAsync(receipts);
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            var settings = _configuration.EmailSettings;

            if (string.IsNullOrEmpty(settings.ImapHost))
                throw new Exception("Не указан IMAP хост.");

            if (settings.ImapPort <= 0)
                throw new Exception("Не указан порт IMAP.");

            if (string.IsNullOrEmpty(settings.Email))
                throw new Exception("Не указан Email адрес.");

            if (settings.UseOAuth)
            {
                if (string.IsNullOrEmpty(settings.AccessToken) && string.IsNullOrEmpty(settings.RefreshToken))
                    throw new Exception("Для OAuth2 требуется авторизация (AccessToken/RefreshToken отсутствует).");
            }
            else
            {
                if (string.IsNullOrEmpty(settings.Password))
                    throw new Exception("Не указан пароль приложения.");
            }

            try
            {
                using (var client = new ImapClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await ConnectAsync(client, settings);
                    await client.DisconnectAsync(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email Connection Test Failed: {ex.Message}");
                throw new Exception($"Ошибка подключения: {ex.Message}");
            }
        }

        public async Task<List<Receipt>> FetchReceiptsAsync()
        {
            var settings = _configuration.EmailSettings;
            var receipts = new List<Receipt>();

            // --- ИСПРАВЛЕННАЯ ПРОВЕРКА ---
            // Для OAuth пароль не нужен, но нужен Email и AccessToken. 
            // Хост проверяем всегда.
            bool isHostValid = !string.IsNullOrEmpty(settings.ImapHost);
            bool isAuthValid = settings.UseOAuth
                ? (!string.IsNullOrEmpty(settings.Email) && !string.IsNullOrEmpty(settings.AccessToken))
                : (!string.IsNullOrEmpty(settings.Email) && !string.IsNullOrEmpty(settings.Password));

            if (!isHostValid || !isAuthValid)
            {
                Console.WriteLine("[EmailService] Validation failed: Check ImapHost, Email or Credentials.");
                return receipts;
            }
            // ----------------------------

            try
            {
                using (var client = new ImapClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    // Здесь ConnectAsync должен уметь работать с OAuth2
                    await ConnectAsync(client, settings);

                    var folderName = string.IsNullOrEmpty(settings.FolderToScan) ? "INBOX" : settings.FolderToScan;
                    var inbox = client.GetFolder(folderName);
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    // Поиск писем (оставляем вашу логику)
                    SearchQuery query = SearchQuery.All;
                    if (!string.IsNullOrEmpty(settings.SenderFilter))
                        query = SearchQuery.FromContains(settings.SenderFilter);

                    if (!string.IsNullOrEmpty(settings.SubjectFilter))
                    {
                        var subjectQuery = SearchQuery.SubjectContains(settings.SubjectFilter);
                        query = query == SearchQuery.All ? subjectQuery : query.And(subjectQuery);
                    }

                    var uids = await inbox.SearchAsync(query);
                    // Берем последние 20 для производительности
                    var recentUids = uids.OrderByDescending(x => x).Take(20).ToList();

                    foreach (var uid in recentUids)
                    {
                        var message = await inbox.GetMessageAsync(uid);
                        var sender = message.From.Mailboxes.FirstOrDefault()?.Address;
                        Console.WriteLine($"[EmailService] Processing email from: {sender}, Subject: {message.Subject}");

                        // 1. Пытаемся найти чек в теле письма
                        var body = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody;
                        if (!string.IsNullOrEmpty(body))
                        {
                            var receipt = _receiptParser.Parse(body, sender, true);
                            if (receipt != null && receipt.TotalSum > 0)
                            {
                                Console.WriteLine($"[EmailService] Found valid receipt in email body. Sum: {receipt.TotalSum}");
                                receipts.Add(receipt);
                                continue; // Пропускаем PDF только если нашли чек с суммой
                            }
                        }

                        // 2. Проверяем ВСЕ части письма на наличие PDF (рекурсивно)
                        var pdfParts = FindPdfParts(message.Body);
                        Console.WriteLine($"[EmailService] Found {pdfParts.Count} PDF parts in message");
                        
                        foreach (var pdfPart in pdfParts)
                        {
                            Console.WriteLine($"[EmailService] Processing PDF: {pdfPart.FileName}");
                            using (var stream = new System.IO.MemoryStream())
                            {
                                await pdfPart.Content.DecodeToAsync(stream);
                                stream.Position = 0;

                                try
                                {
                                    using (var document = UglyToad.PdfPig.PdfDocument.Open(stream))
                                    {
                                        var text = string.Join(" ", document.GetPages().Select(p => p.Text));
                                        
                                        var receipt = _receiptParser.Parse(text, sender, true);

                                        // Fallback is now handled by ReceiptParser Generic Config
                                        if (receipt != null && receipt.TotalSum > 0)
                                        {
                                            Console.WriteLine($"[EmailService] Parsed valid receipt from PDF. Sum: {receipt.TotalSum}, Date: {receipt.ReceiptDate}");
                                            receipts.Add(receipt);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[EmailService] Error parsing PDF {pdfPart.FileName}: {ex.Message}");
                                }
                            }
                        }
                    }
                    await client.DisconnectAsync(true);
                }
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                Console.WriteLine($"[EmailService] Auth failed: {authEx.Message}");
                throw new Exception("Ошибка аутентификации. Проверьте токен или пароль.", authEx);
            }
            catch (MailKit.Net.Imap.ImapProtocolException imapEx)
            {
                Console.WriteLine($"[EmailService] IMAP error: {imapEx.Message}");
                throw new Exception("Ошибка IMAP протокола. Проверьте настройки сервера.", imapEx);
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                Console.WriteLine($"[EmailService] Network error: {socketEx.Message}");
                throw new Exception("Ошибка сети. Проверьте подключение к интернету.", socketEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Unexpected error: {ex}");
                throw new Exception($"Ошибка при получении писем: {ex.Message}", ex);
            }

            return receipts;
        }

        /// <summary>
        /// Рекурсивно ищет все PDF-части в структуре MIME-сообщения
        /// </summary>
        private List<MimePart> FindPdfParts(MimeEntity entity)
        {
            var pdfParts = new List<MimePart>();
            
            if (entity is MimePart part)
            {
                // Проверяем MIME-тип и расширение файла
                bool hasPdfExtension = part.FileName?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) == true;
                bool isPdf = part.ContentType?.MimeType == "application/pdf" ||
                             (part.ContentType?.MimeType == "application/octet-stream" && hasPdfExtension);
                
                if (isPdf)
                {
                    pdfParts.Add(part);
                }
            }
            else if (entity is Multipart multipart)
            {
                // Рекурсивно обходим все части multipart
                foreach (var child in multipart)
                {
                    pdfParts.AddRange(FindPdfParts(child));
                }
            }
            else if (entity is MessagePart messagePart)
            {
                // Обрабатываем вложенные сообщения (forwarded emails)
                if (messagePart.Message?.Body != null)
                {
                    pdfParts.AddRange(FindPdfParts(messagePart.Message.Body));
                }
            }
            
            return pdfParts;
        }


        private async Task ConnectAsync(ImapClient client, EmailSettings settings)
        {
             await client.ConnectAsync(settings.ImapHost, settings.ImapPort, settings.UseSsl);

             if (settings.UseOAuth)
             {
                 await RefreshTokenIfNeededAsync(settings);

                 if (string.IsNullOrEmpty(settings.AccessToken))
                     throw new InvalidOperationException("OAuth selected but AccessToken is empty.");

                 var oauth2 = new SaslMechanismOAuth2(settings.Email, settings.AccessToken);
                 await client.AuthenticateAsync(oauth2);
             }
             else
             {
                 await client.AuthenticateAsync(settings.Email, settings.Password);
             }
        }

        private async Task RefreshTokenIfNeededAsync(EmailSettings settings)
        {
            // Refresh if token is missing or expiring in less than 5 minutes
            bool needsRefresh = string.IsNullOrEmpty(settings.AccessToken) || 
                               (settings.TokenExpiry.HasValue && settings.TokenExpiry.Value < DateTime.Now.AddMinutes(5));

            if (needsRefresh && !string.IsNullOrEmpty(settings.RefreshToken))
            {
                Console.WriteLine($"[EmailService] Access token expired or missing. Refreshing via {settings.OAuthProvider}...");
                try
                {
                    // Определяем провайдера и получаем credentials
                    string clientId;
                    string clientSecret;
                    string providerName = settings.OAuthProvider ?? "Google";

                    if (providerName.Equals("Yandex", StringComparison.OrdinalIgnoreCase))
                    {
                        clientId = !string.IsNullOrEmpty(settings.ClientId) ? settings.ClientId : Configs.YandexAuthConfig.Default.ClientId;
                        clientSecret = !string.IsNullOrEmpty(settings.ClientSecret) ? settings.ClientSecret : Configs.YandexAuthConfig.Default.ClientSecret;
                    }
                    else // Google
                    {
                        clientId = !string.IsNullOrEmpty(settings.ClientId) ? settings.ClientId : Configs.GoogleAuthConfig.Default.ClientId;
                        clientSecret = !string.IsNullOrEmpty(settings.ClientSecret) ? settings.ClientSecret : Configs.GoogleAuthConfig.Default.ClientSecret;
                    }

                    var response = await _oauthService.RefreshTokenAsync(providerName, settings.RefreshToken, clientId, clientSecret);
                    
                    settings.AccessToken = response.AccessToken;
                    settings.TokenExpiry = DateTime.Now.AddSeconds(response.ExpiresIn);
                    
                    // Note: Google might not return a new refresh token if it's still valid
                    if (!string.IsNullOrEmpty(response.RefreshToken))
                    {
                        settings.RefreshToken = response.RefreshToken;
                    }

                    Console.WriteLine($"[EmailService] Token refreshed successfully. New expiry: {settings.TokenExpiry}");
                    
                    // Save configuration
                    ConfigurationManager.ConfigManager.Save();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EmailService] Failed to refresh token: {ex.Message}");
                    // We don't throw here to allow following authentication attempt to fail naturally
                }
            }
        }
    }
}
