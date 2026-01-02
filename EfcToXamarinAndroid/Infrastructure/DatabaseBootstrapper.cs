using System;
using System.Collections.Generic;
using System.Text;

namespace EfcToXamarinAndroid.Core.Infrastructure
{
    public static class DatabaseBootstrapper
    {
        static DatabaseBootstrapper()
        {
            SQLitePCL.Batteries_V2.Init();
        }
        public static void EnsureInitialized() { /* триггер статик-ctor */ }
    }
}
