namespace Owl
{
    partial class BookOpen
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
            this.BookListPanel = new System.Windows.Forms.Panel();
            this.BookList = new System.Windows.Forms.ListBox();
            this.DialogPanel = new System.Windows.Forms.Panel();
            this.deleteButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.openCancelButton = new System.Windows.Forms.Button();
            this.BookListPanel.SuspendLayout();
            this.DialogPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BookListPanel
            // 
            this.BookListPanel.AutoScroll = true;
            this.BookListPanel.Controls.Add(this.BookList);
            this.BookListPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.BookListPanel.Location = new System.Drawing.Point(0, 0);
            this.BookListPanel.Name = "BookListPanel";
            this.BookListPanel.Padding = new System.Windows.Forms.Padding(5);
            this.BookListPanel.Size = new System.Drawing.Size(322, 165);
            this.BookListPanel.TabIndex = 0;
            // 
            // BookList
            // 
            this.BookList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BookList.FormattingEnabled = true;
            this.BookList.Location = new System.Drawing.Point(5, 5);
            this.BookList.Name = "BookList";
            this.BookList.Size = new System.Drawing.Size(312, 155);
            this.BookList.TabIndex = 0;
            this.BookList.SelectedIndexChanged += new System.EventHandler(this.BookListSelectedIndexChanged);
            this.BookList.DoubleClick += new System.EventHandler(this.BookListDoubleClick);
            // 
            // DialogPanel
            // 
            this.DialogPanel.Controls.Add(this.deleteButton);
            this.DialogPanel.Controls.Add(this.okButton);
            this.DialogPanel.Controls.Add(this.openCancelButton);
            this.DialogPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.DialogPanel.Location = new System.Drawing.Point(0, 168);
            this.DialogPanel.Name = "DialogPanel";
            this.DialogPanel.Size = new System.Drawing.Size(322, 49);
            this.DialogPanel.TabIndex = 1;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(205, 13);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Удалить";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButtonClick);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(43, 13);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "Ок";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // openCancelButton
            // 
            this.openCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.openCancelButton.Location = new System.Drawing.Point(124, 13);
            this.openCancelButton.Name = "openCancelButton";
            this.openCancelButton.Size = new System.Drawing.Size(75, 23);
            this.openCancelButton.TabIndex = 0;
            this.openCancelButton.Text = "Отмена";
            this.openCancelButton.UseVisualStyleBackColor = true;
            this.openCancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // BookOpen
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.openCancelButton;
            this.ClientSize = new System.Drawing.Size(322, 217);
            this.Controls.Add(this.DialogPanel);
            this.Controls.Add(this.BookListPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BookOpen";
            this.ShowInTaskbar = false;
            this.Text = "Открыть книгу";
            this.Load += new System.EventHandler(this.BookOpenLoad);
            this.BookListPanel.ResumeLayout(false);
            this.DialogPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BookListPanel;
        private System.Windows.Forms.ListBox BookList;
        private System.Windows.Forms.Panel DialogPanel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button openCancelButton;
        private System.Windows.Forms.Button deleteButton;
    }
}