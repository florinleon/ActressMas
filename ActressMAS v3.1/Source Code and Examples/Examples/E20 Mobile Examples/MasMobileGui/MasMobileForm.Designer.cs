namespace MasMobileGui
{
    partial class MasMobileForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listBoxAgents = new System.Windows.Forms.ListBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.textBoxServerIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxServerPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonRunMas = new System.Windows.Forms.Button();
            this.comboBoxName = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // listBoxAgents
            // 
            this.listBoxAgents.FormattingEnabled = true;
            this.listBoxAgents.ItemHeight = 16;
            this.listBoxAgents.Location = new System.Drawing.Point(42, 53);
            this.listBoxAgents.MultiColumn = true;
            this.listBoxAgents.Name = "listBoxAgents";
            this.listBoxAgents.Size = new System.Drawing.Size(663, 100);
            this.listBoxAgents.TabIndex = 0;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(42, 490);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(126, 32);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Enabled = false;
            this.buttonDisconnect.Location = new System.Drawing.Point(220, 490);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(126, 32);
            this.buttonDisconnect.TabIndex = 2;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // textBoxServerIP
            // 
            this.textBoxServerIP.Location = new System.Drawing.Point(371, 438);
            this.textBoxServerIP.Name = "textBoxServerIP";
            this.textBoxServerIP.Size = new System.Drawing.Size(85, 22);
            this.textBoxServerIP.TabIndex = 3;
            this.textBoxServerIP.Text = "127.0.0.1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(295, 441);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Server IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Agents";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(576, 490);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(126, 32);
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Location = new System.Drawing.Point(42, 213);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.Size = new System.Drawing.Size(662, 187);
            this.richTextBoxLog.TabIndex = 7;
            this.richTextBoxLog.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 179);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "Container log";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(530, 441);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Server port:";
            // 
            // textBoxServerPort
            // 
            this.textBoxServerPort.Location = new System.Drawing.Point(619, 438);
            this.textBoxServerPort.Name = "textBoxServerPort";
            this.textBoxServerPort.Size = new System.Drawing.Size(85, 22);
            this.textBoxServerPort.TabIndex = 9;
            this.textBoxServerPort.Text = "5000";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(39, 441);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "Name:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonRunMas
            // 
            this.buttonRunMas.Enabled = false;
            this.buttonRunMas.Location = new System.Drawing.Point(398, 490);
            this.buttonRunMas.Name = "buttonRunMas";
            this.buttonRunMas.Size = new System.Drawing.Size(126, 32);
            this.buttonRunMas.TabIndex = 13;
            this.buttonRunMas.Text = "Run MAS";
            this.buttonRunMas.UseVisualStyleBackColor = true;
            this.buttonRunMas.Click += new System.EventHandler(this.buttonRunMas_Click);
            // 
            // comboBoxName
            // 
            this.comboBoxName.FormattingEnabled = true;
            this.comboBoxName.Items.AddRange(new object[] {
            "Container1",
            "Container2",
            "Container3"});
            this.comboBoxName.Location = new System.Drawing.Point(95, 434);
            this.comboBoxName.Name = "comboBoxName";
            this.comboBoxName.Size = new System.Drawing.Size(154, 24);
            this.comboBoxName.TabIndex = 14;
            // 
            // MasMobileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(747, 557);
            this.Controls.Add(this.comboBoxName);
            this.Controls.Add(this.buttonRunMas);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxServerPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxServerIP);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.listBoxAgents);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MasMobileForm";
            this.Text = "ActressMas Container";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ContainerForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxAgents;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.TextBox textBoxServerIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxServerPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonRunMas;
        private System.Windows.Forms.ComboBox comboBoxName;
    }
}

