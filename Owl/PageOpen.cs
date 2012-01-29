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

        private void LoadPages()
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
                        s += String.Format(" (Строк: {0} )", page.Lines.Count);

                    s += String.Format(" [Файл: {0}]", page.FileName);

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
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            if (_selectedPage != null)
            {
                _redactor.LoadElement(_selectedPage);
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
            if (_selectedPage == null) return;

            if (MessageBox.Show(String.Format("Вы уверены, что хотите удалить страницу №{0} без возможности восстановления?",
                                              _selectedPage.Number),
                                @"Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _redactor.DeleteElement(_selectedPage);
                PageList.Items.Clear();
                BeginInvoke(new InvokeDelegate(LoadPages));
            }
        }

        private void PageListSelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedPage = _pages[PageList.SelectedIndex];
        }

        private void PageOpenShown(object sender, EventArgs e)
        {
            BeginInvoke(new InvokeDelegate(LoadPages));
        }
    }
}
