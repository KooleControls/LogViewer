namespace LogViewer.Controls
{
    partial class TcpSourceControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Cleanup();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            lblHost = new Label();
            txtHost = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            chkEncryption = new CheckBox();
            lblLookBack = new Label();
            numLookBack = new NumericUpDown();
            btnConnect = new Button();
            ((System.ComponentModel.ISupportInitialize)numLookBack).BeginInit();
            SuspendLayout();
            //
            // lblHost
            //
            lblHost.AutoSize = true;
            lblHost.Location = new Point(8, 12);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(35, 15);
            lblHost.TabIndex = 0;
            lblHost.Text = "Host";
            //
            // txtHost
            //
            txtHost.Location = new Point(8, 30);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(260, 23);
            txtHost.TabIndex = 1;
            //
            // lblPort
            //
            lblPort.AutoSize = true;
            lblPort.Location = new Point(8, 60);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(29, 15);
            lblPort.TabIndex = 2;
            lblPort.Text = "Port";
            //
            // txtPort
            //
            txtPort.Location = new Point(8, 78);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(260, 23);
            txtPort.TabIndex = 3;
            txtPort.Text = "31700";
            //
            // chkEncryption
            //
            chkEncryption.AutoSize = true;
            chkEncryption.Checked = true;
            chkEncryption.CheckState = CheckState.Checked;
            chkEncryption.Location = new Point(8, 112);
            chkEncryption.Name = "chkEncryption";
            chkEncryption.Size = new Size(89, 19);
            chkEncryption.TabIndex = 4;
            chkEncryption.Text = "Encryption";
            chkEncryption.UseVisualStyleBackColor = true;
            //
            // lblLookBack
            //
            lblLookBack.AutoSize = true;
            lblLookBack.Location = new Point(8, 142);
            lblLookBack.Name = "lblLookBack";
            lblLookBack.Size = new Size(96, 15);
            lblLookBack.TabIndex = 5;
            lblLookBack.Text = "Look back (lines)";
            //
            // numLookBack
            //
            numLookBack.Location = new Point(8, 160);
            numLookBack.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numLookBack.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLookBack.Name = "numLookBack";
            numLookBack.Size = new Size(260, 23);
            numLookBack.TabIndex = 6;
            numLookBack.Value = new decimal(new int[] { 250, 0, 0, 0 });
            //
            // btnConnect
            //
            btnConnect.Location = new Point(8, 196);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(260, 30);
            btnConnect.TabIndex = 7;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            //
            // TcpSourceControl
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnConnect);
            Controls.Add(numLookBack);
            Controls.Add(lblLookBack);
            Controls.Add(chkEncryption);
            Controls.Add(txtPort);
            Controls.Add(lblPort);
            Controls.Add(txtHost);
            Controls.Add(lblHost);
            Name = "TcpSourceControl";
            Size = new Size(282, 985);
            ((System.ComponentModel.ISupportInitialize)numLookBack).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblHost;
        private TextBox txtHost;
        private Label lblPort;
        private TextBox txtPort;
        private CheckBox chkEncryption;
        private Label lblLookBack;
        private NumericUpDown numLookBack;
        private Button btnConnect;
    }
}
