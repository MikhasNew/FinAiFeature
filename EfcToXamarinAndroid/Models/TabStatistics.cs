using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Models
{
    public class TabStatistics
    {
        public int Count { get; set; }
        public float TotalSum { get; set; }
        public float AvgSum { get; set; }
        public float MaxSum { get; set; }
        public float MinSum { get; set; }

        public Dictionary<string, float> TopCategories { get; set; } = [];
        public Dictionary<int, int> MccStats { get; set; } = [];
    }

}
