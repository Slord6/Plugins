using System;
using System.Collections.Generic;
using System.Text;

namespace PluginBase
{
    public class PluginResultData
    {
        public string Name;
        public byte[] Data;
        public string Base64Data => Convert.ToBase64String(Data);

        public PluginResultData(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }
    }
}
