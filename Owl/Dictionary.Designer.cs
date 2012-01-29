namespace Owl
{
    partial class Dictionary
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.wordInformation = new System.Windows.Forms.Label();
            this.variantImage = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.wordList = new System.Windows.Forms.ListBox();
            this.variantList = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.variantImage)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.wordInformation);
            this.panel1.Controls.Add(this.variantImage);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.panel1.Size = new System.Drawing.Size(587, 88);
            this.panel1.TabIndex = 0;
            // 
            // wordInformation
            // 
            this.wordInformation.AutoSize = true;
            this.wordInformation.Location = new System.Drawing.Point(36, 32);
            this.wordInformation.Name = "wordInformation";
            this.wordInformation.Size = new System.Drawing.Size(90, 13);
            this.wordInformation.TabIndex = 1;
            this.wordInformation.Text = "Выберите слово";
            // 
            // variantImage
            // 
            this.variantImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.variantImage.Dock = System.Windows.Forms.DockStyle.Right;
            this.variantImage.Location = new System.Drawing.Point(477, 0);
            this.variantImage.Name = "variantImage";
            this.variantImage.Size = new System.Drawing.Size(100, 88);
            this.variantImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.variantImage.TabIndex = 0;
            this.variantImage.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.splitContainer1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 88);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(587, 319);
            this.panel2.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.wordList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.variantList);
            this.splitContainer1.Size = new System.Drawing.Size(587, 319);
            this.splitContainer1.SplitterDistance = 195;
            this.splitContainer1.TabIndex = 1;
            // 
            // wordList
            // 
            this.wordList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wordList.FormattingEnabled = true;
            this.wordList.Location = new System.Drawing.Point(0, 0);
            this.wordList.Name = "wordList";
            this.wordList.Size = new System.Drawing.Size(195, 319);
            this.wordList.TabIndex = 0;
            this.wordList.SelectedIndexChanged += new System.EventHandler(this.WordListSelectedIndexChanged);
            // 
            // variantList
            // 
            this.variantList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.variantList.LargeImageList = this.imageList1;
            this.variantList.Location = new System.Drawing.Point(0, 0);
            this.variantList.Name = "variantList";
            this.variantList.Size = new System.Drawing.Size(388, 319);
            this.variantList.TabIndex = 0;
            this.variantList.UseCompatibleStateImageBehavior = false;
            this.variantList.View = System.Windows.Forms.View.List;
            this.variantList.SelectedIndexChanged += new System.EventHandler(this.VariantListSelectedIndexChanged);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // Dictionary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 406);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Dictionary";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dictionary";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.variantImage)).EndInit();
            this.panel2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label wordInformation;
        private System.Windows.Forms.PictureBox variantImage;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox wordList;
        private System.Windows.Forms.ListView variantList;
        private System.Windows.Forms.ImageList imageList1;

    }
}