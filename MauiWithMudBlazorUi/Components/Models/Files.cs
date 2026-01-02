using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCastomUserAuthon.Shared
{
    public class Files
    {
        public string ETag { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public long Size { get; set; } = 0;
    }
}
