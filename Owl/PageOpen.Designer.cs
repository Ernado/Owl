namespace Owl
{
    partial class PageOpen
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
            this.PageList = new System.Windows.Forms.ListBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.DialogPanel = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.BookListPanel = new System.Windows.Forms.Panel();
            this.DialogPanel.SuspendLayout();
            this.BookListPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PageList
            // 
            this.PageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PageList.FormattingEnabled = true;
            this.PageList.Location = new System.Drawing.Point(5, 5);
            this.PageList.Name = "PageList";
            this.PageList.Size = new System.Drawing.Size(302, 155);
            this.PageList.TabIndex = 0;
            this.PageList.SelectedIndexChanged += new System.EventHandler(this.PageListSelectedIndexChanged);
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(200, 9);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Удалить";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButtonClick);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(38, 9);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "Ок";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // DialogPanel
            // 
            this.DialogPanel.Controls.Add(this.deleteButton);
            this.DialogPanel.Controls.Add(this.okButton);
            this.DialogPanel.Controls.Add(this.cancelButton);
            this.DialogPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.DialogPanel.Location = new System.Drawing.Point(0, 166);
            this.DialogPanel.Name = "DialogPanel";
            this.DialogPanel.Size = new System.Drawing.Size(312, 41);
            this.DialogPanel.TabIndex = 3;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(119, 9);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // BookListPanel
            // 
            this.BookListPanel.AutoScroll = true;
            this.BookListPanel.Controls.Add(this.PageList);
            this.BookListPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.BookListPanel.Location = new System.Drawing.Point(0, 0);
            this.BookListPanel.Name = "BookListPanel";
            this.BookListPanel.Padding = new System.Windows.Forms.Padding(5);
            this.BookListPanel.Size = new System.Drawing.Size(312, 165);
            this.BookListPanel.TabIndex = 2;
            // 
            // PageOpen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 207);
            this.Controls.Add(this.DialogPanel);
            this.Controls.Add(this.BookListPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PageOpen";
            this.ShowInTaskbar = false;
            this.Text = "Открыть страницу";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PageOpenFormClosed);
            this.Shown += new System.EventHandler(this.PageOpenShown);
            this.DialogPanel.ResumeLayout(false);
            this.BookListPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox PageList;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Panel DialogPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel BookListPanel;
    }
}