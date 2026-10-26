namespace LogViewer.Forms
{
    partial class GapAnalysisForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelTop = new Panel();
            labelMaxGap = new Label();
            numericUpDownMaxGap = new NumericUpDown();
            buttonOpen = new Button();
            labelSummary = new Label();
            labelFolder = new Label();
            splitContainerMain = new SplitContainer();
            splitContainerScope = new SplitContainer();
            scopeView1 = new FormsLib.Scope.Controls.ScopeView();
            cursorsView1 = new FormsLib.Scope.Controls.CursorsView();
            traceView1 = new FormsLib.Scope.Controls.TraceView();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMaxGap).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerScope).BeginInit();
            splitContainerScope.Panel1.SuspendLayout();
            splitContainerScope.Panel2.SuspendLayout();
            splitContainerScope.SuspendLayout();
            SuspendLayout();
            //
            // panelTop
            //
            panelTop.Controls.Add(labelMaxGap);
            panelTop.Controls.Add(numericUpDownMaxGap);
            panelTop.Controls.Add(buttonOpen);
            panelTop.Controls.Add(labelSummary);
            panelTop.Controls.Add(labelFolder);
            panelTop.Dock = DockStyle.Top;
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1264, 40);
            panelTop.TabIndex = 0;
            //
            // labelMaxGap
            //
            labelMaxGap.AutoSize = true;
            labelMaxGap.Location = new Point(12, 13);
            labelMaxGap.Name = "labelMaxGap";
            labelMaxGap.Text = "Gap longer than (hours)";
            //
            // numericUpDownMaxGap
            //
            numericUpDownMaxGap.Location = new Point(160, 10);
            numericUpDownMaxGap.Maximum = new decimal(new int[] { 720, 0, 0, 0 });
            numericUpDownMaxGap.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownMaxGap.Name = "numericUpDownMaxGap";
            numericUpDownMaxGap.Size = new Size(60, 23);
            numericUpDownMaxGap.TabIndex = 0;
            numericUpDownMaxGap.Value = new decimal(new int[] { 5, 0, 0, 0 });
            //
            // buttonOpen
            //
            buttonOpen.Location = new Point(235, 8);
            buttonOpen.Name = "buttonOpen";
            buttonOpen.Size = new Size(100, 27);
            buttonOpen.TabIndex = 1;
            buttonOpen.Text = "Open saved…";
            buttonOpen.UseVisualStyleBackColor = true;
            //
            // labelSummary
            //
            labelSummary.AutoSize = true;
            labelSummary.Location = new Point(350, 13);
            labelSummary.Name = "labelSummary";
            labelSummary.Text = "No houses";
            //
            // labelFolder
            //
            labelFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelFolder.AutoEllipsis = true;
            labelFolder.ForeColor = SystemColors.GrayText;
            labelFolder.Location = new Point(560, 13);
            labelFolder.Name = "labelFolder";
            labelFolder.Size = new Size(692, 15);
            //
            // splitContainerMain
            //
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.FixedPanel = FixedPanel.Panel2;
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Panel1.Controls.Add(splitContainerScope);
            splitContainerMain.Panel2.Controls.Add(traceView1);
            splitContainerMain.Size = new Size(1264, 641);
            splitContainerMain.SplitterDistance = 1000;
            splitContainerMain.TabIndex = 1;
            //
            // splitContainerScope
            //
            splitContainerScope.Dock = DockStyle.Fill;
            splitContainerScope.FixedPanel = FixedPanel.Panel2;
            splitContainerScope.Name = "splitContainerScope";
            splitContainerScope.Orientation = Orientation.Horizontal;
            splitContainerScope.Panel1.Controls.Add(scopeView1);
            splitContainerScope.Panel2.Controls.Add(cursorsView1);
            splitContainerScope.Size = new Size(1000, 641);
            splitContainerScope.SplitterDistance = 565;
            splitContainerScope.TabIndex = 0;
            //
            // scopeView1
            //
            scopeView1.BorderStyle = BorderStyle.FixedSingle;
            scopeView1.Dock = DockStyle.Fill;
            scopeView1.Name = "scopeView1";
            scopeView1.TabIndex = 0;
            //
            // cursorsView1
            //
            cursorsView1.Dock = DockStyle.Fill;
            cursorsView1.Name = "cursorsView1";
            cursorsView1.TabIndex = 0;
            //
            // traceView1
            //
            traceView1.Dock = DockStyle.Fill;
            traceView1.Name = "traceView1";
            traceView1.TabIndex = 0;
            //
            // GapAnalysisForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 681);
            Controls.Add(splitContainerMain);
            Controls.Add(panelTop);
            MinimumSize = new Size(700, 400);
            StartPosition = FormStartPosition.CenterScreen;
            Name = "GapAnalysisForm";
            Text = "Gap analysis";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMaxGap).EndInit();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainerScope.Panel1.ResumeLayout(false);
            splitContainerScope.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerScope).EndInit();
            splitContainerScope.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Panel panelTop;
        private Label labelMaxGap;
        private NumericUpDown numericUpDownMaxGap;
        private Button buttonOpen;
        private Label labelSummary;
        private Label labelFolder;
        private SplitContainer splitContainerMain;
        private SplitContainer splitContainerScope;
        private FormsLib.Scope.Controls.ScopeView scopeView1;
        private FormsLib.Scope.Controls.CursorsView cursorsView1;
        private FormsLib.Scope.Controls.TraceView traceView1;
    }
}
