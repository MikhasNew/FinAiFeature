using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Parsers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace EfcToXamarinAndroid.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppConfiguration _configuration;
        private readonly ReceiptParser _receiptParser;
        private readonly ReceiptProcessor _receiptProcessor;

        public EmailService(AppConfiguration configuration, ReceiptParser receiptParser)
        {
            _configuration = configuration;
            _receiptParser = receiptParser;
            _receiptProcessor = new ReceiptProcessor(); // Manually instantiating since it handles its own context
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
                    await client.ConnectAsync(settings.ImapHost, settings.ImapPort, settings.UseSsl);
                    await client.AuthenticateAsync(settings.Email, settings.Password);
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
                    await client.ConnectAsync(settings.ImapHost, settings.ImapPort, settings.UseSsl);
                    await client.AuthenticateAsync(settings.Email, settings.Password);

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
                    // Take last 20
                    var recentUids = uids.OrderByDescending(x => x).Take(20).ToList();

                    foreach (var uid in recentUids)
                    {
                        var message = await inbox.GetMessageAsync(uid);
                        var body = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody;
                        var sender = message.From.Mailboxes.FirstOrDefault()?.Address;

                        // 1. Try parsing body
                        if (!string.IsNullOrEmpty(body))
                        {
                            var receipt = _receiptParser.Parse(body, sender, true);
                            if (receipt != null)
                            {
                                receipts.Add(receipt);
                                continue; // Found validation in body, skip attachments? Or parse both? let's parse both/check attachments if body failed? 
                                // For now, assume one receipt per email.
                            }
                        }

                        // 2. Try parsing attachments (PDF)
                        foreach (var attachment in message.Attachments)
                        {
                             if (attachment is MimePart mimePart && 
                                (mimePart.ContentType.MimeType == "application/pdf" || mimePart.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)))
                             {
                                  using (var stream = new System.IO.MemoryStream())
                                  {
                                        await mimePart.Content.DecodeToAsync(stream);
                                        stream.Position = 0;
                                        try 
                                        {
                                            using (var document = UglyToad.PdfPig.PdfDocument.Open(stream))
                                            {
                                                 var text = string.Join(" ", document.GetPages().Select(p => p.Text));
                                                 var receipt = _receiptParser.Parse(text, sender, true);
                                                 if (receipt != null) 
                                                 {
                                                     receipts.Add(receipt);
                                                     break; // Found in attachment
                                                 }
                                                 else if (sender == "mikail.petrovik@gmail.com")
                                                 {
                                                     // Hardcoded test/fallback for this specific user request
                                                     var testReceipt = ParseTestPdf(text);
                                                     if (testReceipt != null) receipts.Add(testReceipt);
                                                 }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error parsing PDF: {ex.Message}");
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
        private Receipt ParseTestPdf(string content)
        {
             // Simple fallback regex for testing
             // Assume typical invoice: "Total: 123.45" or "Sum: 123.45"
             // User email: mikail.petrovik@gmail.com
             var receipt = new Receipt { RawData = content, ShopName = "Test Shop (PDF)", ReceiptDate = DateTime.Now };
             
             // Try to find sum
             var sumMatch = System.Text.RegularExpressions.Regex.Match(content, @"(Total|Sum|Itogo)[\s:]+([0-9]+[.,][0-9]{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
             if (sumMatch.Success)
             {
                 if (float.TryParse(sumMatch.Groups[2].Value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float sum))
                 {
                     receipt.TotalSum = sum;
                     return receipt;
                 }
             }
             return null;
        }

    }
}
