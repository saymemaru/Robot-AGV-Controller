using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server
{
    public interface ICommandHandler
    {
        string CommandName { get; }
        string Description { get; }
        bool Execute(string[] args, IPEndPoint sender, TcpServer server);
    }

    public class CommandSystem
    {
        private readonly Dictionary<string, ICommandHandler> _commandHandlers =
            new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase);

        public void RegisterCommand(ICommandHandler handler)
        {
            _commandHandlers[handler.CommandName] = handler;
        }

        public bool ProcessMessage(string message, IPEndPoint sender, TcpServer server)
        {
            // 检查是否是命令格式 (例如: /command arg1 arg2)
            if (!message.StartsWith("/"))
                return false;

            // 解析命令和参数
            var parts = message.Substring(1).Split(' ');
            if (parts.Length == 0)
                return false;

            var commandName = parts[0];
            var args = parts.Length > 1 ?
                new ArraySegment<string>(parts, 1, parts.Length - 1).Array :
                new string[0];

            // 查找命令处理器
            if (_commandHandlers.TryGetValue(commandName, out var handler))
            {
                return handler.Execute(args, sender, server);
            }

            // 命令未找到
            server.SendMessage(sender.Address.ToString(), sender.Port, $"未知命令: {commandName}");
            return false;
        }

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

        public bool Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            if (args.Length == 0)
            {
                server.SendMessage(sender.Address.ToString(), sender.Port, "用法: /broadcast <消息>");
                return false;
            }

            string message = string.Join(" ", args);
            server.BroadcastMessage($"[广播] {message} (来自 {sender.Address})");
            return true;
        }
    }

    // 获取服务器时间命令
    public class TimeCommand : ICommandHandler
    {
        public string CommandName => "time";
        public string Description => "获取服务器当前时间";

        public bool Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            server.SendMessage(sender.Address.ToString(), sender.Port,
                $"服务器时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            return true;
        }
    }

    // 列出在线客户端命令
    public class ListClientsCommand : ICommandHandler
    {
        public string CommandName => "clients";
        public string Description => "列出所有在线客户端";

        public bool Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            // 注意: 这里需要修改TcpServer以提供获取客户端列表的方法
            // 假设我们添加了一个GetConnectedClients方法
            var clients = server.GetConnectedClients();

            if (clients.Count == 0)
            {
                server.SendMessage(sender.Address.ToString(), sender.Port, "没有客户端在线");
                return true;
            }

            string response = "在线客户端:\n";
            foreach (var client in clients)
            {
                response += $"- {client}\n";
            }

            server.SendMessage(sender.Address.ToString(), sender.Port, response);
            return true;
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

        public bool Execute(string[] args, IPEndPoint sender, TcpServer server)
        {
            var commands = _commandSystem.GetAvailableCommands();
            string response = "可用命令:\n";

            foreach (var cmd in commands)
            {
                response += $"{cmd}\n";
            }

            server.SendMessage(sender.Address.ToString(), sender.Port, response);
            return true;
        }
    }
}
