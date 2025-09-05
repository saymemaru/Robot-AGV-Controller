using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server.RCS_API
{
    public abstract class APIReponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public HttpRequestMessage OriginalRequest { get; set; }
        public HttpResponseMessage OriginalResponse { get; set; }
    }
}
