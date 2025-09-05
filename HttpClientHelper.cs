using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    public class HttpClientHelper
    {
        private const int Time_Out_Second = 30;

        //单例client
        private static readonly Lazy<HttpClientHelper> _LazyInstance
            = new Lazy<HttpClientHelper>(() => new HttpClientHelper(Time_Out_Second));
        public static HttpClientHelper Instance => _LazyInstance.Value;
        private HttpClientHelper(int _timeoutSeconds)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds); // 设置超时时间
        }

        private HttpClient _httpClient;
        public event Action<string> LogMessage;  // 日志事件

        // 请求结果类
        public class RequestResult
        {
            public bool Success { get; set; }

            public string Content { get; set; }

            public RequestResult(bool success, string content = "")
            {
                Success = success;
                Content = content;
            }
        }

        // 执行HTTP请求的通用方法
        public async Task<RequestResult> ExecuteAsync(string url, HttpMethod method, string? content = null, dynamic headers = null)
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

                        //待办（响应内容转换为其他格式，目前为字符串）
                        // 读取响应内容
                        string responseContent = await response.Content.ReadAsStringAsync();

                        //JsonConvert.DeserializeObject(responseContent);
                        return new RequestResult(true, responseContent);
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

        //待办（将Log搓到Utility里，形成通用方法）

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
