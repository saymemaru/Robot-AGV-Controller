using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    internal interface IHttpAPIHandler
    {
        string Name { get; }
        string Path { get; }
        string Description { get; }
        Task Execute(HttpListenerRequest request, HttpListenerResponse response);
    }

    public class PostHandler : IHttpAPIHandler
    {
        public string Name => "PostHandler";
        public string Path => "/post";
        public string Description => "This is a post handler";

        public async Task Execute(HttpListenerRequest request, HttpListenerResponse response)
        {

        }



    }
    
}
