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
            ServerPortBox = new TextBox();
            TextInputBox = new TextBox();
            StartServerButton = new Button();
            ClientIPBox = new TextBox();
            SendButton = new Button();
            ClientPortBox = new TextBox();
            StopServerButton = new Button();
            BoardcastButton = new Button();
            CameraConectionButton1 = new Button();
            SaveCloudPointButton = new Button();
            SaveRGBButton = new Button();
            HttpServerUrlBox = new TextBox();
            StartHttpServerButton = new Button();
            RCSURLBox = new TextBox();
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
            LogBox.Location = new Point(12, 99);
            LogBox.Multiline = true;
            LogBox.Name = "LogBox";
            LogBox.ScrollBars = ScrollBars.Vertical;
            LogBox.Size = new Size(710, 334);
            LogBox.TabIndex = 1;
            LogBox.TextChanged += LogBox_TextChanged;
            // 
            // ServerPortBox
            // 
            ServerPortBox.Location = new Point(127, 12);
            ServerPortBox.Name = "ServerPortBox";
            ServerPortBox.Size = new Size(72, 23);
            ServerPortBox.TabIndex = 2;
            ServerPortBox.Text = "1145";
            ServerPortBox.TextChanged += ServerPortBox_TextChanged;
            // 
            // TextInputBox
            // 
            TextInputBox.Location = new Point(12, 443);
            TextInputBox.Multiline = true;
            TextInputBox.Name = "TextInputBox";
            TextInputBox.ScrollBars = ScrollBars.Vertical;
            TextInputBox.Size = new Size(710, 57);
            TextInputBox.TabIndex = 3;
            TextInputBox.TextChanged += TextInputBox_TextChanged;
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
            ClientIPBox.Location = new Point(383, 12);
            ClientIPBox.Name = "ClientIPBox";
            ClientIPBox.Size = new Size(112, 23);
            ClientIPBox.TabIndex = 5;
            ClientIPBox.Text = "192.168.58.2";
            ClientIPBox.TextChanged += ClientIPBox_TextChanged;
            // 
            // SendButton
            // 
            SendButton.Location = new Point(579, 12);
            SendButton.Name = "SendButton";
            SendButton.Size = new Size(75, 23);
            SendButton.TabIndex = 7;
            SendButton.Text = "发送";
            SendButton.UseVisualStyleBackColor = true;
            SendButton.Click += SendButton_Click;
            // 
            // ClientPortBox
            // 
            ClientPortBox.Location = new Point(501, 12);
            ClientPortBox.Name = "ClientPortBox";
            ClientPortBox.Size = new Size(72, 23);
            ClientPortBox.TabIndex = 8;
            ClientPortBox.Text = "191";
            ClientPortBox.TextChanged += ClientPortBox_TextChanged;
            // 
            // StopServerButton
            // 
            StopServerButton.Location = new Point(294, 12);
            StopServerButton.Name = "StopServerButton";
            StopServerButton.Size = new Size(83, 23);
            StopServerButton.TabIndex = 9;
            StopServerButton.Text = "停止服务器";
            StopServerButton.UseVisualStyleBackColor = true;
            StopServerButton.Click += StopServerButton_Click;
            // 
            // BoardcastButton
            // 
            BoardcastButton.Location = new Point(729, 443);
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
            // SaveCloudPointButton
            // 
            SaveCloudPointButton.Location = new Point(728, 70);
            SaveCloudPointButton.Name = "SaveCloudPointButton";
            SaveCloudPointButton.Size = new Size(75, 23);
            SaveCloudPointButton.TabIndex = 12;
            SaveCloudPointButton.Text = "保存点云";
            SaveCloudPointButton.UseVisualStyleBackColor = true;
            SaveCloudPointButton.Click += SaveCloudPointButton_Click;
            // 
            // SaveRGBButton
            // 
            SaveRGBButton.Location = new Point(728, 99);
            SaveRGBButton.Name = "SaveRGBButton";
            SaveRGBButton.Size = new Size(75, 23);
            SaveRGBButton.TabIndex = 13;
            SaveRGBButton.Text = "保存RGB";
            SaveRGBButton.UseVisualStyleBackColor = true;
            SaveRGBButton.Click += SaveRGBButton_Click;
            // 
            // HttpServerUrlBox
            // 
            HttpServerUrlBox.Location = new Point(12, 41);
            HttpServerUrlBox.Name = "HttpServerUrlBox";
            HttpServerUrlBox.Size = new Size(187, 23);
            HttpServerUrlBox.TabIndex = 14;
            HttpServerUrlBox.Text = "http://192.168.2.102:8090/";
            HttpServerUrlBox.TextChanged += httpServerUrlBox_TextChanged;
            // 
            // StartHttpServerButton
            // 
            StartHttpServerButton.Location = new Point(205, 41);
            StartHttpServerButton.Name = "StartHttpServerButton";
            StartHttpServerButton.Size = new Size(83, 23);
            StartHttpServerButton.TabIndex = 15;
            StartHttpServerButton.Text = "Start Listen";
            StartHttpServerButton.UseVisualStyleBackColor = true;
            StartHttpServerButton.Click += StartHttpServerButton_Click;
            // 
            // RCSURLBox
            // 
            RCSURLBox.Location = new Point(12, 70);
            RCSURLBox.Name = "RCSURLBox";
            RCSURLBox.Size = new Size(187, 23);
            RCSURLBox.TabIndex = 16;
            RCSURLBox.Text = "http://192.168.2.101:50060/";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(818, 512);
            Controls.Add(RCSURLBox);
            Controls.Add(StartHttpServerButton);
            Controls.Add(HttpServerUrlBox);
            Controls.Add(SaveRGBButton);
            Controls.Add(SaveCloudPointButton);
            Controls.Add(CameraConectionButton1);
            Controls.Add(BoardcastButton);
            Controls.Add(StopServerButton);
            Controls.Add(ClientPortBox);
            Controls.Add(SendButton);
            Controls.Add(ClientIPBox);
            Controls.Add(StartServerButton);
            Controls.Add(TextInputBox);
            Controls.Add(ServerPortBox);
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
        private TextBox ServerPortBox;
        private TextBox TextInputBox;
        private Button StartServerButton;
        private TextBox ClientIPBox;
        private Button SendButton;
        private TextBox ClientPortBox;
        private Button StopServerButton;
        private Button BoardcastButton;
        private Button CameraConectionButton1;
        private Button SaveCloudPointButton;
        private Button SaveRGBButton;
        private TextBox HttpServerUrlBox;
        private Button StartHttpServerButton;
        private TextBox RCSURLBox;
    }
}
