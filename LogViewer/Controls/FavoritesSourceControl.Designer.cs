namespace LogViewer.Controls
{
    partial class FavoritesSourceControl
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
            buttonCancel = new Button();
            buttonFetch = new Button();
            labelUntill = new Label();
            labelFrom = new Label();
            dateTimePickerUntill = new DateTimePicker();
            dateTimePickerFrom = new DateTimePicker();
            listBoxFavorites = new ListBox();
            progressBarFetch = new ProgressBar();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(167, 391);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(65, 23);
            buttonCancel.TabIndex = 35;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonFetch
            // 
            buttonFetch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonFetch.Location = new Point(167, 362);
            buttonFetch.Name = "buttonFetch";
            buttonFetch.Size = new Size(65, 23);
            buttonFetch.TabIndex = 34;
            buttonFetch.Text = "Fetch";
            buttonFetch.UseVisualStyleBackColor = true;
            // 
            // labelUntill
            // 
            labelUntill.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelUntill.AutoSize = true;
            labelUntill.Location = new Point(1, 399);
            labelUntill.Name = "labelUntill";
            labelUntill.Size = new Size(35, 15);
            labelUntill.TabIndex = 33;
            labelUntill.Text = "Untill";
            // 
            // labelFrom
            // 
            labelFrom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelFrom.AutoSize = true;
            labelFrom.Location = new Point(1, 370);
            labelFrom.Name = "labelFrom";
            labelFrom.Size = new Size(35, 15);
            labelFrom.TabIndex = 32;
            labelFrom.Text = "From";
            // 
            // dateTimePickerUntill
            // 
            dateTimePickerUntill.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dateTimePickerUntill.CustomFormat = "yyyy-MM-dd HH:mm";
            dateTimePickerUntill.Format = DateTimePickerFormat.Custom;
            dateTimePickerUntill.Location = new Point(64, 393);
            dateTimePickerUntill.Name = "dateTimePickerUntill";
            dateTimePickerUntill.Size = new Size(97, 23);
            dateTimePickerUntill.TabIndex = 31;
            // 
            // dateTimePickerFrom
            // 
            dateTimePickerFrom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dateTimePickerFrom.CustomFormat = "yyyy-MM-dd HH:mm";
            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.Location = new Point(64, 364);
            dateTimePickerFrom.Name = "dateTimePickerFrom";
            dateTimePickerFrom.Size = new Size(97, 23);
            dateTimePickerFrom.TabIndex = 30;
            // 
            // listBoxFavorites
            // 
            listBoxFavorites.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxFavorites.FormattingEnabled = true;
            listBoxFavorites.IntegralHeight = false;
            listBoxFavorites.ItemHeight = 15;
            listBoxFavorites.Location = new Point(3, 3);
            listBoxFavorites.Name = "listBoxFavorites";
            listBoxFavorites.Size = new Size(230, 353);
            listBoxFavorites.TabIndex = 29;
            // 
            // progressBarFetch
            // 
            progressBarFetch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBarFetch.Location = new Point(3, 422);
            progressBarFetch.Name = "progressBarFetch";
            progressBarFetch.Size = new Size(229, 5);
            progressBarFetch.TabIndex = 36;
            // 
            // FavoritesSourceControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(progressBarFetch);
            Controls.Add(buttonCancel);
            Controls.Add(buttonFetch);
            Controls.Add(labelUntill);
            Controls.Add(labelFrom);
            Controls.Add(dateTimePickerUntill);
            Controls.Add(dateTimePickerFrom);
            Controls.Add(listBoxFavorites);
            Name = "FavoritesSourceControl";
            Size = new Size(236, 430);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonCancel;
        private Button buttonFetch;
        private Label labelUntill;
        private Label labelFrom;
        private DateTimePicker dateTimePickerUntill;
        private DateTimePicker dateTimePickerFrom;
        private ListBox listBoxFavorites;
        private ProgressBar progressBarFetch;
    }
}
