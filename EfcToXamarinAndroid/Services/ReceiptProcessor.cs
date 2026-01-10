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
            if (receipts == null || !receipts.Any()) return;

            using (var context = new DataItemContext(DatesRepositorio.DbFullPath))
            {
                foreach (var receipt in receipts)
                {
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

                    if (receiptSum == 0) continue; // Skip empty receipts

                    var existingItem = await context.Cats
                        .Where(x => Math.Abs(x.Sum - receiptSum) < 0.1) // Float comparison
                        .Where(x => x.Receipt == null)
                        .ToListAsync(); // Fetch candidates

                     var match = existingItem
                        .Where(x => Math.Abs((x.Date - receipt.ReceiptDate).TotalHours) < 24)
                        .FirstOrDefault();

                    if (match != null)
                    {
                        // Update existing
                        match.Receipt = receipt;
                        if (string.IsNullOrEmpty(match.Title) && !string.IsNullOrEmpty(receipt.ShopName))
                        {
                            match.Title = receipt.ShopName;
                        }
                       // context.Receipts.Add(receipt); // EF Core might handle this via navigation property fixup?
                       // Better to just set the navigation property
                       // match.Receipt = receipt is enough if receipt is satisfied.
                       // However, receipt is new.
                       context.Receipts.Add(receipt); 
                    }
                    else
                    {
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
                         // context.Receipts.Add(receipt); // generic Add on DBSet is usually enough
                    }
                }

                await context.SaveChangesAsync();
            }
            
            // Trigger UI update if needed via repository static events?
            // DatesRepositorio currently triggers events when lists change.
            // Since we used a separate context, the static lists in DatesRepositorio are stale.
            // We should call DatesRepositorio.SetDatasFromDB() or manually add?
            // Calling SetDatasFromDB() is safest to refresh.
            await DatesRepositorio.SetDatasFromDB();
        }
    }
}
