using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using Owl.DataBase.Repositories;
using Owl.DataBase.Domain;
using Owl.Domain;

namespace Owl
{
    public partial class Redactor : Form
    {
        private VectorRedactor _vectorRedactor;
        protected Layout DocumentLayout;
        public Book Book;
        public Page Page;
        public Line Line;
        public Word Word;

        private const string Dbfile = "database.db";
        private string _programName = "Редактор";
        public readonly BookRepository Repository = new BookRepository(Dbfile);
        
        private Graphics GetCanvas()
        {
            return Graphics.FromImage(interfaceBox.Image);
        }

        public Redactor()
        {
            InitializeComponent();
        }

        public void LoadLine (Line line)
        {
            Line = line;

            var lines = Page.Lines as List<Line>;
            if ((lines != null && !lines.Contains(Line)) || lines == null)
                saveBookMenuItem.Enabled = true;
            
            Text = Book.Name + " (" + Book.Directory + ") [" + Page.Number + "] [" + Line.Number + "] - " + _programName;

            lineEditGroupBox.Enabled = true;
            wordTabPanel.Enabled = true;
            documentTabControl.SelectedIndex = 2;

            if (Line.Words.Count > 0)
                LoadWord(Line.Words[0]);
        }

        public void LoadWord(Word word)
        {
            Word = word;

            var words = Line.Words as List<Word>;
            if ((words != null && !words.Contains(Word)) || words == null)
                saveBookMenuItem.Enabled = true;

            wordEditGroupBox.Enabled = true;
            documentTabControl.SelectedIndex = 3;

            Text = Book.Name + " (" + Book.Directory + ") [" + Page.Number + "] [" + Line.Number + "] [" + Word.Number + "] - " + _programName;
        }
  
        public void LoadBook (Book book)
        {
            Text = book.Name + " (" + book.Directory + ") - " + _programName;
            Book = book;
            PageMenu.Enabled = true;

            saveBookMenuItem.Enabled = true;
            documentTabControl.Enabled = true;
            documentTabControl.SelectedIndex = 0;

            pageTabPanel.Enabled = true;
            
            documentNameInputBox.Text = Book.Name;
            pagesCountLink.Text = Book.Pages.Count.ToString();

            if (Book.Pages.Count > 0)
                LoadPage(Book.Pages[0]);
        }

        public void LoadPage (Page page)
        {
            Text = Book.Name + " (" + Book.Directory + ") ["+page.Number+"] - " + _programName;
            Page = page;
            var image = new Bitmap(Book.Directory + "//" + page.FileName);
            interfaceBox.BackgroundImage = image;
            interfaceBox.Image = new Bitmap(image.Width, image.Height);
            centeredInterfaceHolderPanel.Visible = true;
            
            lineTabPanel.Enabled = true;
            pageEditGroupBox.Enabled = true;

            documentTabControl.SelectedIndex = 1;

            var pages = Book.Pages as List<Page>;
            if ((pages != null && !pages.Contains(Page)) || pages == null)
                saveBookMenuItem.Enabled = true;

            if (Page.Lines.Count>0)
                LoadLine(Page.Lines[0]);

            _vectorRedactor = new VectorRedactor(GetCanvas(), this);
            //DocumentLayout = new Layout(Page, );
        }

        public static void CheckForDbFile()
        {
            if (File.Exists(Dbfile)) return;
            var configuration = Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .UsingFile(Dbfile)
                )
                .Mappings(m =>
                          m.FluentMappings.AddFromAssemblyOf<Book>())
                .BuildConfiguration();
            new SchemaExport(configuration).Execute(false, true, false);
        }


        private void PageCreate(object sender, EventArgs e)
        {
            new PageCreate(this).Show();
        }

        private void SaveChanges ()
        {
            Repository.Update(Book);
        }

        private void ExitButtonClick(object sender, EventArgs e)
        {
            if (Book!=null&&Page!=null)
            {
                var fromDb = Repository.GetByName(Book.Name);
                if (fromDb!=Book||!fromDb.Pages.Contains(Page))
                {
                    var dialogResult = MessageBox.Show(@"Сохранить изменённые данные?",@"Выход",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
                    switch (dialogResult)
                    {
                        case DialogResult.Yes:
                            SaveChanges();
                            break;
                        case DialogResult.No:
                            break;
                        case DialogResult.Cancel:
                            return;
                    }
                }
            }
            Close();
        }

        private void OpenDocumentMenuItemClick(object sender, EventArgs e)
        {
            new BookOpen(this).Show();
        }

        private void CreateBookMenuItemClick(object sender, EventArgs e)
        {
            new BookCreate(this).Show();
        }

        private void AnalyzePage ()
        {
            AnalyzablePage page;
            Cursor = Cursors.WaitCursor;
            Enabled = false;
            try {page = new AnalyzablePage(Page);}
            catch(Exception exception)
            {
                Enabled = true;
                MessageBox.Show(exception.Message, @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Status.Text = @"Автоматическое построение разметки...";
            page.SegmentatePage();
            Status.Text = "";
            Enabled = true;
        }


        private delegate void InvokeDelegate();

        private void AnalyzeButtonClick(object sender, EventArgs e)
        {
            AnalyzePage();
           //BeginInvoke(new InvokeDelegate(AnalyzePage));
        }

        private void SaveBookMenuItemClick(object sender, EventArgs e)
        {
            if (Book != null)
            {
                SaveChanges();
                saveBookMenuItem.Enabled = false;
            }
        }

        private void OpenPageMenuItemClick(object sender, EventArgs e)
        {
            new PageOpen(this).Show();
        }

        private void RedactorLoad(object sender, EventArgs e)
        {
            _vectorRedactor = new VectorRedactor(GetCanvas(), this);
        }

        private void SelectNewTab(object sender, EventArgs e)
        {
            documentTabControl.SelectedIndex++;
        }

        private void InterfaceScrollContainerSizeChanged(object sender, EventArgs e)
        {
            var newLeft = (Width - centeredInterfaceHolderPanel.Size.Width - rightPanel.Width) / 2;

            if (newLeft <= 10)
                newLeft = 10;

            centeredInterfaceHolderPanel.Left = newLeft;
            centeredInterfaceHolderPanel.Top = 10;
        }

        private void InterfaceBoxMouseClick(object sender, MouseEventArgs e)
        {
            _vectorRedactor.ProcessClick(e);
        }

        private void InterfaceBoxMouseDown(object sender, MouseEventArgs e)
        {
            _vectorRedactor.ProcessMouseDown(e);
        }

        private void InterfaceBoxMouseMove(object sender, MouseEventArgs e)
        {
            _vectorRedactor.ProcessMove(e);
        }

        private void InterfaceBoxMouseUp(object sender, MouseEventArgs e)
        {
            _vectorRedactor.ProcessMouseUp();
        }

        private void MoveModeButtonClick(object sender, EventArgs e)
        {
            if (_vectorRedactor.ProcessModeChangeToMove())
            {
                moveModeButton.Checked = true;
            }
        }

        private void CreateMoveButtonClick(object sender, EventArgs e)
        {
            if (_vectorRedactor.ProcessModeChangeToCreate())
            {
                createMoveButton.Checked = true;
            }
        }

        private void AddModeButtonClick(object sender, EventArgs e)
        {
            if (_vectorRedactor.ProcessModeChangeToAdd())
            {
                addModeButton.Checked = true;
            }
        }

 
    }
}
