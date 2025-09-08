using FR_TCP_Server.RCS_API;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using static FR_TCP_Server.HttpClientHelper;
using static System.Net.Mime.MediaTypeNames;

namespace FR_TCP_Server
{
    public partial class Form1 : Form
    {
        private string httpServerUrl;
        //public string RCSUrl { get; private set; }

        public static TcpServer TCPServer { get; private set; } = new TcpServer();
        private string serverIP;
        private int serverPort;

        private IPAddress clientIP;
        private int clientPort;

        private HttpServerHelper httpServer;

        private string? inputMessage;


        public Form1()
        {
            InitializeComponent();

            //RCSUrl initial
            ConfigManager.Instance.RCSUrl = RCSURLBox.Text;

            //httpserver address initial
            httpServer = new HttpServerHelper();
            httpServer.LogMessage += Server_LogMessage;
            httpServerUrl = HttpServerUrlBox.Text;

            //tcp address initial
            TCPServer.LogMessage += Server_LogMessage;
            TCPServer.MessageReceived += Server_MessageReceived;

            serverIP = ServerIPBox.Text;
            int.TryParse(ServerPortBox.Text, out serverPort);

            clientIP = IPAddress.Parse(ClientIPBox.Text);
            int.TryParse(ClientPortBox.Text, out clientPort);
        }

        private void Server_MessageReceived(string msg, IPEndPoint endPoint)
        {
            // 追加接收到的消息到文本框
            //AppendTextToLog($"来自 {endPoint} 的消息: {msg}" + Environment.NewLine);
        }

        private void Server_LogMessage(string msg)
        {
            // 追加日志到文本框
            AppendTextToLog(msg + Environment.NewLine);
        }

        //线程安全地追加文本到日志文本框
        private void AppendTextToLog(string text)
        {
            if (LogBox.InvokeRequired)
            {
                LogBox.Invoke(new Action(() => AppendTextToLog(text)));
            }
            else
            {
                LogBox.AppendText(text);
                LogBox.ScrollToCaret(); // 自动滚动到底部
            }
        }
        //线程安全地追加时间轴日志
        private bool Log(string logText)
        {
            AppendTextToLog($"[{DateTime.Now:HH:mm:ss}] {logText}" + Environment.NewLine);
            return true;
        }


        //日志文本框变化
        private void LogBox_TextChanged(object sender, EventArgs e)
        {

        }

        //启动服务器
        private void StartServerButton_Click(object sender, EventArgs e)
        {
            try
            {
                TCPServer.StartAsync(serverIP, serverPort);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动服务器失败: {ex.Message}");
            }
        }

        //服务器IP
        private void ServerIPBox_TextChanged(object sender, EventArgs e)
        {
            serverIP = ServerIPBox.Text;
        }

