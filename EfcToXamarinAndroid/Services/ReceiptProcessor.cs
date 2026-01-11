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

            var newItemsToAdd = new List<DataItem>();

            try
            {
                using (var context = new DataItemContext(DatesRepositorio.DbFullPath))
                {
                    foreach (var receipt in receipts)
                    {
                        Console.WriteLine($"[ReceiptProcessor] Processing receipt: {receipt.ShopName}, Sum: {receipt.TotalSum}, Date: {receipt.ReceiptDate}");

                        // Calculate sum if missing
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

                        // Criteria for matching:
                        // 1. Same Sum (tolerance 0.1)
                        // 2. Not already linked to a receipt
                        // 3. Date match (tolerance +/- 24 hours)

                        // Note: We fetch candidates from DB to ensure we check against persisted data
                        var candidates = await context.Cats
                            .Where(x => Math.Abs(x.Sum - receiptSum) < 0.1)
                            .Where(x => x.Receipt == null)
                            .ToListAsync();

                        var match = candidates
                           .Where(x => Math.Abs((x.Date - receipt.ReceiptDate).TotalHours) < 24)
                           .FirstOrDefault();

                        if (match != null)
                        {
                            Console.WriteLine($"[ReceiptProcessor] MATCH FOUND! ID: {match.Id}");
                            // Link to existing transaction
                            match.Receipt = receipt;
                            
                            // Update description if it was generic/empty
                            if (string.IsNullOrEmpty(match.Descripton) && !string.IsNullOrEmpty(receipt.ShopName))
                            {
                                match.Descripton = receipt.ShopName;
                            }
                            
                            // Prepare receipt for DB addition (as part of the relationship)
                            // The context will track 'receipt' as added because 'match' is tracked and we set the navigation property
                        }
                        else
                        {
                            Console.WriteLine($"[ReceiptProcessor] NO MATCH. Preparing new DataItem for AddDatas.");
                            
                            // Create new DataItem (DTO style) to pass to AddDatas
                            // We do NOT add it to 'context' here.
                            DataItem newItem = new DataItem(OperacionTyps.OPLATA, (DateTime)receipt.ReceiptDate);
                            newItem.Sum = receiptSum;
                            newItem.OperacionTyp = OperacionTyps.OPLATA; 
                            newItem.Descripton = receipt.ShopName ?? "Receipt";
                            newItem.Receipt = receipt;
                            newItem.IsNewDataItem = true;

                            newItemsToAdd.Add(newItem);
                        }
                    }

                    // Save updates to EXISTING linked transactions
                    if (context.ChangeTracker.HasChanges())
                    {
                        Console.WriteLine($"[ReceiptProcessor] Saving updates to existing transactions...");
                        await context.SaveChangesAsync();
                        Console.WriteLine($"[ReceiptProcessor] Updates saved.");
                    }
                }

                // Add NEW transactions via Repository (handles duplicates)
                if (newItemsToAdd.Count > 0)
                {
                    Console.WriteLine($"[ReceiptProcessor] Sending {newItemsToAdd.Count} new items to DatesRepositorio.AddDatas...");
                    await DatesRepositorio.AddDatas(newItemsToAdd);
                    Console.WriteLine($"[ReceiptProcessor] New items processed.");
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
