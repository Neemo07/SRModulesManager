using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SR_Manager
{
    public sealed class Config
    {
        private string _filepath;
        
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string value, string filePath);

        public bool Load(string filepath)
        {
            if (!File.Exists(filepath)) return false;

            _filepath = filepath;

            return true;
        }

        public string Read(string key, string section, string def = null)
        {
            var retVal = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, retVal, 1024, _filepath);
            return retVal.ToString();
        }

        public void Write(string key, string value, string section) => WritePrivateProfileString(section, key, value, _filepath);

        public void DeleteKey(string key, string section) => Write(key, null, section);

        public void DeleteSection(string section) => Write(null, null, section);

        public bool KeyExists(string key, string section) => Read(key, section).Length > 0;

        public bool SectionExists(string section) => File.ReadAllLines(_filepath).Any(line => line.StartsWith($"[{section}]"));
    }
}