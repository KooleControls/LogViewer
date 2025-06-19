namespace LogViewer.Controls
{
    partial class ApiSourceControl
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
            richTextBoxInfoView = new RichTextBox();
            buttonFetch = new Button();
            label2 = new Label();
            label1 = new Label();
            dateTimePickerUntill = new DateTimePicker();
            dateTimePickerFrom = new DateTimePicker();
            listBoxObjectItems = new ListBox();
            textBoxSearch = new TextBox();
            progressBar1 = new ProgressBar();
            comboBoxGateways = new ComboBox();
            comboBoxResorts = new ComboBox();
            comboBoxOrganisations = new ComboBox();
            buttonCancel = new Button();
            buttonSearch = new Button();
            checkBoxRequireGateways = new CheckBox();
            SuspendLayout();
            // 
            // richTextBoxInfoView
            // 
            richTextBoxInfoView.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxInfoView.Font = new Font("Consolas", 9F);
            richTextBoxInfoView.Location = new Point(3, 364);
            richTextBoxInfoView.Name = "richTextBoxInfoView";
            richTextBoxInfoView.ReadOnly = true;
            richTextBoxInfoView.ScrollBars = RichTextBoxScrollBars.None;
            richTextBoxInfoView.Size = new Size(230, 63);
            richTextBoxInfoView.TabIndex = 27;
            richTextBoxInfoView.Text = "api\nhost\nheaders\ninfo";
            richTextBoxInfoView.WordWrap = false;
            // 
            // buttonFetch
            // 
            buttonFetch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonFetch.Location = new Point(168, 295);
            buttonFetch.Name = "buttonFetch";
            buttonFetch.Size = new Size(65, 23);
            buttonFetch.TabIndex = 26;
            buttonFetch.Text = "Fetch";
            buttonFetch.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(4, 330);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 25;
            label2.Text = "Untill";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(4, 301);
            label1.Name = "label1";
            label1.Size = new Size(35, 15);
            label1.TabIndex = 24;
            label1.Text = "From";
            // 
            // dateTimePickerUntill
            // 
            dateTimePickerUntill.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dateTimePickerUntill.CustomFormat = "yyyy-MM-dd HH:mm";
            dateTimePickerUntill.Format = DateTimePickerFormat.Custom;
            dateTimePickerUntill.Location = new Point(67, 324);
            dateTimePickerUntill.Name = "dateTimePickerUntill";
            dateTimePickerUntill.Size = new Size(95, 23);
            dateTimePickerUntill.TabIndex = 23;
            // 
            // dateTimePickerFrom
            // 
            dateTimePickerFrom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dateTimePickerFrom.CustomFormat = "yyyy-MM-dd HH:mm";
            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.Location = new Point(67, 295);
            dateTimePickerFrom.Name = "dateTimePickerFrom";
            dateTimePickerFrom.Size = new Size(95, 23);
            dateTimePickerFrom.TabIndex = 22;
            // 
            // listBoxObjectItems
            // 
            listBoxObjectItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxObjectItems.FormattingEnabled = true;
            listBoxObjectItems.IntegralHeight = false;
            listBoxObjectItems.ItemHeight = 15;
            listBoxObjectItems.Location = new Point(3, 120);
            listBoxObjectItems.Name = "listBoxObjectItems";
            listBoxObjectItems.Size = new Size(230, 140);
            listBoxObjectItems.TabIndex = 21;
            // 
            // textBoxSearch
            // 
            textBoxSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSearch.Location = new Point(3, 61);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.PlaceholderText = "Search";
            textBoxSearch.Size = new Size(159, 23);
            textBoxSearch.TabIndex = 20;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(4, 353);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(229, 5);
            progressBar1.TabIndex = 19;
            // 
            // comboBoxGateways
            // 
            comboBoxGateways.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxGateways.FormattingEnabled = true;
            comboBoxGateways.Location = new Point(4, 266);
            comboBoxGateways.Name = "comboBoxGateways";
            comboBoxGateways.Size = new Size(230, 23);
            comboBoxGateways.TabIndex = 17;
            // 
            // comboBoxResorts
            // 
            comboBoxResorts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxResorts.FormattingEnabled = true;
            comboBoxResorts.Location = new Point(3, 32);
            comboBoxResorts.Name = "comboBoxResorts";
            comboBoxResorts.Size = new Size(230, 23);
            comboBoxResorts.TabIndex = 16;
            // 
            // comboBoxOrganisations
            // 
            comboBoxOrganisations.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxOrganisations.FormattingEnabled = true;
            comboBoxOrganisations.Location = new Point(3, 3);
            comboBoxOrganisations.Name = "comboBoxOrganisations";
            comboBoxOrganisations.Size = new Size(230, 23);
            comboBoxOrganisations.TabIndex = 15;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(168, 324);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(65, 23);
            buttonCancel.TabIndex = 28;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSearch
            // 
            buttonSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSearch.Location = new Point(168, 60);
            buttonSearch.Name = "buttonSearch";
            buttonSearch.Size = new Size(65, 23);
            buttonSearch.TabIndex = 29;
            buttonSearch.Text = "Search";
            buttonSearch.UseVisualStyleBackColor = true;
            // 
            // checkBoxRequireGateways
            // 
            checkBoxRequireGateways.Checked = true;
            checkBoxRequireGateways.CheckState = CheckState.Checked;
            checkBoxRequireGateways.Location = new Point(4, 90);
            checkBoxRequireGateways.Name = "checkBoxRequireGateways";
            checkBoxRequireGateways.Size = new Size(229, 24);
            checkBoxRequireGateways.TabIndex = 30;
            checkBoxRequireGateways.Text = "Hide objects without gateways";
            checkBoxRequireGateways.UseVisualStyleBackColor = true;
            // 
            // ApiSourceControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(checkBoxRequireGateways);
            Controls.Add(buttonSearch);
            Controls.Add(buttonCancel);
            Controls.Add(richTextBoxInfoView);
            Controls.Add(buttonFetch);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dateTimePickerUntill);
            Controls.Add(dateTimePickerFrom);
            Controls.Add(listBoxObjectItems);
            Controls.Add(textBoxSearch);
            Controls.Add(progressBar1);
            Controls.Add(comboBoxGateways);
            Controls.Add(comboBoxResorts);
            Controls.Add(comboBoxOrganisations);
            Name = "ApiSourceControl";
            Size = new Size(236, 430);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBoxInfoView;
        private Button buttonFetch;
        private Label label2;
        private Label label1;
        private DateTimePicker dateTimePickerUntill;
        private DateTimePicker dateTimePickerFrom;
        private ListBox listBoxObjectItems;
        private TextBox textBoxSearch;
        private ProgressBar progressBar1;
        private ComboBox comboBoxGateways;
        private ComboBox comboBoxResorts;
        private ComboBox comboBoxOrganisations;
        private Button buttonCancel;
        private Button buttonSearch;
        private CheckBox checkBoxRequireGateways;
    }
}
