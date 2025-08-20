using System.Net;

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
            int.TryParse(PortBox.Text, out serverPort);

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

        private void LogBox_TextChanged(object sender, EventArgs e)
        {

        }

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

        private void ServerIPBox_TextChanged(object sender, EventArgs e)
        {
            serverIP = ServerIPBox.Text;
        }

        private void PortBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(PortBox.Text, out serverPort))
            {
                // 转换成功，使用number进行后续操作
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("请输入有效的整数");
            }
        }

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

        private void ClientIPBox_TextChanged(object sender, EventArgs e)
        {
            clientIP = ClientIPBox.Text;
        }

        private void textInputBox_TextChanged(object sender, EventArgs e)
        {
            inputMessage = textInputBox.Text;
        }

        private void ClientPortBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(PortBox.Text, out clientPort))
            {
                // 转换成功，使用number进行后续操作
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("请输入有效的整数");
            }
        }

        private void StopServerButton_Click(object sender, EventArgs e)
        {
            server.Stop();
        }

        private void BoardcastButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textInputBox.Text))
            {
                MessageBox.Show("请输入要广播的消息");
                return;
            }

            server.BroadcastMessage(textInputBox.Text);
            textInputBox.Clear();
        }
    }
}
