using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;  
using System.Threading.Tasks;
using System.Net;
using System.Security.Policy;

namespace FR_TCP_Server

{
    public class HttpServerHelper
    {
        private readonly HttpListener _listener;
        private CancellationTokenSource _cts; //跳出侦听循环标识符
        private bool _isRunning = false;

        public event Action<string> LogMessage;  // 日志事件

        public HttpServerHelper()
        {
            _listener = new HttpListener();
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
                _isRunning = true;
                Log($"http服务器 {url} 侦听中");

                // 开始监听请求
                await ListenForRequests(_cts.Token);
            }
            catch (Exception ex)
            {
                _isRunning = false;
                Log($"启动服务器时出错: {ex.Message}");
            }
        }

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


        private async void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

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

                // 将JSON字符串转换为RequestData（json）对象（待实现）
                if (requestBody != null)
                {
                    Log($"请求内容: {requestBody}");
                    //RequestData requestData = JsonConvert.DeserializeObject<RequestData>(requestBody)
                }

                // 处理不同的HTTP方法
                /*string responseString;
                switch (request.HttpMethod)
                {
                    case "GET":
                        responseString = "你好！这是一个GET请求的响应。";
                        break;
                    case "POST":
                        responseString = $"你好！这是一个POST请求的响应。收到的内容: {requestBody}";
                        break;
                    default:
                        responseString = "不支持的请求方法";
                        response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        break;
                }*/

                // 构造响应内容
                // (现在只支持JSON格式和对post请求的响应，且无论是否接收到请求内容都返回成功)
                var responseObject = new
                {
                    Success = true,
                    ErrorCode = 0,
                    Msg = "请求成功！"
                };
                string jsonResponse = JsonConvert.SerializeObject(responseObject);
                var buffer = Encoding.UTF8.GetBytes(jsonResponse);

                // 发送响应
                // 获取Response对象，并设置响应头和相关属性
                response = context.Response;
                // 设置返回内容的长度
                response.ContentLength64 = buffer.Length;
                // 设置HTTP状态码，200表示成功
                response.StatusCode = 200;
                // 设置状态描述（可选，通常StatusCode设置就够了）
                response.StatusDescription = "OK";
                // 设置内容类型为 application/json
                response.ContentType = "application/json; charset=utf-8";

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
                //Console.WriteLine($"已发送响应: {jsonResponse}");
            }
            catch (Exception ex)
            {
                Log($"处理请求时出错: {ex.Message}");
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

        private void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            _isRunning = false;

            Log("已停止侦听。");
        }


    }

    public class HttpClientHlper
    {
        private HttpClient _httpClient;

        public event Action<string> LogMessage;  // 日志事件

        public HttpClientHlper(int _timeoutSeconds)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds); // 设置超时时间
        }

        public async Task<T> ExecuteAsync<T>(string url, HttpMethod method, string content = null, dynamic headers = null)
        {

            try
            {
                // 创建请求消息
                using (var request = new HttpRequestMessage(method, url))
                {
                    // 添加请求头
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }

                    // 添加请求内容
                    if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
                    {
                        request.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    }

                    // 发送请求
                    using (var response = await _httpClient.SendAsync(request))
                    {
                        // 确保成功状态码
                        _ = response.EnsureSuccessStatusCode();

                        // 读取响应内容
                        var responseContent = await response.Content.ReadAsStringAsync();

                        // 反序列化响应
                        return JsonConvert.DeserializeObject<T>(responseContent);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Log($"HTTP请求失败: {ex.Message}");
                throw new Exception($"HTTP请求失败: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                Log($"请求超时: {ex.Message}");
                throw new Exception($"请求超时: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Log($"请求处理失败: {ex.Message}");
                throw new Exception($"请求处理失败: {ex.Message}", ex);
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
    }

}


