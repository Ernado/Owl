using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Owl.DataBase.Domain;

namespace Owl
{
    public partial class BookCreate : Form
    {
        private Book _createdBook;
        private readonly Redactor _redactor;

        public BookCreate(Redactor redactor)
        {
            _redactor = redactor;
            _redactor.Enabled = false;
            InitializeComponent();
        }

        private delegate void InvokeDelegate();

        private void CheckForBookName()
        {
            if (BookNameBox.Text=="")
            {
                BookNameBox.BackColor = Color.Red;
                return;
            }
            BookNameBox.BackColor = _redactor.Repository.GetByName(BookFolderBox.Text) == null ? Color.LightGreen : Color.Red;
        }

        private void BookNameBoxTextChanged(object sender, EventArgs e)
        {
            BookFolderBox.Text = BookNameBox.Text;
            BeginInvoke(new InvokeDelegate(CheckForBookName));
        }

        private void OkClick(object sender, EventArgs e)
        {
            if (BookFolderBox.Text=="" || BookNameBox.Text=="")
            {
                MessageBox.Show(@"Имя документа или папки не может быть пустым", @"Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            var book = new Book {Directory = BookFolderBox.Text, Name = BookNameBox.Text};
            Cursor = Cursors.WaitCursor;



            if (Directory.Exists(book.Directory))
            {
                MessageBox.Show(@"Папка с таким именем уже существует", @"Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                return;
            }

            if (_redactor.Repository.GetByName(book.Name)!=null)
            {
                MessageBox.Show(@"Документ с таким именем уже существует", @"Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                return;
            }

            Directory.CreateDirectory(book.Directory);

            _redactor.Repository.Add(book);
            _createdBook = _redactor.Repository.GetByName(BookNameBox.Text);
            
            _redactor.LoadBook(_createdBook);

            Cursor = Cursors.Default;
            _redactor.Enabled = true;
            Close();
        }

        private void BookFolderBoxTextChanged(object sender, EventArgs e)
        {

            if (Directory.Exists(BookFolderBox.Text)||BookFolderBox.Text=="")
            {
                BookFolderBox.BackColor = Color.Red;
                OkButton.Enabled = false;
                infoToolTip.SetToolTip(BookFolderBox,"Папка уже существует");
            }
            else
            {
                infoToolTip.SetToolTip(BookFolderBox, "Название папки");
                BookFolderBox.BackColor = Color.LightGreen;
                OkButton.Enabled = true;
            }
        }

        private void CreateCancelButtonClick(object sender, EventArgs e)
        {
            _redactor.Enabled = true;
            Close();
        }
    }
}
