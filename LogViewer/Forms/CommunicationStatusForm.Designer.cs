namespace LogViewer.Forms
{
    partial class CommunicationStatusForm
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
            labelFilter = new Label();
            textBoxFilter = new TextBox();
            labelLookBack = new Label();
            numericUpDownLookBack = new NumericUpDown();
            comboBoxLookBackUnit = new ComboBox();
            buttonStart = new Button();
            buttonGaps = new Button();
            labelStatus = new Label();
            listViewHouses = new ListView();
            columnHouse = new ColumnHeader();
            columnGateway = new ColumnHeader();
            columnSntp = new ColumnHeader();
            columnThermostat = new ColumnHeader();
            columnIndoorUnit = new ColumnHeader();
            columnAnyLog = new ColumnHeader();
            columnStatus = new ColumnHeader();
            progressBar1 = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)numericUpDownLookBack).BeginInit();
            SuspendLayout();
            //
            // labelOrg
            //
            labelOrg.AutoSize = true;
            labelOrg.Location = new Point(12, 15);
            labelOrg.Name = "labelOrg";
            labelOrg.Text = "Organisation";
            //
            // comboBoxOrganisations
            //
            comboBoxOrganisations.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOrganisations.FormattingEnabled = true;
            comboBoxOrganisations.Location = new Point(100, 12);
            comboBoxOrganisations.Name = "comboBoxOrganisations";
            comboBoxOrganisations.Size = new Size(250, 23);
            comboBoxOrganisations.TabIndex = 0;
            //
            // labelResort
            //
            labelResort.AutoSize = true;
            labelResort.Location = new Point(370, 15);
            labelResort.Name = "labelResort";
            labelResort.Text = "Resort";
            //
            // comboBoxResorts
            //
            comboBoxResorts.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxResorts.FormattingEnabled = true;
            comboBoxResorts.Location = new Point(420, 12);
            comboBoxResorts.Name = "comboBoxResorts";
            comboBoxResorts.Size = new Size(250, 23);
            comboBoxResorts.TabIndex = 1;
            //
            // labelFilter
            //
            labelFilter.AutoSize = true;
            labelFilter.Location = new Point(12, 47);
            labelFilter.Name = "labelFilter";
            labelFilter.Text = "Name filter";
            //
            // textBoxFilter
            //
            textBoxFilter.Location = new Point(100, 44);
            textBoxFilter.Name = "textBoxFilter";
            textBoxFilter.Size = new Size(250, 23);
            textBoxFilter.TabIndex = 2;
            textBoxFilter.Text = "Golfbreker";
            //
            // labelLookBack
            //
            labelLookBack.AutoSize = true;
            labelLookBack.Location = new Point(370, 47);
            labelLookBack.Name = "labelLookBack";
            labelLookBack.Text = "Look back";
            //
            // numericUpDownLookBack
            //
            numericUpDownLookBack.Location = new Point(440, 44);
            numericUpDownLookBack.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numericUpDownLookBack.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownLookBack.Name = "numericUpDownLookBack";
            numericUpDownLookBack.Size = new Size(70, 23);
            numericUpDownLookBack.TabIndex = 3;
            numericUpDownLookBack.Value = new decimal(new int[] { 7, 0, 0, 0 });
            //
            // comboBoxLookBackUnit
            //
            comboBoxLookBackUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxLookBackUnit.FormattingEnabled = true;
            comboBoxLookBackUnit.Items.AddRange(new object[] { "Days", "Hours" });
            comboBoxLookBackUnit.Location = new Point(516, 44);
            comboBoxLookBackUnit.Name = "comboBoxLookBackUnit";
            comboBoxLookBackUnit.Size = new Size(70, 23);
            comboBoxLookBackUnit.TabIndex = 4;
            //
            // buttonStart
            //
            buttonStart.Location = new Point(595, 42);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(75, 27);
            buttonStart.TabIndex = 5;
            buttonStart.Text = "Start";
            buttonStart.UseVisualStyleBackColor = true;
            //
            // buttonGaps
            //
            buttonGaps.Location = new Point(676, 42);
            buttonGaps.Name = "buttonGaps";
            buttonGaps.Size = new Size(95, 27);
            buttonGaps.TabIndex = 6;
            buttonGaps.Text = "Gap analysis";
            buttonGaps.UseVisualStyleBackColor = true;
            //
            // labelStatus
            //
            labelStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelStatus.AutoEllipsis = true;
            labelStatus.Location = new Point(780, 47);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(192, 15);
            //
            // listViewHouses
            //
            listViewHouses.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewHouses.Columns.AddRange(new ColumnHeader[] { columnHouse, columnGateway, columnSntp, columnThermostat, columnIndoorUnit, columnAnyLog, columnStatus });
            listViewHouses.FullRowSelect = true;
            listViewHouses.GridLines = true;
            listViewHouses.Location = new Point(12, 78);
            listViewHouses.MultiSelect = false;
            listViewHouses.Name = "listViewHouses";
            listViewHouses.Size = new Size(960, 440);
            listViewHouses.TabIndex = 7;
            listViewHouses.UseCompatibleStateImageBehavior = false;
            listViewHouses.View = View.Details;
            //
            // columns
            //
            columnHouse.Text = "House";
            columnHouse.Width = 160;
            columnGateway.Text = "Gateway";
            columnGateway.Width = 140;
            columnSntp.Text = "Last SNTP";
            columnSntp.Width = 125;
            columnThermostat.Text = "Last thermostat temp";
            columnThermostat.Width = 135;
            columnIndoorUnit.Text = "Last indoor unit temp";
            columnIndoorUnit.Width = 135;
            columnAnyLog.Text = "Last log (any)";
            columnAnyLog.Width = 125;
            columnStatus.Text = "Status";
            columnStatus.Width = 200;
            //
            // progressBar1
            //
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(12, 526);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(960, 10);
            progressBar1.TabIndex = 8;
            //
            // CommunicationStatusForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 548);
            Controls.Add(progressBar1);
            Controls.Add(listViewHouses);
            Controls.Add(labelStatus);
            Controls.Add(buttonGaps);
            Controls.Add(buttonStart);
            Controls.Add(comboBoxLookBackUnit);
            Controls.Add(numericUpDownLookBack);
            Controls.Add(labelLookBack);
            Controls.Add(textBoxFilter);
            Controls.Add(labelFilter);
            Controls.Add(comboBoxResorts);
            Controls.Add(labelResort);
            Controls.Add(comboBoxOrganisations);
            Controls.Add(labelOrg);
            MinimumSize = new Size(700, 300);
            StartPosition = FormStartPosition.CenterScreen;
            Name = "CommunicationStatusForm";
            Text = "Communication status";
            ((System.ComponentModel.ISupportInitialize)numericUpDownLookBack).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label labelOrg;
        private ComboBox comboBoxOrganisations;
        private Label labelResort;
        private ComboBox comboBoxResorts;
        private Label labelFilter;
        private TextBox textBoxFilter;
        private Label labelLookBack;
        private NumericUpDown numericUpDownLookBack;
        private ComboBox comboBoxLookBackUnit;
        private Button buttonStart;
        private Button buttonGaps;
        private Label labelStatus;
        private ListView listViewHouses;
        private ColumnHeader columnHouse;
        private ColumnHeader columnGateway;
        private ColumnHeader columnSntp;
        private ColumnHeader columnThermostat;
        private ColumnHeader columnIndoorUnit;
        private ColumnHeader columnAnyLog;
        private ColumnHeader columnStatus;
        private ProgressBar progressBar1;
    }
}
