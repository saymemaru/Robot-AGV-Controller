using System.Net;
using System.Threading;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace FR_TCP_Server
{
    public partial class Form1 : Form
    {
        private TcpServer server;
        private string serverIP;
        private int serverPort;

        private string clientIP;
        private int clientPort;

        private string inputMessage;


        public Form1()
        {
            InitializeComponent();

            server = new TcpServer();
            server.LogMessage += Server_LogMessage;
            server.MessageReceived += Server_MessageReceived;

            serverIP = ServerIPBox.Text;
            int.TryParse(ServerPortBox.Text, out serverPort);

            clientIP = ClientIPBox.Text;
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

        //日志文本框变化
        private void LogBox_TextChanged(object sender, EventArgs e)
        {

        }

        //启动服务器
        private void StartServerButton_Click(object sender, EventArgs e)
        {
            try
            {
                server.Start(serverIP, serverPort);

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
                server.SendMessage(clientIP, clientPort, inputMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发送消息失败: {ex.Message}");
            }
        }

        //客户端IP
        private void ClientIPBox_TextChanged(object sender, EventArgs e)
        {
            clientIP = ClientIPBox.Text;
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
            server.Stop();
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
                server.BroadcastMessage(TextInputBox.Text);
                TextInputBox.Clear();
                return;
            }
            //是命令
            // 先在UI线程读取文本
            string commandText = TextInputBox.Text;
            ThreadPool.QueueUserWorkItem(_ => server.ExecuteServerCommand(commandText));
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
    }
}
