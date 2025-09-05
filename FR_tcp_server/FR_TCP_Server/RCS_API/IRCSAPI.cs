using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server.RCS_API
{
    
    internal interface IRCSAPI
    {
        public static string? Name { get; }
        public static string? APIpath { get; }
        public static HttpMethod? HttpMethod { get; }
    }
}