        //服务器端口
        private void ServerPortBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(ServerPortBox.Text, out serverPort))
            {
                // 转换成功，使用number进行后续操作
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("请输入有效的整数");
            }
        }

        //发送消息
        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                TCPServer.SendMessage(clientIP, clientPort, inputMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发送消息失败: {ex.Message}");
            }
        }

        //客户端IP
        private void ClientIPBox_TextChanged(object sender, EventArgs e)
        {
            clientIP = IPAddress.Parse(ClientIPBox.Text);
        }

        //输入消息
        private void TextInputBox_TextChanged(object sender, EventArgs e)
        {
            inputMessage = TextInputBox.Text;
        }

        //客户端端口
        private void ClientPortBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(ServerPortBox.Text, out clientPort))
            {
                // 转换成功，使用number进行后续操作
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("请输入有效的整数");
            }
        }

        //停止服务器
        private void StopServerButton_Click(object sender, EventArgs e)
        {
            TCPServer.Stop();
        }

        //广播
        private void BoardcastButton_Click(object sender, EventArgs e)
        {
            //输入为空
            if (string.IsNullOrWhiteSpace(TextInputBox.Text))
            {
                MessageBox.Show("请输入要广播的消息");
                return;
            }
            //不是命令
            if (!TextInputBox.Text.StartsWith("/"))
            {
                TCPServer.BroadcastMessage(TextInputBox.Text);
                TextInputBox.Clear();
                return;
            }
            //是命令
            // 先在UI线程读取文本
            string commandText = TextInputBox.Text;
            _ = Task.Run(() =>
            {
                // 在后台线程处理命令
                TCPServer.ExecuteServerCommand(commandText);
            });
            //ThreadPool.QueueUserWorkItem(_ => TCPServer.ExecuteServerCommand(commandText));
            TextInputBox.Clear();
        }

        //连接相机
        private void CameraConnectionButton_Click(object sender, EventArgs e)
        {
            string cameraLog;

            if (CameraManager.Instance.isCameraConnected)
            {
                AppendTextToLog($"[{DateTime.Now:HH:mm:ss}] 相机已连接" + Environment.NewLine);
                LogBox.ScrollToCaret();
                return;
            }
            else if (CameraManager.Instance.InitializeCamera(out cameraLog))
            {
                AppendTextToLog($"[{DateTime.Now:HH:mm:ss}] 相机初始化成功: "
                    + cameraLog + Environment.NewLine);
                LogBox.ScrollToCaret();
            }
            else
            {
                AppendTextToLog($"[{DateTime.Now:HH:mm:ss}] 相机初始化失败: "
                    + cameraLog + Environment.NewLine);
                LogBox.ScrollToCaret();
            }
        }

        //保存点云图
        private void SaveCloudPointButton_Click(object sender, EventArgs e)
        {
            if (!CameraManager.Instance.isCameraConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }
            CameraManager.Instance.SaveCloudPointFile();
        }

        //保存RGB图像
        private void SaveRGBButton_Click(object sender, EventArgs e)
        {
            if (!CameraManager.Instance.isCameraConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }
            CameraManager.Instance.SaveRGBFile();
        }

        //http服务器URL输入
        private void httpServerUrlBox_TextChanged(object sender, EventArgs e)
        {
            httpServerUrl = HttpServerUrlBox.Text;
        }

        //启动http服务器
        private async void StartHttpServerButton_Click(object sender, EventArgs e)
        {
            try
            {
                await httpServer.Start(httpServerUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动服务器失败: {ex.Message}");
            }
        }

        private void RCSURLBox_TextChanged(object sender, EventArgs e)
        {
            ConfigManager.Instance.RCSUrl = RCSURLBox.Text;
        }

        //待办（同时按测试按钮可能会冲突）
        private void TestButton1_Click(object sender, EventArgs e)
        {
            //验证api地址
            Log($"api地址: {ConfigManager.Instance.RCSUrl + RequestGetTaskByAgvCodeByRCS.APIpath}");

            _ = Task.Run(async () =>
            {
                //获取任务编码
                RequestResult taskCodeResult =
                await HttpClientHelper.Instance.ExecuteAsync(
                    ConfigManager.Instance.RCSUrl + RequestGetTaskByAgvCodeByRCS.APIpath,
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
                    ConfigManager.Instance.RCSUrl + RequestChangeTaskStateByTaskByRCS.APIpath,
                    RequestChangeTaskStateByTaskByRCS.HttpMethod,
                    JsonConvert.SerializeObject(RequestChangeTaskStateByTaskByRCS.
                        CreateRequest(
                            "2", //地图编码
                            taskCode))
                    );

                Log($"已暂停任务[{pauseResult.Content}]");

            });

        }

        private void TestButton2_Click(object sender, EventArgs e)
        {
            //验证api地址
            Log($"api地址: {ConfigManager.Instance.RCSUrl + RequestGetTaskByAgvCodeByRCS.APIpath}");

            _ = Task.Run(async () =>
            {
                //获取任务编码
                RequestResult taskCodeResult =
                await HttpClientHelper.Instance.ExecuteAsync(
                    ConfigManager.Instance.RCSUrl + RequestGetTaskByAgvCodeByRCS.APIpath,
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

                RequestResult recoverResult = 
                await HttpClientHelper.Instance.ExecuteAsync(
                    ConfigManager.Instance.RCSUrl + RequestRecoverAgvTaskByTaskByRCS.APIpath,
                    RequestRecoverAgvTaskByTaskByRCS.HttpMethod,
                    JsonConvert.SerializeObject(RequestRecoverAgvTaskByTaskByRCS.
                        CreateRequest(
                            "2", //地图编码
                            taskCode))
                    );
                Log($"已恢复任务[{recoverResult.Content}]");
            });
        }
    }
}
