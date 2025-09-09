using FR_TCP_Server.RCS_API;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;  
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static FR_TCP_Server.HttpClientHelper;

namespace FR_TCP_Server

{
    public class HttpServerHelper
    {
        //http侦听器
        private readonly HttpListener _listener;
        //api路由处理字典
        private readonly Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>> _routeHandlers;
        //跳出侦听循环标识符
        private CancellationTokenSource? _cts;

        //服务器运行状态
        public bool isRunning { get;private set; } = false;

        // 日志事件
        public event Action<string>? LogMessage;  

        public HttpServerHelper()
        {
            _listener = new HttpListener();

            // 注册api
            _routeHandlers = new()
            {
                ["/api/data"] = async (req, res) => await HandleDataRequest(req, res),
                ["/POST"] = async (req, res) => await HandlePostRequest(req, res),
                ["/api/pause"] = async (req, res) => await HandlePauseRequest(req, res),
                ["/api/work"] = async (req, res) => await HandleRobotRequest(req, res),
                // 其他接口...
            };
        }


        public async Task Start(string url)
        {
            //设置侦听url
            string _prefix = url;
            _listener.Prefixes.Add(_prefix);

            _cts = new CancellationTokenSource();

            try
            {
                _listener.Start();
                isRunning = true;
                Log($"http服务器 {url} 侦听中");

                // 开始监听请求
                await ListenForRequests(_cts.Token);
            }
            catch (Exception ex)
            {
                isRunning = false;
                Log($"启动服务器时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 循环侦听请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ListenForRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 异步获取上下文
                    var context = await _listener.GetContextAsync().ConfigureAwait(false);

                    // 处理请求（不阻塞主循环）
                    _ = Task.Run(() => ProcessRequest(context), cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    break; // 监听器已关闭，退出循环
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Log($"监听请求时出错: {ex.Message}");
                    }
                }
            }
        }


        private async Task ProcessRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest? request = context.Request;
                HttpListenerResponse? response = context.Response;

                //Console.WriteLine($"收到请求: {request.HttpMethod} {request.Url}");
                Log($"收到请求: {request.HttpMethod} {request.Url}");

                // 读取请求内容
                string? requestBody = null;
                if (request.HasEntityBody)
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                }

                // 初级判断，不应加入复杂逻辑
                // 将JSON字符串转换为RequestData（json）对象（待实现）
                if (requestBody != null)
                {
                    //尝试收集并注册发出请求的AGV信息
                    AGVInfoManager.Instance.TryRegisterAGV(requestBody, out var agvInfo);

                    //调试
                    Log($"{JsonConvert.SerializeObject(agvInfo)}");//当前agv信息
                    Log($"{JsonConvert.SerializeObject(AGVInfoManager.Instance.AGVInfoDic)}");//所有已注册的agv信息
                    Log($"请求内容: {requestBody}");
                }

