using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace FR_TCP_Server
{
    internal class ConfigManager
    {
        private static readonly Lazy<ConfigManager> _instance =
            new Lazy<ConfigManager>(() => new ConfigManager());
        public static ConfigManager Instance => _instance.Value;

        // 私有构造函数（防止外部实例化）
        private ConfigManager()
        {
        }

        public string RCSUrl { get; set; } = "http://192.168.2.101:50060/";

    }
}
