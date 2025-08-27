using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    // 命令处理接口(创建命令)
    public interface ICommandHandler
    {
        string CommandName { get; }
        string Description { get; }
        CommandResult Execute(string[] args, IPEndPoint sender, TcpServer server);

    }

    // 命令执行结果
    public class CommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public CommandResult(bool success, string message = "")
        {
            Success = success;
            Message = message;
        }
    }

    // 命令系统,使用时创建命令系统对象，然后使用RegisterCommand注册命令，最后调用ProcessMessage处理消息
    public class CommandSystem
    {
        // 存储注册命令字典
        private readonly ConcurrentDictionary<string, ICommandHandler> _commandHandlers =
            new(StringComparer.OrdinalIgnoreCase);

        // 注册命令
        public void RegisterCommand(ICommandHandler handler)
        {
            if (_commandHandlers.ContainsKey(handler.CommandName))
                throw new InvalidOperationException($"命令已注册: {handler.CommandName}");
            _commandHandlers[handler.CommandName] = handler;
        }

        // 处理指令
        public CommandResult ProcessMessage(string message, IPEndPoint sender, TcpServer server)
        {
            // 检查是否以斜杠开头 (例如: /command arg1 arg2)
            if (!message.StartsWith("/"))
            {
                server.SendMessage(sender.Address, sender.Port, "不是有效的命令格式，请以斜杠开头");
                return new CommandResult(false, "不是有效的命令格式，请以斜杠开头");
            }

            // 服务器指令冷却判断
            var lastCommandTime = server._lastCommandTime;
            int cooldownSeconds = server.CommandCooldownSeconds;
            DateTime now = DateTime.Now;
            if (lastCommandTime.TryGetValue(sender, out DateTime lastTime))
            {
                if ((now - lastTime).TotalSeconds < cooldownSeconds)
                {
                    server.SendMessage(sender.Address, sender.Port, $"命令冷却中，请稍后再试（{cooldownSeconds}秒）");
                    return new CommandResult(true, $"命令冷却中，请稍后再试（{cooldownSeconds}秒）");
                }
            }

            // 解析命令和参数,以空格分割
            var parts = message.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                server.SendMessage(sender.Address, sender.Port, "不是有效的命令格式，请填入参数");
                return new CommandResult(false, "不是有效的命令格式，请填入参数");
            }

            
            //分割命令和参数
            var commandName = parts[0];
            var args = parts.Skip(1).ToArray();

            // 查找命令处理器并执行
            if (_commandHandlers.TryGetValue(commandName, out var handler))
            {
                // 更新最后命令时间
                server._lastCommandTime[sender] = DateTime.Now;

                return handler.Execute(args, sender, server);
            }

            // 命令未找到
            server.SendMessage(sender.Address, sender.Port, $"未知命令: {commandName}");
            return new CommandResult(false, $"未知命令: {commandName}");
        }

        // 获取所有可用命令
        public IEnumerable<string> GetAvailableCommands()
        {
            foreach (var handler in _commandHandlers.Values)
            {
                yield return $"/{handler.CommandName} - {handler.Description}";
            }
        }
    }

    // 广播消息命令
    public class BroadcastCommand : ICommandHandler
    {
        public string CommandName => "broadcast";
        public string Description => "向所有客户端广播消息";

        public CommandResult Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            if (args.Length == 0)
            {
                server.SendMessage(sender.Address, sender.Port, "用法: /broadcast <消息>");
                return new CommandResult(false, "用法: /broadcast <消息>");
            }

            string message = string.Join(" ", args);
            server.BroadcastMessage($"[广播] {sender.Address} 说: {message}");
            return new CommandResult(true, $"{sender.Address} [广播] {message}");
        }
    }

    // 获取服务器时间命令
    public class TimeCommand : ICommandHandler
    {
        public string CommandName => "time";
        public string Description => "获取服务器当前时间";

        public CommandResult Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            server.SendMessage(sender.Address, sender.Port,
                $"服务器时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            return new CommandResult(true, $"服务器时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
    }

    // 列出在线客户端命令
    public class ListClientsCommand : ICommandHandler
    {
        public string CommandName => "clients";
        public string Description => "列出所有在线客户端";

        public CommandResult Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            // 注意: 这里需要修改TcpServer以提供获取客户端列表的方法
            var clients = server.GetConnectedClientsAddresses();

            if (clients.Count == 0)
            {
                server.SendMessage(sender.Address, sender.Port, "没有客户端在线");
                return new CommandResult(false, "没有客户端在线");
            }

            string response = $"在线客户端({clients.Count}):\n";
            foreach (var client in clients)
            {
                response += $"- {client}\n";
            }

            server.SendMessage(sender.Address, sender.Port, response);
            return new CommandResult(true, response);
        }
    }

    // 帮助命令
    public class HelpCommand : ICommandHandler
    {
        private readonly CommandSystem _commandSystem;

        public HelpCommand(CommandSystem commandSystem)
        {
            _commandSystem = commandSystem;
        }

        public string CommandName => "help";
        public string Description => "显示可用命令";

        public CommandResult Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            var commands = _commandSystem.GetAvailableCommands();
            string response = "可用命令:\n";

            foreach (var cmd in commands)
            {
                response += $"{cmd}\n";
            }

            server.SendMessage(sender.Address, sender.Port, response);
            return new CommandResult(true, response);
        }
    }

    // 私聊命令
    public class WhisperCommand : ICommandHandler
    {
        public string CommandName => "whisper";
        public string Description => "向指定IP的客户端发送私聊消息，用法: /whisper <目标IP> <消息>";

        public CommandResult Execute(string[] args, IPEndPoint sender, TcpServer server)
        {

            //此处没有考虑消息为空格字符的情况
            if (args.Length < 2)
            {
                server.SendMessage(sender.Address, sender.Port, "用法: /whisper <目标IP> <消息>");
                return new CommandResult(false, "用法: /whisper <目标IP> <消息>");
            }

            string targetIp = args[0];
            string message = string.Join(" ", args.Skip(1));

            // 查找目标客户端端口
            List<IPEndPoint> clientsIPEndPoint = server.GetConnectedClientsIPEndPoint();
            var targetClient = clientsIPEndPoint.FirstOrDefault(c =>
                c.Address.ToString() == targetIp);

            if (targetClient == null)
            {
                server.SendMessage(sender.Address, sender.Port, $"未找到目标客户端: {targetIp}");
                return new CommandResult(false, $"{sender.Address} 未找到目标客户端: {targetIp}");
            }

            server.SendMessage(targetClient.Address, targetClient.Port, $"[私聊] {sender.Address} 说: {message}");
            server.SendMessage(sender.Address, sender.Port, $"已发送私聊给 {targetIp}: {message}");
            return new CommandResult(true, $"{sender.Address} [私聊] {targetIp}: {message}");
        }
    }
}
