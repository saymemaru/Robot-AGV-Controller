using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    public class TcpServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private readonly object _clientsLock = new ();
        private readonly List<TcpClient> _connectedClients = new();

        public string ServerIp { get; private set; }
        public int ServerPort { get; private set; }

        private readonly CommandSystem _commandSystem = new CommandSystem();
        private readonly Dictionary<IPEndPoint, DateTime> _lastCommandTime = new Dictionary<IPEndPoint, DateTime>();
        private const int CommandCooldownSeconds = 1; // 命令冷却时间(秒)

        public event Action<string> LogMessage;  // 日志事件
        public event Action<string, IPEndPoint> MessageReceived;  // 接收消息事件

        public TcpServer()
        {
            _isRunning = false;

            //注册指令
            _commandSystem.RegisterCommand(new BroadcastCommand());
            _commandSystem.RegisterCommand(new TimeCommand());
            _commandSystem.RegisterCommand(new ListClientsCommand());
            _commandSystem.RegisterCommand(new HelpCommand(_commandSystem));
        }

        // 启动服务器
        public void Start(string ip, int port)
        {
            if (_isRunning) return;

            ServerIp = ip;
            ServerPort = port;
            IPAddress ipAddress = IPAddress.Parse(ip);

            _listener = new TcpListener(ipAddress, port);
            _listener.Start();
            _isRunning = true;

            Log($"服务器已启动 {ip}:{port}");
            ThreadPool.QueueUserWorkItem(AcceptClients);
        }

        // 停止服务器
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _listener.Stop();
            Log("服务器已停止");
        }

        // 接受客户端连接
        private void AcceptClients(object state)
        {
            try
            {
                while (_isRunning)
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) Log($"接受连接错误: {ex.Message}");
            }
        }

        // 处理客户端通信
        private void HandleClient(object state)
        {
            TcpClient client = (TcpClient)state;
            IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            string clientInfo = $"{clientEndPoint.Address}:{clientEndPoint.Port}";

            // 添加到连接列表
            lock (_clientsLock)
            {
                _connectedClients.Add(client);
            }

            Log($"客户端已连接: {clientInfo} (当前连接数: {_connectedClients.Count})");

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    while (_isRunning && client.Connected)
                    {
                        if (stream.DataAvailable)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;

                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                            // 检查是否是命令并处理
                            if (!_commandSystem.ProcessMessage(message, clientEndPoint, this))
                            {
                                // 如果不是命令，触发普通消息事件
                                MessageReceived?.Invoke(message, clientEndPoint);
                                Log($"来自 {clientInfo} 的消息: {message}");
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"处理客户端错误 ({clientInfo}): {ex.Message}");
            }
            finally
            {
                // 从列表中移除
                lock (_clientsLock)
                {
                    _connectedClients.Remove(client);
                }
                client.Close();
                Log($"客户端断开: {clientInfo} (当前连接数: {_connectedClients.Count})");
            }
        }

        public void BroadcastMessage(string message)
        {
            List<TcpClient> clientsToSend;

            // 复制当前客户端列表以避免长时间锁定
            lock (_clientsLock)
            {
                clientsToSend = new List<TcpClient>(_connectedClients);
            }

            if (clientsToSend.Count == 0)
            {
                Log("没有已连接的客户端可发送广播");
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(message);
            int successCount = 0;

            foreach (var client in clientsToSend)
            {
                try
                {
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    Log($"向客户端发送广播失败: {ex.Message}");
                }
            }

            Log($"已向 {successCount}/{clientsToSend.Count} 个客户端广播消息: {message}");
        }


        // 向指定客户端发送消息
        public void SendMessage(string ip, int port, string message)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(ip, port);
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    client.GetStream().Write(data, 0, data.Length);
                    Log($"已发送消息到 {ip}:{port}：{message}");
                }
            }
            catch (Exception ex)
            {
                Log($"发送消息错误: {ex.Message}");
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


        //获取连接的客户端
        public List<string> GetConnectedClients()
        {
            lock (_clientsLock)
            {
                var result = new List<string>();
                foreach (var client in _connectedClients)
                {
                    try
                    {
                        if (client.Connected)
                        {
                            var endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                            result.Add($"{endPoint.Address}:{endPoint.Port}");
                        }
                    }
                    catch
                    {
                        // 忽略已断开连接的客户端
                    }
                }
                return result;
            }
        }
    }
}
