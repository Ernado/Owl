using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Owl.DataBase.Domain;

namespace Owl
{
    public partial class BookOpen : Form
    {
        private Book _selectedBook;
        private readonly Redactor _redactor;
        private List<Book> _books; 

        public BookOpen(Redactor redactor)
        {
            _redactor = redactor;
            InitializeComponent();
            _redactor.Enabled = false;

            okButton.Enabled = false;
            deleteButton.Enabled = false;
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            _redactor.Enabled = true;
            Close();
            Dispose(true);
        }

        private void LoadBooks ()
        {
            _books = _redactor.Repository.GetAll().ToList();
            Cursor = Cursors.WaitCursor;
            foreach (var book in _books)
            {
                var s = book.Name;
                if (book.Pages.Count == 0)
                    s += " (Пустой)";
                else
                    s += " (Страниц: " + book.Pages.Count + ") ";
                BookList.Items.Add(s);
            }
            if (BookList.Items.Count == 0)
            {
                okButton.Enabled = false;
                deleteButton.Enabled = false;
            }
            else
                BookList.SelectedIndex = 0;
            Cursor = Cursors.Default;
        }

        private delegate void InvokeDelegate();

        private void BookOpenLoad(object sender, EventArgs e)
        {
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            if (BookList.SelectedIndex == -1) return;
            _redactor.LoadBook(_selectedBook);
            _redactor.Enabled = true;
            Close();
        }

        private void BookListSelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedBook = _books[BookList.SelectedIndex];
            if (_selectedBook == null)
            {
                okButton.Enabled = false;
                deleteButton.Enabled = false;
            }
            okButton.Enabled = true;
            deleteButton.Enabled = true;
        }

        private void DeleteButtonClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"Вы уверены, что хотите удалить " + _selectedBook.Name + @" без возможности восстановления?", 
                @"Подтверждение удаления",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)
            {
                _redactor.Repository.Remove(_selectedBook);
                BookList.Items.Clear();
                BeginInvoke(new InvokeDelegate(LoadBooks));
            }
        }

        private void BookListDoubleClick(object sender, EventArgs e)
        {
            if (_selectedBook != null)
            {
                _redactor.LoadBook(_selectedBook);
                _redactor.Enabled = true;
                Close();
            }
        }

        private void BookOpenShown(object sender, EventArgs e)
        {

            BeginInvoke(new InvokeDelegate(LoadBooks));
        }
    }
}
