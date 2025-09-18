using FR_TCP_Server.RCS_API;
using Gemini335;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FR_TCP_Server.HttpClientHelper;

namespace FR_TCP_Server
{
    // 命令处理接口(创建命令)
    public interface ICommandHandler
    {
        string CommandName { get; }
        string Description { get; }
        Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server);

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
        //指令起始字符
        private readonly char commandSymbol = '/';

        // 注册命令
        public void RegisterCommand(ICommandHandler handler)
        {
            if (_commandHandlers.ContainsKey(handler.CommandName))
                throw new InvalidOperationException($"命令已注册: {handler.CommandName}");
            _commandHandlers[handler.CommandName] = handler;
        }

        //判断起始符
        public bool IsStartWithCommandSymbol(string message)
        {
            if (!message.StartsWith(commandSymbol))
            {
                return false;
            }
            return true;
        }

        // 处理指令
        public async Task<CommandResult> ProcessMessageAsync(string message, IPEndPoint sender, TcpServer server)
        {
            // 检查是否以斜杠开头 (例如: /command arg1 arg2)
            if (!IsStartWithCommandSymbol(message))
            {
                await server.SendMessageAsync(sender.Address, sender.Port, "不是有效的命令格式，请以斜杠开头");
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
                    await server.SendMessageAsync(sender.Address, sender.Port, $"命令冷却中，请稍后再试（{cooldownSeconds}秒）");
                    return new CommandResult(true, $"命令冷却中，请稍后再试（{cooldownSeconds}秒）");
                }
            }

