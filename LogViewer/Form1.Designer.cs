using LogViewer.AppContext;
using LogViewer.Config.Models;

namespace LogViewer
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
            FormsLib.Scope.ScopeController scopeController1 = new FormsLib.Scope.ScopeController();
            Logging.LogCollection logCollection1 = new Logging.LogCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            ProfileConfig profileConfig1 = new ProfileConfig();
            UnhandeledConfig unhandeledConfig1 = new UnhandeledConfig();
            ScopeViewContext scopeViewContext1 = new ScopeViewContext();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            scopeView1 = new FormsLib.Scope.Controls.ScopeView();
            menuStrip1 = new MenuStrip();
            markerView1 = new FormsLib.Scope.Controls.CursorsView();
            tabControl1 = new TabControl();
            tabPageApi = new TabPage();
            apiSourceControl1 = new Controls.ApiSourceControl(hybridCache);
            tabPageTraces = new TabPage();
            traceView1 = new FormsLib.Scope.Controls.TraceView();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPageApi.SuspendLayout();
            tabPageTraces.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Size = new Size(1904, 1019);
            splitContainer1.SplitterDistance = 1604;
            splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.FixedPanel = FixedPanel.Panel2;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(scopeView1);
            splitContainer2.Panel1.Controls.Add(menuStrip1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(markerView1);
            splitContainer2.Size = new Size(1604, 1019);
            splitContainer2.SplitterDistance = 942;
            splitContainer2.TabIndex = 0;
            // 
            // scopeView1
            // 
            scopeView1.BackgroundImageLayout = ImageLayout.Stretch;
            scopeView1.BorderStyle = BorderStyle.FixedSingle;
            scopeView1.DataSource = scopeController1;
            scopeView1.Dock = DockStyle.Fill;
            scopeView1.Location = new Point(0, 24);
            scopeView1.Margin = new Padding(4, 3, 4, 3);
            scopeView1.Name = "scopeView1";
            scopeView1.Size = new Size(1604, 918);
            scopeView1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(32, 32);
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1604, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // markerView1
            // 
            markerView1.DataSource = null;
            markerView1.Dock = DockStyle.Fill;
            markerView1.Location = new Point(0, 0);
            markerView1.Name = "markerView1";
            markerView1.Size = new Size(1604, 73);
            markerView1.TabIndex = 0;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageApi);
            tabControl1.Controls.Add(tabPageTraces);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(296, 1019);
            tabControl1.TabIndex = 0;
            // 
            // tabPageApi
            // 
            tabPageApi.Controls.Add(apiSourceControl1);
            tabPageApi.Location = new Point(4, 24);
            tabPageApi.Name = "tabPageApi";
            tabPageApi.Padding = new Padding(3);
            tabPageApi.Size = new Size(288, 991);
            tabPageApi.TabIndex = 1;
            tabPageApi.Text = "WebAPI";
            tabPageApi.UseVisualStyleBackColor = true;
            // 
            // apiSourceControl1
            // 
            profileConfig1.Name = null;
            profileConfig1.Traces = (Dictionary<string, TraceConfig>)resources.GetObject("profileConfig1.Traces");
            unhandeledConfig1.NameKey = "LogCode";
            unhandeledConfig1.Offset = 0;
            profileConfig1.Unhandeled = unhandeledConfig1;
            scopeViewContext1.EndDate = new DateTime(0L);
            scopeViewContext1.StartDate = new DateTime(0L);
            apiSourceControl1.Dock = DockStyle.Fill;
            apiSourceControl1.Location = new Point(3, 3);
            apiSourceControl1.Margin = new Padding(6);
            apiSourceControl1.Name = "apiSourceControl1";
            apiSourceControl1.Size = new Size(282, 985);
            apiSourceControl1.TabIndex = 1;
            // 
            // tabPageTraces
            // 
            tabPageTraces.Controls.Add(traceView1);
            tabPageTraces.Location = new Point(4, 24);
            tabPageTraces.Name = "tabPageTraces";
            tabPageTraces.Padding = new Padding(3);
            tabPageTraces.Size = new Size(288, 991);
            tabPageTraces.TabIndex = 0;
            tabPageTraces.Text = "Traces";
            tabPageTraces.UseVisualStyleBackColor = true;
            // 
            // traceView1
            // 
            traceView1.DataSource = null;
            traceView1.Dock = DockStyle.Fill;
            traceView1.Location = new Point(3, 3);
            traceView1.Margin = new Padding(4, 3, 4, 3);
            traceView1.Name = "traceView1";
            traceView1.Size = new Size(282, 985);
            traceView1.TabIndex = 0;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 1019);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1904, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(118, 17);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1904, 1041);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Name = "Form1";
            Text = "Form1";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPageApi.ResumeLayout(false);
            tabPageTraces.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private FormsLib.Scope.Controls.ScopeView scopeView1;
        private MenuStrip menuStrip1;
        private FormsLib.Scope.Controls.CursorsView markerView1;
        private TabControl tabControl1;
        private TabPage tabPageTraces;
        private FormsLib.Scope.Controls.TraceView traceView1;
        private TabPage tabPageApi;
        private Controls.ApiSourceControl apiSourceControl1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
    }
}