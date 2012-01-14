namespace Owl
{
    partial class Redactor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redactor));
            this.Status = new System.Windows.Forms.StatusStrip();
            this.Tools = new System.Windows.Forms.ToolStrip();
            this.AnalyzeButton = new System.Windows.Forms.ToolStripButton();
            this.leftToolPanel = new System.Windows.Forms.Panel();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.InterfaceContainer = new System.Windows.Forms.SplitContainer();
            this.interfaceBox = new System.Windows.Forms.PictureBox();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.docuentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenDocumentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CreateBookMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBookMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.закрытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitButton = new System.Windows.Forms.ToolStripMenuItem();
            this.PageMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenPageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CreatePageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadImageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeletePageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Tools.SuspendLayout();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InterfaceContainer)).BeginInit();
            this.InterfaceContainer.Panel1.SuspendLayout();
            this.InterfaceContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.interfaceBox)).BeginInit();
            this.MainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // Status
            // 
            this.Status.Location = new System.Drawing.Point(0, 424);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(854, 22);
            this.Status.TabIndex = 0;
            this.Status.Text = "statusStrip1";
            // 
            // Tools
            // 
            this.Tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AnalyzeButton});
            this.Tools.Location = new System.Drawing.Point(0, 24);
            this.Tools.Name = "Tools";
            this.Tools.Size = new System.Drawing.Size(854, 25);
            this.Tools.TabIndex = 1;
            this.Tools.Text = "toolStrip1";
            // 
            // AnalyzeButton
            // 
            this.AnalyzeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.AnalyzeButton.Image = ((System.Drawing.Image)(resources.GetObject("AnalyzeButton.Image")));
            this.AnalyzeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AnalyzeButton.Name = "AnalyzeButton";
            this.AnalyzeButton.Size = new System.Drawing.Size(86, 22);
            this.AnalyzeButton.Text = "Найти строки";
            this.AnalyzeButton.Click += new System.EventHandler(this.AnalyzeButtonClick);
            // 
            // leftToolPanel
            // 
            this.leftToolPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.leftToolPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftToolPanel.Location = new System.Drawing.Point(0, 49);
            this.leftToolPanel.Name = "leftToolPanel";
            this.leftToolPanel.Size = new System.Drawing.Size(36, 375);
            this.leftToolPanel.TabIndex = 2;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.InterfaceContainer);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(36, 49);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(818, 375);
            this.mainPanel.TabIndex = 3;
            // 
            // InterfaceContainer
            // 
            this.InterfaceContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InterfaceContainer.Location = new System.Drawing.Point(0, 0);
            this.InterfaceContainer.Name = "InterfaceContainer";
            // 
            // InterfaceContainer.Panel1
            // 
            this.InterfaceContainer.Panel1.Controls.Add(this.interfaceBox);
            // 
            // InterfaceContainer.Panel2
            // 
            this.InterfaceContainer.Panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.InterfaceContainer.Size = new System.Drawing.Size(818, 375);
            this.InterfaceContainer.SplitterDistance = 620;
            this.InterfaceContainer.TabIndex = 0;
            // 
            // interfaceBox
            // 
            this.interfaceBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.interfaceBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.interfaceBox.Location = new System.Drawing.Point(0, 0);
            this.interfaceBox.Name = "interfaceBox";
            this.interfaceBox.Size = new System.Drawing.Size(620, 375);
            this.interfaceBox.TabIndex = 0;
            this.interfaceBox.TabStop = false;
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.docuentMenuItem,
            this.PageMenu});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(854, 24);
            this.MainMenu.TabIndex = 4;
            this.MainMenu.Text = "menuStrip1";
            // 
            // docuentMenuItem
            // 
            this.docuentMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenDocumentMenuItem,
            this.CreateBookMenuItem,
            this.saveBookMenuItem,
            this.закрытьToolStripMenuItem,
            this.exitButton});
            this.docuentMenuItem.Name = "docuentMenuItem";
            this.docuentMenuItem.Size = new System.Drawing.Size(73, 20);
            this.docuentMenuItem.Text = "Документ";
            // 
            // OpenDocumentMenuItem
            // 
            this.OpenDocumentMenuItem.Name = "OpenDocumentMenuItem";
            this.OpenDocumentMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.OpenDocumentMenuItem.Size = new System.Drawing.Size(172, 22);
            this.OpenDocumentMenuItem.Text = "Открыть";
            this.OpenDocumentMenuItem.Click += new System.EventHandler(this.OpenDocumentMenuItemClick);
            // 
            // CreateBookMenuItem
            // 
            this.CreateBookMenuItem.Name = "CreateBookMenuItem";
            this.CreateBookMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.CreateBookMenuItem.Size = new System.Drawing.Size(172, 22);
            this.CreateBookMenuItem.Text = "Создать";
            this.CreateBookMenuItem.Click += new System.EventHandler(this.CreateBookMenuItemClick);
            // 
            // saveBookMenuItem
            // 
            this.saveBookMenuItem.Name = "saveBookMenuItem";
            this.saveBookMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveBookMenuItem.Size = new System.Drawing.Size(172, 22);
            this.saveBookMenuItem.Text = "Сохранить";
            this.saveBookMenuItem.Click += new System.EventHandler(this.SaveBookMenuItemClick);
            // 
            // закрытьToolStripMenuItem
            // 
            this.закрытьToolStripMenuItem.Name = "закрытьToolStripMenuItem";
            this.закрытьToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.закрытьToolStripMenuItem.Text = "Закрыть";
            // 
            // exitButton
            // 
            this.exitButton.Name = "exitButton";
            this.exitButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.exitButton.Size = new System.Drawing.Size(172, 22);
            this.exitButton.Text = "Выйти";
            this.exitButton.Click += new System.EventHandler(this.ExitButtonClick);
            // 
            // PageMenu
            // 
            this.PageMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenPageMenuItem,
            this.CreatePageMenuItem,
            this.loadImageMenuItem,
            this.DeletePageMenuItem});
            this.PageMenu.Enabled = false;
            this.PageMenu.Name = "PageMenu";
            this.PageMenu.Size = new System.Drawing.Size(72, 20);
            this.PageMenu.Text = "Страница";
            // 
            // OpenPageMenuItem
            // 
            this.OpenPageMenuItem.Name = "OpenPageMenuItem";
            this.OpenPageMenuItem.Size = new System.Drawing.Size(205, 22);
            this.OpenPageMenuItem.Text = "Открыть";
            // 
            // CreatePageMenuItem
            // 
            this.CreatePageMenuItem.Name = "CreatePageMenuItem";
            this.CreatePageMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.CreatePageMenuItem.Size = new System.Drawing.Size(205, 22);
            this.CreatePageMenuItem.Text = "Создать";
            this.CreatePageMenuItem.Click += new System.EventHandler(this.PageCreate);
            // 
            // loadImageMenuItem
            // 
            this.loadImageMenuItem.Name = "loadImageMenuItem";
            this.loadImageMenuItem.Size = new System.Drawing.Size(205, 22);
            this.loadImageMenuItem.Text = "Загрузить изображение";
            // 
            // DeletePageMenuItem
            // 
            this.DeletePageMenuItem.Name = "DeletePageMenuItem";
            this.DeletePageMenuItem.Size = new System.Drawing.Size(205, 22);
            this.DeletePageMenuItem.Text = "Удалить";
            // 
            // Redactor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 446);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.leftToolPanel);
            this.Controls.Add(this.Tools);
            this.Controls.Add(this.Status);
            this.Controls.Add(this.MainMenu);
            this.MainMenuStrip = this.MainMenu;
            this.Name = "Redactor";
            this.Text = "Redactor";
            this.Tools.ResumeLayout(false);
            this.Tools.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            this.InterfaceContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.InterfaceContainer)).EndInit();
            this.InterfaceContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.interfaceBox)).EndInit();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip Status;
        private System.Windows.Forms.ToolStrip Tools;
        private System.Windows.Forms.Panel leftToolPanel;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.SplitContainer InterfaceContainer;
        private System.Windows.Forms.PictureBox interfaceBox;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem docuentMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenDocumentMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CreateBookMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitButton;
        private System.Windows.Forms.ToolStripButton AnalyzeButton;
        private System.Windows.Forms.ToolStripMenuItem saveBookMenuItem;
        private System.Windows.Forms.ToolStripMenuItem закрытьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PageMenu;
        private System.Windows.Forms.ToolStripMenuItem OpenPageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CreatePageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadImageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeletePageMenuItem;
    }
}