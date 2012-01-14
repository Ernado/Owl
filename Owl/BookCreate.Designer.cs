namespace Owl
{
    partial class BookCreate
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.OkButton = new System.Windows.Forms.Button();
            this.CreateCancelButton = new System.Windows.Forms.Button();
            this.BookNameBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BookFolderBox = new System.Windows.Forms.TextBox();
            this.infoToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Название:";
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(37, 98);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 1;
            this.OkButton.Text = "Ok";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkClick);
            // 
            // CreateCancelButton
            // 
            this.CreateCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CreateCancelButton.Location = new System.Drawing.Point(118, 98);
            this.CreateCancelButton.Name = "CreateCancelButton";
            this.CreateCancelButton.Size = new System.Drawing.Size(75, 23);
            this.CreateCancelButton.TabIndex = 2;
            this.CreateCancelButton.Text = "Отмена";
            this.CreateCancelButton.UseVisualStyleBackColor = true;
            this.CreateCancelButton.Click += new System.EventHandler(this.CreateCancelButtonClick);
            // 
            // BookNameBox
            // 
            this.BookNameBox.Location = new System.Drawing.Point(101, 18);
            this.BookNameBox.Name = "BookNameBox";
            this.BookNameBox.Size = new System.Drawing.Size(100, 20);
            this.BookNameBox.TabIndex = 3;
            this.BookNameBox.TextChanged += new System.EventHandler(this.BookNameBoxTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Папка:";
            // 
            // BookFolderBox
            // 
            this.BookFolderBox.Location = new System.Drawing.Point(101, 50);
            this.BookFolderBox.Name = "BookFolderBox";
            this.BookFolderBox.Size = new System.Drawing.Size(100, 20);
            this.BookFolderBox.TabIndex = 5;
            this.BookFolderBox.TextChanged += new System.EventHandler(this.BookFolderBoxTextChanged);
            // 
            // BookCreate
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CreateCancelButton;
            this.ClientSize = new System.Drawing.Size(237, 138);
            this.Controls.Add(this.BookFolderBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BookNameBox);
            this.Controls.Add(this.CreateCancelButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BookCreate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Новый документ:";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CreateCancelButton;
        private System.Windows.Forms.TextBox BookNameBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox BookFolderBox;
        private System.Windows.Forms.ToolTip infoToolTip;
    }
}