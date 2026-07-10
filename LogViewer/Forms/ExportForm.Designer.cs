namespace LogViewer.Forms
{
    partial class ExportForm
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
            labelOrg = new Label();
            comboBoxOrganisations = new ComboBox();
            labelResort = new Label();
            comboBoxResorts = new ComboBox();
            buttonExportResort = new Button();
            buttonExportAll = new Button();
            buttonCancel = new Button();
            progressBar1 = new ProgressBar();
            SuspendLayout();
            //
            // labelOrg
            //
            labelOrg.AutoSize = true;
            labelOrg.Location = new Point(12, 15);
            labelOrg.Name = "labelOrg";
            labelOrg.Size = new Size(79, 15);
            labelOrg.Text = "Organisation";
            //
            // comboBoxOrganisations
            //
            comboBoxOrganisations.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxOrganisations.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOrganisations.FormattingEnabled = true;
            comboBoxOrganisations.Location = new Point(12, 33);
            comboBoxOrganisations.Name = "comboBoxOrganisations";
            comboBoxOrganisations.Size = new Size(360, 23);
            comboBoxOrganisations.TabIndex = 0;
            //
            // labelResort
            //
            labelResort.AutoSize = true;
            labelResort.Location = new Point(12, 66);
            labelResort.Name = "labelResort";
            labelResort.Size = new Size(42, 15);
            labelResort.Text = "Resort";
            //
            // comboBoxResorts
            //
            comboBoxResorts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxResorts.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxResorts.FormattingEnabled = true;
            comboBoxResorts.Location = new Point(12, 84);
            comboBoxResorts.Name = "comboBoxResorts";
            comboBoxResorts.Size = new Size(360, 23);
            comboBoxResorts.TabIndex = 1;
            //
            // buttonExportResort
            //
            buttonExportResort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExportResort.Location = new Point(12, 123);
            buttonExportResort.Name = "buttonExportResort";
            buttonExportResort.Size = new Size(110, 27);
            buttonExportResort.TabIndex = 2;
            buttonExportResort.Text = "Export resort";
            buttonExportResort.UseVisualStyleBackColor = true;
            //
            // buttonExportAll
            //
            buttonExportAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExportAll.Location = new Point(128, 123);
            buttonExportAll.Name = "buttonExportAll";
            buttonExportAll.Size = new Size(110, 27);
            buttonExportAll.TabIndex = 3;
            buttonExportAll.Text = "Export all";
            buttonExportAll.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCancel.Location = new Point(262, 123);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(110, 27);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // progressBar1
            //
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(12, 162);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(360, 8);
            progressBar1.TabIndex = 5;
            //
            // ExportForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 182);
            Controls.Add(progressBar1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonExportAll);
            Controls.Add(buttonExportResort);
            Controls.Add(comboBoxResorts);
            Controls.Add(labelResort);
            Controls.Add(comboBoxOrganisations);
            Controls.Add(labelOrg);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Name = "ExportForm";
            Text = "Export for KC220 tool";
            ResumeLayout(false);
            PerformLayout();
        }

        private Label labelOrg;
        private ComboBox comboBoxOrganisations;
        private Label labelResort;
        private ComboBox comboBoxResorts;
        private Button buttonExportResort;
        private Button buttonExportAll;
        private Button buttonCancel;
        private ProgressBar progressBar1;
    }
}
