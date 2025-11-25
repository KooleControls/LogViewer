namespace LogViewer.Controls
{
    partial class MqttSourceControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox_BrokerUrl = new TextBox();
            textBox_topic = new TextBox();
            btnConnect = new Button();
            SuspendLayout();
            // 
            // textBox_BrokerUrl
            // 
            textBox_BrokerUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox_BrokerUrl.Location = new Point(3, 3);
            textBox_BrokerUrl.Name = "textBox_BrokerUrl";
            textBox_BrokerUrl.Size = new Size(235, 23);
            textBox_BrokerUrl.TabIndex = 0;
            textBox_BrokerUrl.Text = "basserver.local";
            // 
            // textBox_topic
            // 
            textBox_topic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox_topic.Location = new Point(3, 32);
            textBox_topic.Name = "textBox_topic";
            textBox_topic.Size = new Size(235, 23);
            textBox_topic.TabIndex = 1;
            textBox_topic.Text = "000000_0";
            // 
            // btnConnect
            // 
            btnConnect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnConnect.Location = new Point(3, 61);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(235, 23);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // MqttSourceControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnConnect);
            Controls.Add(textBox_topic);
            Controls.Add(textBox_BrokerUrl);
            Name = "MqttSourceControl";
            Size = new Size(241, 93);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox_BrokerUrl;
        private TextBox textBox_topic;
        private Button btnConnect;
    }
}
