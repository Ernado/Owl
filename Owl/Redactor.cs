using System;
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
        protected Layout DocumentLayout;
        public Book Book;
        public Page Page;
        public Line Line;
        public Word Word;
        private const string Dbfile = "database.db";
        private string _programName = "Редактор";
        public readonly BookRepository Repository = new BookRepository(Dbfile);

        public Redactor()
        {
            InitializeComponent();
        }

        public void LoadLine (Line line)
        {
            throw new NotImplementedException();
        }

        public void LoadWord(Word word)
        {
            throw new NotImplementedException();
        }
  
        public void LoadBook (Book book)
        {
            Text = book.Name + " (" + book.Directory + ") - " + _programName;
            Book = book;
            PageMenu.Enabled = true;
            if (Book.Pages.Count>0)
                LoadPage(Book.Pages[0]);
            saveBookMenuItem.Enabled = true;
        }

        public void LoadPage (Page page)
        {
            Text = Book.Name + " (" + Book.Directory + ") ["+page.Number+"] - " + _programName;
            Page = page;
            interfaceBox.BackgroundImage = new Bitmap(Book.Directory + "//" + page.FileName);
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
    }
}
