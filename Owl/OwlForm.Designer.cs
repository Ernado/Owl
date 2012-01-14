namespace Owl
{
    partial class OwlForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.imageOutput = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageOutput)).BeginInit();
            this.SuspendLayout();
            // 
            // imageOutput
            // 
            this.imageOutput.Location = new System.Drawing.Point(0, 2);
            this.imageOutput.Name = "imageOutput";
            this.imageOutput.Size = new System.Drawing.Size(682, 409);
            this.imageOutput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imageOutput.TabIndex = 2;
            this.imageOutput.TabStop = false;
            this.imageOutput.Click += new System.EventHandler(this.imageOutput_Click);
            // 
            // OwlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 412);
            this.Controls.Add(this.imageOutput);
            this.Name = "OwlForm";
            this.Text = "Owl 0.1";
            this.Load += new System.EventHandler(this.OwlFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.imageOutput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imageOutput;



    }
}

