using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfcToXamarinAndroid.Core
{
    public class Receipt
    {
        [Key]
        public int Id { get; set; }

        public int DataItemId { get; set; }
        
        [ForeignKey("DataItemId")]
        public DataItem? DataItem { get; set; }

        public string? ShopName { get; set; }
        public string? ShopInn { get; set; }
        public string? Address { get; set; }
        public DateTime ReceiptDate { get; set; }
        public float TotalSum { get; set; }
        public string? RawData { get; set; }

        public List<ReceiptItem>? Items { get; set; }
    }
}
