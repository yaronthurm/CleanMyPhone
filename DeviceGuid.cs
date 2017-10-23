using System.IO;
using System.Linq;

namespace CleanMyPhone
{
    public class DeviceGuid
    {
        public string ID { get; private set; }
        
        public static DeviceGuid LoadFromContent(string content)
        {
            var configValues = content.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None)
                .Select(x => x.Split(new char[] { '=' }, 2))
                .Select(x => new { key = x[0].Trim(), value = x[1].Trim() })
                .ToDictionary(x => x.key, x => x.value);

            var ret = new DeviceGuid();            
            ret.ID = configValues["id"];
            return ret;
        }
    }
}