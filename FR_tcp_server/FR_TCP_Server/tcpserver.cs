using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        //命令系统
        private readonly CommandSystem _commandSystem = new CommandSystem();
        internal Dictionary<IPEndPoint, DateTime> _lastCommandTime = new Dictionary<IPEndPoint, DateTime>();
        public int CommandCooldownSeconds { get;private set; } = 1; // 命令冷却时间(秒)

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
            _commandSystem.RegisterCommand(new WhisperCommand());
            _commandSystem.RegisterCommand(new RecoverAGVCommand());
            _commandSystem.RegisterCommand(new SnapCommand());
            _commandSystem.RegisterCommand(new GetPoseCommand());
        }

        // 启动服务器
        public void Start(string ip, int port)
        {
            if (_isRunning)
            {
                Log($"服务器运行中 {ip}:{port}");
                return;
            }

            ServerIp = ip;
            ServerPort = port;
            IPAddress ipAddress = IPAddress.Parse(ip);

            _listener = new TcpListener(ipAddress, port);
            _listener.Start();
            _isRunning = true;

            Log($"服务器已启动 {ip}:{port}");
            _ = Task.Run(() => AcceptClients(null));
            //ThreadPool.QueueUserWorkItem(AcceptClients);
        }

        // 停止服务器
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _listener.Stop();

            // 释放所有客户端连接
            lock (_clientsLock)
            {
                foreach (var client in _connectedClients)
                {
                    try
                    {
                        client.Close(); // 或 client.Dispose();
                    }
                    catch { }
                }
                _connectedClients.Clear();
            }

            Log("服务器已停止");
        }

        // 接受客户端连接
        private async Task AcceptClients(object? state)
        {
            try
            {
                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) Log($"接受连接错误: {ex.Message}");
            }
        }

        // 处理客户端通信
        private async Task HandleClientAsync(object state)
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
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    while (_isRunning && client.Connected)
                    {
                        //只读取一行，防止粘包
                        string? message = await reader.ReadLineAsync();
                        if (message == null)
                            break;

                        // 检查是否是命令并处理
                        
                        if (!_commandSystem.IsStartWithCommandSymbol(message))
                        {
                            MessageReceived?.Invoke(message, clientEndPoint);
                            Log($"来自 {clientInfo} 的消息: {message}");
                        }
                        else
                        {
                            await _commandSystem.ProcessMessageAsync(message, clientEndPoint, this);
                            MessageReceived?.Invoke(message, clientEndPoint);
                            Log($"来自 {clientInfo} 的命令: {message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"处理客户端错误 ({clientInfo}): {ex.Message}");
            }
            finally
            {
                lock (_clientsLock)
                {
                    _connectedClients.Remove(client);
                }
                client.Close();
                Log($"客户端断开: {clientInfo} (当前连接数: {_connectedClients.Count})");
            }
            /*try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    while (_isRunning && client.Connected)
                    {
                        if (stream.DataAvailable)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) 
                                break;

                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            // 检查是否是命令并处理
                             CommandResult commandResult = await _commandSystem.ProcessMessageAsync(message, clientEndPoint, this);
                            if (!commandResult.Success)
                            {
                                // 如果不是命令，触发普通消息事件
                                MessageReceived?.Invoke(message, clientEndPoint);
                                Log($"来自 {clientInfo} 的消息: {message}");
                            }
                        }
                        await Task.Delay(10);
                        //Thread.Sleep(10);
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
            }*/
        }

        //向所有客户端广播
        public async Task BroadcastMessageAsync(string message)
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
                        await stream.WriteAsync(data, 0, data.Length);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    Log($"广播失败: {ex.Message}");
                }
            }

            Log($"已向 {successCount}/{clientsToSend.Count} 个客户端广播消息: {message}");
        }

        // 向指定客户端发送消息
        public void SendMessage(string ip, int port, string message)
        {
            //向自身发送的消息不处理
            if (ip == "127.0.0.1")
            {
                return;
            }

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
        public async Task SendMessageAsync(IPAddress ip, int port, string message)
        {
            //向自身发送的消息不处理
            if (ip.Equals(IPAddress.Loopback))
            {
                return;
            }
            try
            {
                TcpClient? client = null;
                lock (_clientsLock)
                {
                    client = _connectedClients.FirstOrDefault(c =>
                    {
                        var remote = c.Client.RemoteEndPoint as IPEndPoint;
                        return remote != null && remote.Address.Equals(ip);
                    });
                }
                //为连接设置五秒超时
                /*using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    await client.ConnectAsync(ip, port, cts.Token);
                }*/
                if (client.Connected && client != null)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await client.GetStream().WriteAsync(data);
                    Log($"已发送消息到 {ip}:{port}：{message}");
                }
                else
                {
                    Log($"无法连接到 {ip}:{port}，客户端未连接");
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

        public async Task ExecuteServerCommandAsync(string commandText)
        {
            try
            {
                // 创建一个虚拟的发送者（代表服务器自身）
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
                CommandResult commandResult = await _commandSystem.ProcessMessageAsync(commandText, serverEndPoint, this);
                // 处理指令
                if (commandResult.Success)
                {
                    Log($"执行成功: {commandResult.Message}");
                    return;
                }
                else
                {
                    Log($"执行失败: {commandResult.Message}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log($"执行错误: {ex.Message}");
                return;
            }
        }

        //获取连接的客户端
        public List<string> GetConnectedClientsAddresses()
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
        public List<IPEndPoint> GetConnectedClientsIPEndPoint()
        {
            lock (_clientsLock)
            {
                List<IPEndPoint>? result = new List<IPEndPoint>();
                foreach (var client in _connectedClients)
                {
                    try
                    {
                        if (client.Connected)
                        {
                            IPEndPoint? endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                            result.Add(endPoint);
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
        public List<TcpClient> GetConnectedTcpClients()
        {
            lock (_clientsLock)
            {
                return _connectedClients
                    .Where(client =>
                    {
                        try { return client.Connected; }
                        catch { return false; }
                    })
                    .ToList();
            }
        }
    }
}
