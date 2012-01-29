using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Owl.DataBase.Domain;
using Owl.DataBase.Repositories;
using Owl.Domain;
using Owl.Repositories;

namespace Owl
{
    public partial class Dictionary : Form
    {
        class DictionaryWord
        {
            public List<Bitmap> Images;
            public string Name;
            public List<Word> Words;

            public DictionaryWord(Word word, Bitmap image)
            {
                Name = word.Name;
                Images = new List<Bitmap>{image};
                Words = new List<Word>{word};
             }

            public void AddImage (Word word,Bitmap image)
            {
                Images.Add(image);
                Words.Add(word);
            }
        }

        private Redactor _redactor;
        private List<DictionaryWord> _dictionaryWords = new List<DictionaryWord>(); 

        public Dictionary(Redactor redactor)
        {
            InitializeComponent();
            _redactor = redactor;
            LoadWords();
            if (wordList.Items.Count > 0)
                wordList.TabIndex = 0;
        }

        private Bitmap MergeImages (List<Bitmap> images)
        {
            var image = new Bitmap(images[0]);
            images.Remove(images[0]);
            foreach (var bitmap in images)
            {
                var heights = new List<int> {bitmap.Height, image.Height};

                var newImage = new Bitmap(bitmap.Width + image.Width, heights.Max());
                using (var gr = Graphics.FromImage(newImage))
                {
                    gr.DrawImageUnscaled(image,0,0);
                    gr.DrawImageUnscaled(bitmap,image.Width,0);
                }
            }
            return image;
        }

        private Bitmap GetImageOf(Word word, Bitmap pageImage)
        {
            if (word.Polygons == null || word.Polygons.Count <= 0)
                return null;

            var images =
                word.Polygons.Select(
                    polygon =>
                    pageImage.Clone(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints()).GetBounds(),
                                    pageImage.PixelFormat)).ToList();
            return MergeImages(images);
        }

        private DictionaryWord DictionaryHasWord (Word word)
        {
            return _dictionaryWords.Where(dictionaryWord => dictionaryWord.Name == word.Name).FirstOrDefault();
        }

        private void LoadWords ()
        {
            var books = _redactor.Repository.GetAll();
            foreach (var book in books)
            {
                var directory = book.Directory;

                foreach (var page in book.Pages)
                {
                    var pageImage = new Bitmap(String.Format("{0}//{1}", directory, page.FileName));

                    foreach (var line in page.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            var image = GetImageOf(word, pageImage);
                            var dictionaryWithWord = DictionaryHasWord(word);
                            if (dictionaryWithWord == null)
                                _dictionaryWords.Add(new DictionaryWord(word, image));
                            else
                                dictionaryWithWord.AddImage(word, image);
                        }
                    }
                }
            }

            foreach (var dictionaryWord in _dictionaryWords)
            {
                wordList.Items.Add(dictionaryWord.Name ?? "Пустое");
            }
        }

        private void WordListSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = _dictionaryWords[wordList.SelectedIndex];
            imageList1.Images.Clear(); 
            variantList.Items.Clear();
            for (int index = 0; index < selectedItem.Images.Count; index++)
            {
                var image = selectedItem.Images[index];
                imageList1.Images.Add(image);
                variantList.Items.Add(String.Format("Вариант написания {0}", index), index);
            }
            Text = selectedItem.Name;
            if (variantList.Items.Count>0)
                variantList.TabIndex = 0;
        }

        private void VariantListSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedWord = _dictionaryWords[wordList.SelectedIndex].Words[variantList.SelectedIndices[0]];
            var line = selectedWord.Line;
            var page = line.Page;
            var book = page.Book;
            wordInformation.Text = string.Format("Документ: {0}, Страница:{1}, строка {2}, номер {3}",book.Name,page.Number, line.Number, selectedWord.Number);
            variantImage.Image = _dictionaryWords[wordList.SelectedIndex].Images[variantList.SelectedIndices[0]];
        }
    }
}
