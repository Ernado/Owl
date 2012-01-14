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
        private List<Page> _pages;

        private void LoadBooks()
        {
            _pages = _book.Pages as List<Page>;
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
            _book = _redactor.Book;
            if (_book == null)
                Close();
        }
    }
}
