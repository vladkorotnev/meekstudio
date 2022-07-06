using BinarySerialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM
{
    public static class DSCReader
    {
        public static DSCFile ReadFromFile(string path)
        {
            var s = new BinarySerializer();
            var data = File.ReadAllBytes(path);
            var chart = s.Deserialize<DSCFile>(data);
            return chart;
        }

        public static void WriteToFile(string path, DSCFile data)
        {
            var s = new BinarySerializer();
            using(var f = File.OpenWrite(path))
            {
                f.SetLength(0);
                s.Serialize(f, data);
            }
        }

        public static void WriteToStream(Stream s, DSCFile data)
        {
            var ser = new BinarySerializer();
            ser.Serialize(s, data);
        }
    }
}
