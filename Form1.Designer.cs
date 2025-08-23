namespace FR_TCP_Server
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ServerIPBox = new TextBox();
            LogBox = new TextBox();
            PortBox = new TextBox();
            textInputBox = new TextBox();
            StartServerButton = new Button();
            ClientIPBox = new TextBox();
            SendButton = new Button();
            ClientPortBox = new TextBox();
            StopServerButton = new Button();
            BoardcastButton = new Button();
            CameraConectionButton1 = new Button();
            SuspendLayout();
            // 
            // ServerIPBox
            // 
            ServerIPBox.Location = new Point(12, 12);
            ServerIPBox.Name = "ServerIPBox";
            ServerIPBox.Size = new Size(109, 23);
            ServerIPBox.TabIndex = 0;
            ServerIPBox.Text = "192.168.58.5";
            ServerIPBox.TextChanged += ServerIPBox_TextChanged;
            // 
            // LogBox
            // 
            LogBox.Location = new Point(12, 41);
            LogBox.Multiline = true;
            LogBox.Name = "LogBox";
            LogBox.ScrollBars = ScrollBars.Vertical;
            LogBox.Size = new Size(710, 334);
            LogBox.TabIndex = 1;
            LogBox.TextChanged += LogBox_TextChanged;
            // 
            // PortBox
            // 
            PortBox.Location = new Point(127, 12);
            PortBox.Name = "PortBox";
            PortBox.Size = new Size(72, 23);
            PortBox.TabIndex = 2;
            PortBox.Text = "1145";
            PortBox.TextChanged += PortBox_TextChanged;
            // 
            // textInputBox
            // 
            textInputBox.Location = new Point(12, 381);
            textInputBox.Multiline = true;
            textInputBox.Name = "textInputBox";
            textInputBox.ScrollBars = ScrollBars.Vertical;
            textInputBox.Size = new Size(710, 57);
            textInputBox.TabIndex = 3;
            textInputBox.TextChanged += textInputBox_TextChanged;
            // 
            // StartServerButton
            // 
            StartServerButton.Location = new Point(205, 12);
            StartServerButton.Name = "StartServerButton";
            StartServerButton.Size = new Size(83, 23);
            StartServerButton.TabIndex = 4;
            StartServerButton.Text = "启动服务器";
            StartServerButton.UseVisualStyleBackColor = true;
            StartServerButton.Click += StartServerButton_Click;
            // 
            // ClientIPBox
            // 
            ClientIPBox.Location = new Point(294, 12);
            ClientIPBox.Name = "ClientIPBox";
            ClientIPBox.Size = new Size(112, 23);
            ClientIPBox.TabIndex = 5;
            ClientIPBox.Text = "192.168.58.2";
            ClientIPBox.TextChanged += ClientIPBox_TextChanged;
            // 
            // SendButton
            // 
            SendButton.Location = new Point(490, 12);
            SendButton.Name = "SendButton";
            SendButton.Size = new Size(75, 23);
            SendButton.TabIndex = 7;
            SendButton.Text = "发送";
            SendButton.UseVisualStyleBackColor = true;
            SendButton.Click += SendButton_Click;
            // 
            // ClientPortBox
            // 
            ClientPortBox.Location = new Point(412, 12);
            ClientPortBox.Name = "ClientPortBox";
            ClientPortBox.Size = new Size(72, 23);
            ClientPortBox.TabIndex = 8;
            ClientPortBox.Text = "191";
            ClientPortBox.TextChanged += ClientPortBox_TextChanged;
            // 
            // StopServerButton
            // 
            StopServerButton.Location = new Point(571, 12);
            StopServerButton.Name = "StopServerButton";
            StopServerButton.Size = new Size(83, 23);
            StopServerButton.TabIndex = 9;
            StopServerButton.Text = "停止服务器";
            StopServerButton.UseVisualStyleBackColor = true;
            StopServerButton.Click += StopServerButton_Click;
            // 
            // BoardcastButton
            // 
            BoardcastButton.Location = new Point(660, 12);
            BoardcastButton.Name = "BoardcastButton";
            BoardcastButton.Size = new Size(74, 23);
            BoardcastButton.TabIndex = 10;
            BoardcastButton.Text = "广播";
            BoardcastButton.UseVisualStyleBackColor = true;
            BoardcastButton.Click += BoardcastButton_Click;
            // 
            // CameraConectionButton1
            // 
            CameraConectionButton1.Location = new Point(728, 41);
            CameraConectionButton1.Name = "CameraConectionButton1";
            CameraConectionButton1.Size = new Size(75, 23);
            CameraConectionButton1.TabIndex = 11;
            CameraConectionButton1.Text = "相机连接";
            CameraConectionButton1.UseVisualStyleBackColor = true;
            CameraConectionButton1.Click += CameraConnectionButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(CameraConectionButton1);
            Controls.Add(BoardcastButton);
            Controls.Add(StopServerButton);
            Controls.Add(ClientPortBox);
            Controls.Add(SendButton);
            Controls.Add(ClientIPBox);
            Controls.Add(StartServerButton);
            Controls.Add(textInputBox);
            Controls.Add(PortBox);
            Controls.Add(LogBox);
            Controls.Add(ServerIPBox);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox ServerIPBox;
        private TextBox LogBox;
        private TextBox PortBox;
        private TextBox textInputBox;
        private Button StartServerButton;
        private TextBox ClientIPBox;
        private Button SendButton;
        private TextBox ClientPortBox;
        private Button StopServerButton;
        private Button BoardcastButton;
        private Button CameraConectionButton1;
    }
}
