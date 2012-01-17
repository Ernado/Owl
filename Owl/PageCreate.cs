using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Owl.DataBase.Domain;

namespace Owl
{
    public partial class PageCreate : Form
    {
        private readonly Redactor _redactor;
        private readonly Page _page = new Page();
        private string _fullPath;
        public PageCreate (Redactor redactor)
        {
            _redactor = redactor;
            InitializeComponent();
        }

        private void ImageLoaderClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter =
                    @"Файлы изображений (*.BMP;*.JPG;*.GIF;.*PNG;*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|Все файлы (*.*)|*.*",
                Title = @"Открыть изображения",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            Cursor = Cursors.WaitCursor;
            var fileName = openFileDialog.SafeFileName;
            try
            {
                if (fileName == null) throw new Exception("Неверное имя");
                new Bitmap(openFileDialog.FileName);
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Ошибка чтения изображения: " + exception.Message);
                return;
            }
            _page.FileName = fileName;
            _fullPath = openFileDialog.FileName;
            imageLoader.Text = fileName;
            imageLoader.BackColor = Color.LightGreen;
            okButton.Enabled = true;
            Cursor = Cursors.Default;
        }

        private void PageCreateLoad(object sender, EventArgs e)
        {
            okButton.Enabled = false;
            if (_redactor.Book == null)
            {
                throw new Exception("No book to add line");
            }

            var number = 1;
            if (_redactor.Book.Pages.Count>0)
                number = (from page in _redactor.Book.Pages orderby page.Number descending select page.Number).ToList()[0] + 1;
            pageNumberInput.Value = number;
            
            pageNumberInput.BackColor = IsNumberFree((int) pageNumberInput.Value) ? Color.LightGreen : Color.Red;
        }

        private bool IsNumberFree (int number)
        {
            return !_redactor.Book.Pages.Any(page => page.Number == number);
        }

        private void PageNumberInputTextChanged(object sender, EventArgs e)
        {
            pageNumberInput.BackColor = IsNumberFree((int) pageNumberInput.Value) ? Color.LightGreen : Color.Red;
        }

        private void CreateCancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            if(IsNumberFree((int)pageNumberInput.Value)&&File.Exists(_fullPath))
            {
                Enabled = false;
                Cursor = Cursors.WaitCursor;
                _page.FileName = _page.Number.ToString() + "_" + _page.FileName;
                File.Copy(_fullPath, _redactor.Book.Directory + "//" + _page.FileName);
                _page.Book = _redactor.Book;
                _redactor.Book.AddPage(_page);
                _redactor.LoadPage(_page);
                Enabled = true;
                Cursor = Cursors.Default;
                Close();
            }
            else
            {
                MessageBox.Show(@"Ошибка создания страницы", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
