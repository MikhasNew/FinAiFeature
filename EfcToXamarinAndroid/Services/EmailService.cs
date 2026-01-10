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

        public EmailService(AppConfiguration configuration, ReceiptParser receiptParser, OAuthService oauthService)
        {
            _configuration = configuration;
            _receiptParser = receiptParser;
            _oauthService = oauthService;
            _receiptProcessor = new ReceiptProcessor(); 
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
            if (string.IsNullOrEmpty(settings.ImapHost) || string.IsNullOrEmpty(settings.Email) || string.IsNullOrEmpty(settings.Password))
                return false;

            try
            {
                using (var client = new ImapClient())
                {
                    await ConnectAsync(client, settings);
                    await client.DisconnectAsync(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email Connection Test Failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Receipt>> FetchReceiptsAsync()
        {
            var settings = _configuration.EmailSettings;
            var receipts = new List<Receipt>();

            if (string.IsNullOrEmpty(settings.ImapHost) || string.IsNullOrEmpty(settings.Email) || string.IsNullOrEmpty(settings.Password))
                return receipts;

            try
            {
                using (var client = new ImapClient())
                {
                    await ConnectAsync(client, settings);

                    var folderName = string.IsNullOrEmpty(settings.FolderToScan) ? "INBOX" : settings.FolderToScan;
                    var inbox = client.GetFolder(folderName);
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    // Build search query
                    SearchQuery query = SearchQuery.All;
                    if (!string.IsNullOrEmpty(settings.SenderFilter))
                    {
                        query = SearchQuery.FromContains(settings.SenderFilter);
                    }
                    if (!string.IsNullOrEmpty(settings.SubjectFilter)) // Assuming SubjectFilter exists in settings or we use it here
                    {
                        var subjectQuery = SearchQuery.SubjectContains(settings.SubjectFilter);
                        query = query == SearchQuery.All ? subjectQuery : query.And(subjectQuery);
                    }

                    // Limit to recent emails to avoid full scan every time? 
                    // For now, let's fetch last 20 messages for demo or use logic to find unseen
                    // query = query.And(SearchQuery.NotSeen); // Optional: only read unread

                    var uids = await inbox.SearchAsync(query);
                    Console.WriteLine($"[EmailService] Search returned {uids.Count} emails.");
                    // Take last 20
                    var recentUids = uids.OrderByDescending(x => x).Take(20).ToList();

                    foreach (var uid in recentUids)
                    {
                        var message = await inbox.GetMessageAsync(uid);
                        var body = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody;
                        var sender = message.From.Mailboxes.FirstOrDefault()?.Address;

                        Console.WriteLine($"[EmailService] Processing email from: {sender}, Subject: {message.Subject}");

                        // 1. Try parsing body
                        if (!string.IsNullOrEmpty(body))
                        {
                            var receipt = _receiptParser.Parse(body, sender, true);
                            if (receipt != null)
                            {
                                Console.WriteLine($"[EmailService] Found receipt in body. Sum: {receipt.TotalSum}");
                                receipts.Add(receipt);
                                continue; 
                            }
                        }

                        // 2. Try parsing attachments (PDF)
                        if (message.Attachments.Any())
                        {
                            Console.WriteLine($"[EmailService] Checking {message.Attachments.Count()} attachments...");
                        }
                        
                        foreach (var attachment in message.Attachments)
                        {
                             if (attachment is MimePart mimePart && 
                                (mimePart.ContentType.MimeType == "application/pdf" || mimePart.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)))
                             {
                                  Console.WriteLine($"[EmailService] Found PDF attachment: {mimePart.FileName}");
                                  using (var stream = new System.IO.MemoryStream())
                                  {
                                        await mimePart.Content.DecodeToAsync(stream);
                                        stream.Position = 0;
                                        try 
                                        {
                                            using (var document = UglyToad.PdfPig.PdfDocument.Open(stream))
                                            {
                                                 var text = string.Join(" ", document.GetPages().Select(p => p.Text));
                                                 Console.WriteLine($"[EmailService] Extracted text length: {text.Length}");
                                                 Console.WriteLine($"[EmailService] TEXT PREVIEW: {text.Substring(0, Math.Min(text.Length, 500))}..."); // Log first 500 chars
                                                 
                                                 var receipt = _receiptParser.Parse(text, sender, true);
                                                 if (receipt != null) 
                                                 {
                                                     Console.WriteLine($"[EmailService] Parsed receipt from PDF. Sum: {receipt.TotalSum}");
                                                     receipts.Add(receipt);
                                                     break; // Found in attachment
                                                 }
                                                 else if (sender == "mikail.petrovik@gmail.com")
                                                 {
                                                     // Hardcoded test/fallback
                                                     Console.WriteLine($"[EmailService] Using fallback parser for {sender}...");
                                                     var testReceipt = ParseTestPdf(text);
                                                     if (testReceipt != null) 
                                                     {
                                                         Console.WriteLine($"[EmailService] Fallback parsed receipt. Sum: {testReceipt.TotalSum}");
                                                         receipts.Add(testReceipt);
                                                     }
                                                     else
                                                     {
                                                         Console.WriteLine($"[EmailService] Fallback extraction failed.");
                                                     }
                                                 }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"[EmailService] Error parsing PDF: {ex.Message}");
                                        }
                                  }
                             }
                        }
                    }

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching emails: {ex.Message}");
            }

            return receipts;
        }
        private Receipt ParseTestPdf(string text)
        {
            var receipt = new Receipt { RawData = text, ReceiptDate = DateTime.Now };

            // Helper to get value
            string GetValue(string input, string pattern, System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.None)
            {
                var match = System.Text.RegularExpressions.Regex.Match(input, pattern, options);
                return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
            }

            // 1. Reg Number
            receipt.RegNumber = GetValue(text, @"Рег\. номер:\s*(\d+)");

            // 2. Date
            string dateStr = GetValue(text, @"Дата:\s*([\d\.\s:]+)");
            if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
                receipt.ReceiptDate = date;

            // 3. Card
            receipt.CardNumber = GetRegexValue(text, @"Карта:\s*([\d\*]+)");

            // 4. ERIP
            receipt.EripPayerNumber = GetValue(text, @"Номер плательщика ЕРИП:\s*(\d+)");

            // 5. Receiver
            receipt.ShopName = GetValue(text, @"Получатель платежа:\s*(.+?)(?=\r?\n|УНП)", System.Text.RegularExpressions.RegexOptions.Singleline).Trim();

            // 6. Order Number
            receipt.OrderNumber = GetValue(text, @"Заказ №(\d+)");

            // 7. Subject (Product/Service)
            receipt.Subject = GetValue(text, @"Оплата билетов на:\s*(.+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrEmpty(receipt.Subject))
                receipt.Subject = GetValue(text, @"Товар:\s*(.+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrEmpty(receipt.Subject))
                receipt.Subject = GetValue(text, @"Услуга:\s*(.+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

            // 8. Sum
            string amountStr = GetValue(text, @"Сумма всего\s*:\s*([\d\.,]+)\s*(BYN|руб)");
            if (float.TryParse(amountStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                receipt.TotalSum = amount;
                receipt.Currency = "BYN";
            }

            // If we didn't find sum with previous regex, try the general one as last resort
            if (receipt.TotalSum == 0)
            {
                 var sumMatch = System.Text.RegularExpressions.Regex.Match(text, @"(Total|Sum|Itogo|Итого|Всего|Сумма)[\s:.]*([0-9]+[.,][0-9]{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                 if (sumMatch.Success && float.TryParse(sumMatch.Groups[2].Value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float sum))
                 {
                     receipt.TotalSum = sum;
                 }
            }

            return receipt.TotalSum > 0 ? receipt : null;
        }

        private string GetRegexValue(string input, string pattern)
        {
            var match = System.Text.RegularExpressions.Regex.Match(input, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : match.Value.Trim();
            }
            return string.Empty;
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
                Console.WriteLine("[EmailService] Access token expired or missing. Refreshing...");
                try
                {
                    // Prefer settings if entered manually, else use loaded from file
                    var clientId = !string.IsNullOrEmpty(settings.ClientId) ? settings.ClientId : Configs.GoogleAuthConfig.Default.ClientId;
                    var clientSecret = !string.IsNullOrEmpty(settings.ClientSecret) ? settings.ClientSecret : Configs.GoogleAuthConfig.Default.ClientSecret;

                    var response = await _oauthService.RefreshTokenAsync(settings.RefreshToken, clientId, clientSecret);
                    
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
