namespace Owl
{
    partial class PageCreate
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
            this.label2 = new System.Windows.Forms.Label();
            this.CreateCancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.imageLoader = new System.Windows.Forms.Button();
            this.pageNumberInput = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.pageNumberInput)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Изображение:";
            // 
            // CreateCancelButton
            // 
            this.CreateCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CreateCancelButton.Location = new System.Drawing.Point(135, 98);
            this.CreateCancelButton.Name = "CreateCancelButton";
            this.CreateCancelButton.Size = new System.Drawing.Size(75, 23);
            this.CreateCancelButton.TabIndex = 8;
            this.CreateCancelButton.Text = "Отмена";
            this.CreateCancelButton.UseVisualStyleBackColor = true;
            this.CreateCancelButton.Click += new System.EventHandler(this.CreateCancelButtonClick);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(54, 98);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Номер:";
            // 
            // imageLoader
            // 
            this.imageLoader.Location = new System.Drawing.Point(128, 53);
            this.imageLoader.Name = "imageLoader";
            this.imageLoader.Size = new System.Drawing.Size(100, 20);
            this.imageLoader.TabIndex = 12;
            this.imageLoader.Text = "Загрузить";
            this.imageLoader.UseVisualStyleBackColor = true;
            this.imageLoader.Click += new System.EventHandler(this.ImageLoaderClick);
            // 
            // pageNumberInput
            // 
            this.pageNumberInput.Location = new System.Drawing.Point(128, 18);
            this.pageNumberInput.Maximum = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            this.pageNumberInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.pageNumberInput.Name = "pageNumberInput";
            this.pageNumberInput.Size = new System.Drawing.Size(100, 20);
            this.pageNumberInput.TabIndex = 13;
            this.pageNumberInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.pageNumberInput.ValueChanged += new System.EventHandler(this.PageNumberInputTextChanged);
            // 
            // PageCreate
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CreateCancelButton;
            this.ClientSize = new System.Drawing.Size(265, 138);
            this.Controls.Add(this.pageNumberInput);
            this.Controls.Add(this.imageLoader);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CreateCancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PageCreate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Новая страница:";
            this.Load += new System.EventHandler(this.PageCreateLoad);
            ((System.ComponentModel.ISupportInitialize)(this.pageNumberInput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button CreateCancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button imageLoader;
        private System.Windows.Forms.NumericUpDown pageNumberInput;
    }
}