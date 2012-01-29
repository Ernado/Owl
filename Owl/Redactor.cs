using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using Owl.DataBase.Repositories;
using Owl.DataBase.Domain;
using Owl.Domain;
using Owl.Repositories;

namespace Owl
{
    public partial class Redactor : Form
    {
        private VectorRedactorRepository _vectorRedactor;
        protected Layout DocumentLayout;
        public Book Book;
        public Page Page;
        public Line Line;
        public Word Word;
        private bool _changed;

        private VectorRedactorRepository.VectorRedactorConfig _vectorRedactorConfig;
        private const string Dbfile = "database.db";
        private string _programName = "ИТРИТ Редактор Прототип";
        public readonly BookRepository Repository;

        private delegate void InvokeDelegate();

        private Graphics GetCanvas()
        {
            return Graphics.FromImage(interfaceBox.Image);
        }

        public Redactor()
        {
            InitializeComponent();
            CheckForDbFile();
            Repository = new BookRepository(Dbfile);
            _vectorRedactorConfig = new VectorRedactorRepository.VectorRedactorConfig
                                        {
                                            LineBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 255)),
                                            LinePen = new Pen(Color.Blue),
                                            WordBrush = new SolidBrush(Color.FromArgb(40,50,0,255)),
                                            WordPen = new Pen(Color.FromArgb(255, 128, 0, 255))

                                        };
            interfaceBox.Image = new Bitmap(100, 100);
            _vectorRedactor = new VectorRedactorRepository(GetCanvas(), this, _vectorRedactorConfig);
            interfaceBox.MouseWheel += InterfaceScrollContainerMouseWheel;
            centeredInterfaceHolderPanel.MouseWheel += InterfaceScrollContainerMouseWheel;
            MouseWheel += InterfaceScrollContainerMouseWheel;
            ResetPageView();
        }

        private void InterfaceScrollContainerMouseWheel (object sender, MouseEventArgs e)
        {
            interfaceScrollContainer.Focus();
        }

        #region Element Processing
        /// <summary>
        /// Обновляет заголовок окна программы
        /// </summary>
        private void UpdateHeader()
        {
            Text = String.Format("{0}", _programName);
            if (Book != null)
                Text = Text + String.Format(": {0} ({1}) ", Book.Name, Book.Directory);
            else
                return;

            if (Page != null)
                Text = Text + String.Format("cтраница: {0}", Page.Number);
            else
                return;

            if (Line != null)
                Text = Text + String.Format(", строка {0}", Line.Number);
            else
                return;

            if (Word != null)
                Text = Text + String.Format(", слово {0}", Word.Number);
            else
                return;

            if (!string.IsNullOrEmpty(Word.Name))
                Text = Text + String.Format(" ({0})", Word.Name);
        }

        public void LoadElement(Line line)
        {
            Line = line;
            Word = null;

            wordList.Items.Clear();
            wordCountLink.Text = line.Words.Count().ToString();

            var lines = Page.Lines as List<Line>;
            if ((lines != null && !lines.Contains(Line)) || lines == null)
                saveBookMenuItem.Enabled = true;

            lineEditGroupBox.Enabled = true;
            wordTabPanel.Enabled = true;
            documentTabControl.SelectedIndex = 2;

            _vectorRedactor.ProcessActivation(line);

            UpdateElementView(line);
        }

        private void UpdateElementView(Page page)
        {
            lineListBox.Items.Clear();
            linesCountsLink.Text = page.Lines.Count().ToString();
            pageNumberNumeric.Value = page.Number;

            if (page.Lines.Count > 0)
            {
                foreach (var line in page.Lines)
                {
                    var s = line.Number.ToString();
                    if (line.Words.Count == 0)
                        s += " (Пустая)";
                    else
                        s += String.Format(" (Слов: {0})", line.Words.Count);

                    lineListBox.Items.Add(s);
                }
            }
            UpdateElementView(Book);
        }

        private void UpdateElementView(Book book)
        {
            pageListBox.Items.Clear();
            pagesCountLink.Text = book.Pages.Count().ToString();
            documentNameInputBox.Text = Book.Name;

            if (book.Pages != null)
                foreach (var page in book.Pages)
                {
                    var s = page.Number.ToString();
                    if (page.Lines.Count == 0)
                        s += " (Пустая)";
                    else
                        s += String.Format(" (Строк: {0} )", page.Lines.Count);

                    s += String.Format(" [Файл: {0}]", page.FileName);
                    pageListBox.Items.Add(s);
                }
            UpdateHeader();
        }

        private void UpdateElementView(Line line)
        {
            wordList.Items.Clear();

            if (line.Words.Count > 0)
            {
                foreach (var word in line.Words)
                    wordList.Items.Add(String.Format("{0} ({1})", word.Number, word.Name));
            }
            lineNumberNumeric.Value = line.Number;
            wordCountLink.Text = line.Words.Count.ToString();
            UpdateElementView(Page);
        }

        public void LoadElement(Word word)
        {
            Word = word;

            var words = Line.Words as List<Word>;
            if ((words != null && !words.Contains(Word)) || words == null)
                saveBookMenuItem.Enabled = true;

            wordEditGroupBox.Enabled = true;
            documentTabControl.SelectedIndex = 3;

            _vectorRedactor.ProcessActivation(word);

            UpdateElementView(word);
            UpdateHeader();
        }

        private void UpdateElementView(Word word)
        {
            wordNameBox.Text = word.Name;
            wordNumberNumeric.Value = word.Number;
        }

        public void LoadElement(Book book)
        {
            Book = book;
            PageMenu.Enabled = true;

            saveBookMenuItem.Enabled = true;
            documentTabControl.Enabled = true;
            documentTabControl.SelectedIndex = 0;

            pageTabPanel.Enabled = true;

            if (book.Pages.Count > 0)
                LoadElement(book.Pages[0]);

            UpdateElementView(book);
        }

        public void LoadElement(Page page)
        {
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

            _vectorRedactor = new VectorRedactorRepository(GetCanvas(), this, _vectorRedactorConfig);
            _vectorRedactor.LoadPage(Page);

            UpdateElementView(page);
        }

        private void ResetPageView()
        {
            interfaceBox.Image = new Bitmap(100, 100);
            interfaceBox.BackgroundImage = new Bitmap(100, 100);
            centeredInterfaceHolderPanel.Visible = false;
            _vectorRedactor = new VectorRedactorRepository(GetCanvas(), this, _vectorRedactorConfig);
            UpdateHeader();
        }

        public void DeleteElement(Page page)
        {
            var document = page.Book;

            Book.RemovePage(page);

            if (Page == page)
            {
                Page = null;
                ResetPageView();
            }
            
            ProcessElementChanges(document);
        }
        
        private void DeleteElement(Line line)
        {
            _vectorRedactor.ProcessRemove(line);

            var page = line.Page;
            
            Page.RemoveLine(line);

            if (Line == line)
            {
                Line = null;
            }
            
            ProcessElementChanges(page);
        }

        private void DeleteElement(Word word)
        {
            _vectorRedactor.ProcessRemove(word);

            var line = word.Line;

            line.RemoveWord(word);

            if (Word == word)
            {
                Word = null;
            }

            ProcessElementChanges(line);
        }

        #endregion

        private void AnalyzePage()
        {
            AnalyzablePage page;
            Cursor = Cursors.WaitCursor;
            Enabled = false;
            try { page = new AnalyzablePage(Page); }
            catch (Exception exception)
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

        public void ProcessDocumentChanges ()
        {
            _changed = true;
            saveBookMenuItem.Enabled = true;
        }

        public void ProcessElementChanges (Page page)
        {
            ProcessDocumentChanges();
            savePageButton.Enabled = true;
            UpdateElementView(page);
        }

        public void ProcessElementChanges (Line line)
        {
            ProcessDocumentChanges();
            saveLineButton.Enabled = true;
            UpdateElementView(line);
        }

        private void ProcessElementChanges(Book book)
        {
            ProcessDocumentChanges();
            UpdateElementView(book);
        }

        /// <summary>
        /// Проверяет на наличие файла базы данных. Если такового нет - создает новый.
        /// </summary>
        public static void CheckForDbFile()
        {
            if (File.Exists(Dbfile)) return;
            var configuration = Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .UsingFile(Dbfile)
                )
                .Mappings(m =>
                          m.FluentMappings.AddFromAssemblyOf<BookRepository>())
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
                if (_changed)
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
                _changed = false;
            }
        }

        private void OpenPageMenuItemClick(object sender, EventArgs e)
        {
            new PageOpen(this).Show();
        }

        private void RedactorLoad(object sender, EventArgs e)
        {
            
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
            _vectorRedactor.ProcessMouseMove(e);
        }

        private void InterfaceBoxMouseUp(object sender, MouseEventArgs e)
        {
            _vectorRedactor.ProcessMouseUp();
        }

        private void MoveModeButtonClick(object sender, EventArgs e)
        {
            if (_vectorRedactor.ProcessModeChangeToMove())
            {
                addModeButton.Checked = false;
                createMoveButton.Checked = false;
                moveModeButton.Checked = true;
            }
        }

        private void CreateMoveButtonClick(object sender, EventArgs e)
        {
            if (_vectorRedactor.ProcessModeChangeToCreate())
            {
                addModeButton.Checked = false;
                createMoveButton.Checked = true;
                moveModeButton.Checked = false;
            }
        }

        private void AddModeButtonClick(object sender, EventArgs e)
        {
            if (_vectorRedactor.ProcessModeChangeToAdd())
            {
                addModeButton.Checked = true;
                createMoveButton.Checked = false;
                moveModeButton.Checked = false;
            }
        }

        private void InterfaceBoxPaint(object sender, PaintEventArgs e)
        {
            _vectorRedactor.Layout.Canvas = e.Graphics;
            _vectorRedactor.Layout.Canvas.SmoothingMode = SmoothingMode.HighQuality;
            _vectorRedactor.Draw();
        }

        public void InvalidateInterfaceBox()
        {
            interfaceBox.Invalidate();
            interfaceBox.Update();
        }

        private void RedactorFormClosing(object sender, FormClosingEventArgs e)
        {
            Repository.Close();
        }

        private void CreatePageButtonClick(object sender, EventArgs e)
        {
            (new PageCreate(this)).Show(this);
        }

        private void SavePageButtonClick(object sender, EventArgs e)
        {
            var number = pageNumberNumeric.Value;
            if (Book.Pages.Any(page => (page.Number == number) && page != Page))
            {
                MessageBox.Show(@"Страница с таким номером уже существует!", @"Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            else
            {
                Page.Number = (int)number;
                UpdateElementView(Book);
                savePageButton.Enabled = false;
                cacelSavePageButton.Enabled = false;
            }
        }

        private void CacelSavePageButtonClick(object sender, EventArgs e)
        {
            pageNumberNumeric.Value = Page.Number;
            cacelSavePageButton.Enabled = false;
        }

        private void PageNumberNumericValueChanged(object sender, EventArgs e)
        {
            cacelSavePageButton.Enabled = true;
            savePageButton.Enabled = true;
        }

        private void PageListBoxDoubleClick(object sender, EventArgs e)
        {
            var index = pageListBox.SelectedIndex;
            if (index>=0 && index<=Book.Pages.Count)
                LoadElement(Book.Pages[index]);
        }

        private void DeletePageButtonClick(object sender, EventArgs e)
        {
            if (Page == null) return;

            if (MessageBox.Show(String.Format("Вы уверены, что хотите удалить страницу №{0} без возможности восстановления?",
                                              Page.Number),
                                @"Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteElement(Page);
            }
        }

        private void SaveLineButtonClick(object sender, EventArgs e)
        {
            var number = lineNumberNumeric.Value;
            if (Page.Lines.Any(line => (line.Number == number) && line != Line))
            {
                MessageBox.Show(@"Строка с таким номером уже существует!", @"Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            else
            {
                Line.Number = (int)number;
                ProcessDocumentChanges();
                UpdateElementView(Line);
                saveLineButton.Enabled = false;
                cancellLineButton.Enabled = false;
            }
        }

        private void DeleteLineButtonClick(object sender, EventArgs e)
        {
            if (Line == null) return;

            if (MessageBox.Show(String.Format("Вы уверены, что хотите удалить строку №{0} без возможности восстановления?",
                                              Page.Number),
                                @"Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteElement(Line);
                UpdateElementView(Page);
            }
        }

        private void CancellLineButtonClick(object sender, EventArgs e)
        {
            lineNumberNumeric.Value = Line.Number;
            cancellLineButton.Enabled = false;
        }

        private void LineNumberNumericValueChanged(object sender, EventArgs e)
        {
            saveLineButton.Enabled = true;
            cancellLineButton.Enabled = true;
        }

        private void DocumentTabCreateButtonClick(object sender, EventArgs e)
        {
            (new BookCreate(this)).Show(this);
        }

        private void DocumentTabSaveButtonClick(object sender, EventArgs e)
        {
            var name = documentNameInputBox.Text;
            if (Repository.GetByName(name)!=Book)
            {
                MessageBox.Show(@"Документ с таким именем уже существует!", @"Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            else
            {
                Book.Name = name;
                ProcessElementChanges(Book);
                documentTabSaveButton.Enabled = false;
                documentTabCancellButton.Enabled = false;
            }
        }

        private void DocumentNameInputBoxTextChanged(object sender, EventArgs e)
        {
            documentTabSaveButton.Enabled = true;
            documentTabCreateButton.Enabled = true;
        }

        private void OpenDictionaryMenuItemClick(object sender, EventArgs e)
        {
            (new Dictionary(this)).Show(this);
        }

        private void DeleteWordButtonClick(object sender, EventArgs e)
        {
            if (Word == null) return;
            DeleteElement(Word);
            UpdateElementView(Line);
        }

        private void WordNumberNumericValueChanged(object sender, EventArgs e)
        {
            saveWordButton.Enabled = true;
            cancellWordButton.Enabled = true;
        }

        private void SaveWordButtonClick(object sender, EventArgs e)
        {
            var number = wordNumberNumeric.Value;
            if (Line.Words.Any(word => (word.Number == number) && word != Word))
            {
                MessageBox.Show(@"Слово с таким номером уже существует!", @"Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            else
            {
                Word.Number = (int)number;
                Word.Name = wordNameBox.Text;
                ProcessDocumentChanges();
                saveWordButton.Enabled = false;
                cancellWordButton.Enabled = false;
            }
        }

        private void CancellWordButtonClick(object sender, EventArgs e)
        {
            wordNumberNumeric.Value = Word.Number;
            wordNameBox.Text = Word.Name;
            saveWordButton.Enabled = false;
            cancellWordButton.Enabled = false;
        }

        private void WordNameBoxTextChanged(object sender, EventArgs e)
        {
            saveWordButton.Enabled = true;
            cancellWordButton.Enabled = true;
        }

        private void WordListSelectedIndexChanged(object sender, EventArgs e)
        {
            var index = wordList.SelectedIndex;
            if (index >= 0 && index <= Line.Words.Count)
                LoadElement(Line.Words[index]);
        }
    }
}
