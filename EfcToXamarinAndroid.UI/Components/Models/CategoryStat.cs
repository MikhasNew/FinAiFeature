using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.UI.Components.Models
{
    public class CategoryStat
    {
        public string Name { get; set; } = string.Empty;
        public float Amount { get; set; }
        public double Percentage { get; set; }
        public bool IsExpense { get; set; }
    }
}

