using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfcToXamarinAndroid.Core
{
    public class ReceiptItem
    {
        [Key]
        public int Id { get; set; }

        public int ReceiptId { get; set; }
        
        [ForeignKey("ReceiptId")]
        public Receipt? Receipt { get; set; }

        public string? Name { get; set; }
        public double Quantity { get; set; }
        public string? Unit { get; set; } // шт, кг и т.д.
        public float Price { get; set; }
        public float Sum { get; set; }
        public string? Code { get; set; }
    }
}