            // 解析命令和参数,以空格分割
            var parts = message.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                await server.SendMessageAsync(sender.Address, sender.Port, "不是有效的命令格式，请填入参数");
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
                CommandResult commandResult = await handler.ExecuteAsync(args, sender, server);
                return commandResult;
            }

            // 命令未找到
            await server.SendMessageAsync(sender.Address, sender.Port, $"未知命令: {commandName}");
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

        public async Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server)
        {
            if (args.Length == 0)
            {
                await server.SendMessageAsync(sender.Address, sender.Port, "用法: /broadcast <消息>");
                return new CommandResult(false, "用法: /broadcast <消息>");
            }

            string message = string.Join(" ", args);
            await server.BroadcastMessageAsync($"[广播] {sender.Address} 说: {message}");
            return new CommandResult(true, $"{sender.Address} [广播] {message}");
        }
    }

    // 获取服务器时间命令
    public class TimeCommand : ICommandHandler
    {
        public string CommandName => "time";
        public string Description => "获取服务器当前时间";

        public async Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server)
        {
            await server.SendMessageAsync(sender.Address, sender.Port,
                $"服务器时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            return new CommandResult(true, $"服务器时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
    }

    // 列出在线客户端命令
    public class ListClientsCommand : ICommandHandler
    {
        public string CommandName => "clients";
        public string Description => "列出所有在线客户端";

        public async Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server)
        {
            // 注意: 这里需要修改TcpServer以提供获取客户端列表的方法
            var clients = server.GetConnectedClientsAddresses();

            if (clients.Count == 0)
            {
                await server.SendMessageAsync(sender.Address, sender.Port, "没有客户端在线");
                return new CommandResult(false, "没有客户端在线");
            }

            string response = $"在线客户端({clients.Count}):\n";
            foreach (var client in clients)
            {
                response += $"- {client}\n";
            }

            await server.SendMessageAsync(sender.Address, sender.Port, response);
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

        public async Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server)
        {
            var commands = _commandSystem.GetAvailableCommands();
            string response = "可用命令:\n";

            foreach (var cmd in commands)
            {
                response += $"{cmd}\n";
            }

            await server.SendMessageAsync(sender.Address, sender.Port, response);
            return new CommandResult(true, response);
        }
    }

    // 私聊命令
    public class WhisperCommand : ICommandHandler
    {
        public string CommandName => "whisper";
        public string Description => "向指定IP的客户端发送私聊消息，用法: /whisper <目标IP> <消息>";

        public async Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server)
        {

            //此处没有考虑消息为空格字符的情况
            if (args.Length < 2)
            {
                await server.SendMessageAsync(sender.Address, sender.Port, "用法: /whisper <目标IP> <消息>");
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
                await server.SendMessageAsync(sender.Address, sender.Port, $"未找到目标客户端: {targetIp}");
                return new CommandResult(false, $"{sender.Address} 未找到目标客户端: {targetIp}");
            }

            await server.SendMessageAsync(targetClient.Address, targetClient.Port, $"[私聊] {sender.Address} 说: {message}");
            await server.SendMessageAsync(sender.Address, sender.Port, $"已发送私聊给 {targetIp}: {message}");
            return new CommandResult(true, $"{sender.Address} [私聊] {targetIp}: {message}");
        }
    }

    public class RecoverAGVCommand : ICommandHandler
    {
        public string CommandName => "RecoverAGV";
        public string Description => "恢复AGV任务";

        public async Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server)
        {
            if (args.Length < 2)
            {
                await server.SendMessageAsync(sender.Address, sender.Port, "用法: /RecoverAGV <AGVCode> <MapCode>");
                return new CommandResult(false, "用法: /RecoverAGV <AGVCode> <MapCode>");
            }

            string aGVCode = args[0];
            string mapCode = args[1];
            string? response = null;

            //获取任务编码
            RequestResult taskCodeResult =
            await HttpClientHelper.Instance.ExecuteAsync(
                ConfigManager.Instance.RCSUrl + RequestGetTaskByAgvCodeByRCS.APIpath,
                RequestGetTaskByAgvCodeByRCS.HttpMethod,
                JsonConvert.SerializeObject(RequestGetTaskByAgvCodeByRCS.
                    CreateRequest(
                        aGVCode))//agv编码
                );

            //检验RCS返回值
            if (taskCodeResult.Success == true)
            {
                response = $"获取任务编码: {taskCodeResult.Content}";
                await server.SendMessageAsync(sender.Address, sender.Port, response);
            }
            else
            {
                response = $"获取任务编码失败 Error: {taskCodeResult.Content}";
                await server.SendMessageAsync(sender.Address, sender.Port, response);
                return new CommandResult(false, response);
            }

            //接收到的是json字符串需要反序列化   
            string? taskCode = JsonConvert.DeserializeObject<string>(taskCodeResult.Content);

            //恢复任务
            RequestResult recoverResult =
            await HttpClientHelper.Instance.ExecuteAsync(
                ConfigManager.Instance.RCSUrl + RequestRecoverAgvTaskByTaskByRCS.APIpath,
                RequestRecoverAgvTaskByTaskByRCS.HttpMethod,
                JsonConvert.SerializeObject(RequestRecoverAgvTaskByTaskByRCS.
                    CreateRequest(
                        mapCode, //地图编码
                        taskCode))
                );

            //待办
            ///未做返回结果验证 

            response = $"已恢复任务[{recoverResult.Content}]";
            await server.SendMessageAsync(sender.Address, sender.Port, response);
            return new CommandResult(true, response);

        }
    }

    // 拍照命令
    public class SnapCommand : ICommandHandler
    {
        public string CommandName => "snap";
        public string Description => "请求相机拍照";

        public async Task<CommandResult> ExecuteAsync(string[] args, IPEndPoint sender, TcpServer server)
        {
            if (args.Length == 0)
            {
                await server.SendMessageAsync(sender.Address, sender.Port, "用法: /snap <文件名>");
                return new CommandResult(false, "用法: /snap <文件名>");
            }

            //照片文件名
            string fileStartName = args[0];

            //响应
            string response;
            if (SaveFile.dirPath == null)
                response = $"{fileStartName} 已保存到默认文件夹/Pic";
            else
                response = $"{fileStartName} 已保存到 {SaveFile.dirPath}";

            _ = server.SendMessageAsync(sender.Address, sender.Port, response);


            //保存文件
            await Gemini335Camera.Instance.GetRGBDImgAsync(true, fileStartName,false);

            return new CommandResult(true, response);
        }
    }
}