                // 处理不同的HTTP方法（只支持json）
                switch (request.HttpMethod)
                {
                    case "GET":
                        //Get方法
                        break;

                    case "POST":
                        //rcs推送数据/请求数据，目前无论是否接收到请求内容都返回成功
                        ApiRequestHandler(request, response);
                        break;

                    default:
                        //"不支持的请求方法";
                        response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        break;
                }
            }
            catch (Exception ex)
            {
                //向客户端发送错误响应
                //var response = context.Response;
                //SendResponse(response, HttpStatusCode.InternalServerError, new { error = "服务器内部错误" });

                Log($"处理请求时出错: {ex.Message}");
            }
        }


        /// <summary>
        /// 发送响应(json)
        /// </summary>
        /// <param name="response"></param>
        /// <param name="statusCode"></param>
        /// <param name="responseObject"></param>
        /// <returns></returns>
        public async Task SendResponse(HttpListenerResponse response, HttpStatusCode statusCode, object responseObject)
        {
            try
            {
                //json序列化，UTF8
                string jsonResponse = JsonConvert.SerializeObject(responseObject);
                var buffer = Encoding.UTF8.GetBytes(jsonResponse);

                //待办（查看响应头正规写法）
                //设置响应头
                //response.Headers.Add("Access-Control-Allow-Origin", "*"); // 允许所有来源

                // 设置返回内容的长度
                response.ContentLength64 = buffer.Length;
                // 设置HTTP状态码，200表示成功
                response.StatusCode = (int)statusCode;
                // 设置状态描述（可选，通常StatusCode设置就够了）
                if (statusCode == HttpStatusCode.OK)
                {
                    response.StatusDescription = "OK";
                }
                // 设置内容类型为 application/json
                response.ContentType = "application/json; charset=utf-8";

                // 发送响应
                //同步发送响应写法
                /*using (Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }*/

                //异步发送响应写法
                await using (var output = response.OutputStream)
                {
                    await output.WriteAsync(buffer, 0, buffer.Length);
                }
                Log($"已发送响应: {jsonResponse}");
            }
            catch (Exception ex)
            {
                Log($"发送响应时出错: {ex.Message}");
            }

        }

        private void Log(string message)
        {
            if (LogMessage != null)
            {
                // 检查是否需要跨线程调用
                if (LogMessage.Target is Control control && control.InvokeRequired)
                {
                    control.Invoke(new Action(() => LogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}")));
                }
                else
                {
                    LogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
                }
            }
        }

        /// <summary>
        /// 停止侦听
        /// </summary>
        private void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            isRunning = false;

            Log("已停止侦听。");
        }

        //待办（计划修改为类似commandHandler的接口）
        // 处理API请求
        private void ApiRequestHandler(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (request.Url == null)
                {
                    Log($"请求URL为空");
                    return;
                }
                // 查找api字典调用对应的处理方法
                if (_routeHandlers.TryGetValue(request.Url.AbsolutePath, out var handler))
                {
                    handler(request, response);
                }
                else
                {
                    Log($"被请求接口不存在: {request.Url.AbsolutePath}");
                }
            }
            catch (Exception ex)
            {
                //SendResponse(response, HttpStatusCode.InternalServerError, new { error = "服务器内部错误" });
                Log($"处理请求时出错: {ex.Message}");
            }
        }

        //RCS推送数据
        // POST
        private async Task HandlePostRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            var responseObject = new
            {
                Success = true,
                ErrorCode = 0,
                Msg = "请求成功！"
            };
            //发送响应
            await SendResponse(response, HttpStatusCode.OK, responseObject);
        }
        
        // 处理基础数据请求
        // api/data
        private async Task HandleDataRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            // 获取查询参数
            /*var queryParams = new Dictionary<string, string>();
            foreach (string key in request.QueryString.Keys)
            {
                queryParams[key] = request.QueryString[key];
            }*/

            // 构建响应数据
            var responseData = new
            {
                message = "数据接口响应",
                //timestamp = DateTime.Now,
                name = "哈基咪",
            };

            // 发送响应
            await SendResponse(response, HttpStatusCode.OK, responseData);
        }

        //待办（agvcode,mapcode魔法数）
        //处理暂停请求（暂停当前车任务）
        // api/pause
        private async Task HandlePauseRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string RCSUrl = ConfigManager.Instance.RCSUrl;
            //获取任务编码
            RequestResult taskCodeResult =
            await HttpClientHelper.Instance.ExecuteAsync(
                RCSUrl + RequestGetTaskByAgvCodeByRCS.APIpath,
                RequestGetTaskByAgvCodeByRCS.HttpMethod,
                JsonConvert.SerializeObject(RequestGetTaskByAgvCodeByRCS.
                    CreateRequest(
                        "1"))//agv编码
                );
            if (taskCodeResult.Success == true)
            {
                //接收到的是json字符串需要反序列化
                Log($"获得任务编码: {taskCodeResult.Content}");
            }
            else
            {
                Log($"获取任务编码失败 Error: {taskCodeResult.Content}");
            }

            string? taskCode = JsonConvert.DeserializeObject<string>(taskCodeResult.Content);

            //暂停任务
            Log($"{JsonConvert.SerializeObject(RequestChangeTaskStateByTaskByRCS.CreateRequest("2", taskCode))}");

            RequestResult pauseResult =
            await HttpClientHelper.Instance.ExecuteAsync(
                RCSUrl + RequestChangeTaskStateByTaskByRCS.APIpath,
                RequestChangeTaskStateByTaskByRCS.HttpMethod,
                JsonConvert.SerializeObject(RequestChangeTaskStateByTaskByRCS.
                    CreateRequest(
                        "2", //地图编码
                        taskCode))
                );

            Log($"已暂停任务[{pauseResult.Content}]");

            //响应信息
            var responseData = new
            {
                message = "暂停接口响应",
                //timestamp = DateTime.Now,
            };

            // 发送响应
            await SendResponse(response, HttpStatusCode.OK, responseData);
        }


        /// <summary>
        /// 向所有客户端广播，机器人work指令
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task HandleRobotRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            await Form1.TCPServer.BroadcastMessageAsync("work");
        }
    }
}




