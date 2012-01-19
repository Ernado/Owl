using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Owl.DataBase.Domain;

namespace Owl
{
    public partial class PageOpen : Form
    {
        private Book _book;
        private readonly Redactor _redactor;
        private IList<Page> _pages;
        private Page _selectedPage;

        private void LoadBooks()
        {
            _pages = _book.Pages;
            Cursor = Cursors.WaitCursor;
            if (_pages != null)
                foreach (var page in _pages)
                {
                    var s = page.Number.ToString();
                    if (page.Lines.Count == 0)
                        s += " (Пустая)";
                    else
                        s += " (Строк: " + page.Lines.Count + ") ";
                    s += " [" + page.FileName + "]";
                    PageList.Items.Add(s);
                }
            if (PageList.Items.Count == 0)
            {
                okButton.Enabled = false;
                deleteButton.Enabled = false;
            }
            else
                PageList.SelectedIndex = 0;
            Cursor = Cursors.Default;
        }


        private delegate void InvokeDelegate();

        public PageOpen(Redactor redactor)
        {
            InitializeComponent();
            _redactor = redactor;
            _redactor.Enabled = false;
            _book = _redactor.Book;
            if (_book == null)
                Close();
            LoadBooks();
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            if (_selectedPage != null)
            {
                _redactor.LoadPage(_selectedPage);
            }
            Close();
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private void PageOpenFormClosed(object sender, FormClosedEventArgs e)
        {
            _redactor.Enabled = true;
        }

        private void DeleteButtonClick(object sender, EventArgs e)
        {
            if (_selectedPage!=null)
            {
                _redactor.Book.DeletePage(_selectedPage);
            }
        }

        private void PageListSelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedPage = _pages[PageList.SelectedIndex];
        }
    }
}
