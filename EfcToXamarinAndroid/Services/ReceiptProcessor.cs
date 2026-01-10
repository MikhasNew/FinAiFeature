using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EfcToXamarinAndroid.Core.Repository;

namespace EfcToXamarinAndroid.Core.Services
{
    public class ReceiptProcessor
    {
        public async Task ProcessReceiptsAsync(List<Receipt> receipts)
        {
            Console.WriteLine($"[ReceiptProcessor] Starting processing {receipts?.Count ?? 0} receipts...");
            if (receipts == null || !receipts.Any()) return;

            try
            {
                using (var context = new DataItemContext(DatesRepositorio.DbFullPath))
                {
                    foreach (var receipt in receipts)
                    {
                        Console.WriteLine($"[ReceiptProcessor] Processing receipt: {receipt.ShopName}, Sum: {receipt.TotalSum}, Date: {receipt.ReceiptDate}");
                        
                        // Try to find existing transaction
                        // Criteria: 
                        // 1. Same Sum (tolerance 0.01)
                        // 2. Date match (tolerance +/- 24 hours to be safe)
                        // 3. Not already linked to a receipt
                        
                        var receiptSum = receipt.TotalSum;
                        if (receiptSum == 0 && receipt.Items != null && receipt.Items.Any())
                        {
                            receiptSum = receipt.Items.Sum(x => x.Sum);
                            receipt.TotalSum = receiptSum;
                        }

                        if (receiptSum == 0) 
                        {
                             Console.WriteLine($"[ReceiptProcessor] Skipping empty sum.");
                             continue; 
                        }

                        var existingItem = await context.Cats
                            .Where(x => Math.Abs(x.Sum - receiptSum) < 0.1) // Float comparison
                            .Where(x => x.Receipt == null)
                            .ToListAsync(); // Fetch candidates

                         Console.WriteLine($"[ReceiptProcessor] Found {existingItem.Count} candidates by sum.");

                         var match = existingItem
                            .Where(x => Math.Abs((x.Date - receipt.ReceiptDate).TotalHours) < 24)
                            .FirstOrDefault();

                        if (match != null)
                        {
                            Console.WriteLine($"[ReceiptProcessor] MATCH FOUND! ID: {match.Id}");
                            // Update existing
                            match.Receipt = receipt;
                            if (string.IsNullOrEmpty(match.Title) && !string.IsNullOrEmpty(receipt.ShopName))
                            {
                                match.Title = receipt.ShopName;
                            }
                           context.Receipts.Add(receipt); 
                        }
                        else
                        {
                            Console.WriteLine($"[ReceiptProcessor] NO MATCH. Creating new DataItem.");
                            // Create new
                            var newItem = new DataItem
                            {
                                Date = receipt.ReceiptDate,
                                Sum = receiptSum,
                                OperacionTyp = OperacionTyps.OPLATA, // Default to expense
                                Title = receipt.ShopName ?? "Receipt",
                                Descripton = "Added from Receipt",
                                Receipt = receipt,
                                IsNewDataItem = true
                            };
                             context.Cats.Add(newItem);
                        }
                    }

                    Console.WriteLine($"[ReceiptProcessor] Saving changes to DB...");
                    await context.SaveChangesAsync();
                    Console.WriteLine($"[ReceiptProcessor] Saved successfully.");
                }
                
                // Trigger UI update
                await DatesRepositorio.SetDatasFromDB();
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"[ReceiptProcessor] CRITICAL ERROR: {ex}");
                 throw;
            }
        }
    }
}
