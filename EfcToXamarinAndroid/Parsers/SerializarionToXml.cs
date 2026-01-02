
using EfcToXamarinAndroid.Core.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace EfcToXamarinAndroid.Core.Parsers
{
    public class SerializarionToXml
    {

        XmlSerializer xmlFormat = new XmlSerializer(typeof(DataItem[]));
        public string SaveToFile(string filename)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    XmlSerializer xmlFormat = new XmlSerializer(typeof(DataItem[]));
                    xmlFormat.Serialize(fs, DatesRepositorio.DataItems.ToArray());
                }
                return filename;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public DataItem[] DeserializeFile(string filename)
        {

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                DataItem[]? dataItems = xmlFormat.Deserialize(fs) as DataItem[];
                return dataItems;
            }
        }
    }
}

