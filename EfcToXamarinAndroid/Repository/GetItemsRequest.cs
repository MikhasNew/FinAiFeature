using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Repository
{
    public class GetItemsRequest
    {
        public OperacionTyps Typ { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
    }
}
