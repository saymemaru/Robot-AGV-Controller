using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    public class HttpAPISystem
    {
        // 存储注册API字典
        private readonly ConcurrentDictionary<string, IHttpAPIHandler> _httpAPIHandlers =
            new(StringComparer.OrdinalIgnoreCase);

        // 注册API
        public void RegisterAPI(IHttpAPIHandler handler)
        {
            if (_httpAPIHandlers.ContainsKey(handler.Name))
                throw new InvalidOperationException($"API已注册: {handler.Name}");
            _httpAPIHandlers[handler.Name] = handler;
        }

        public bool ProcessAPI(HttpListenerRequest request, HttpListenerResponse response, HttpServerHelper server)
        {
            try
            {
                if (request.Url == null)
                {
                    //url为空
                    return false;
                }
                // 查找api字典调用对应的处理方法
                if (_httpAPIHandlers.TryGetValue(request.Url.AbsolutePath, out IHttpAPIHandler? handler))
                {
                    handler.Execute(request, response, server);
                    return true;
                }
                else
                {
                    //Log($"被请求接口不存在: {request.Url.AbsolutePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                //SendResponse(response, HttpStatusCode.InternalServerError, new { error = "服务器内部错误" });
                //Log($"处理请求时出错: {ex.Message}");
            }
            return false;
        }
    }

    public interface IHttpAPIHandler
    {
        string Name { get; }
        string Path { get; }
        string Description { get; }
        Task Execute(HttpListenerRequest request, HttpListenerResponse response, HttpServerHelper server);
    }

    //RCS推送数据
    // POST
    public class PostHandler : IHttpAPIHandler
    {
        public string Name => "POSTHandler";
        public string Path => "/POST";
        public string Description => "This is a post handler";

        public async Task Execute(HttpListenerRequest request, HttpListenerResponse response, HttpServerHelper server)
        {
            var responseObject = new
            {
                Success = true,
                ErrorCode = 0,
                Msg = "请求成功！"
            };
            //发送响应
            await server.SendResponse(response, HttpStatusCode.OK, responseObject);
        }



    }
    
}
