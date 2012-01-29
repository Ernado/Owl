using System;
using System.Drawing;
using System.Linq;

namespace Owl.Repositories
{
    public static class Hystogram
    {
        private const double Brightness = 0.5;
        private const byte MinimalHeight = 10;
        private const byte R = 4;
        private static int _width;
        private static int _height;

        private static float[] GetHystogram (float[,] bitmap)
        {
            _width = bitmap.GetLength(0);
            _height = bitmap.GetLength(1);

            return Smooth(GetNormalizedDarkPixels(GetBinaryzed(bitmap)));
        }

        /// <summary>
        /// Возвращает нормализованную гистограмму черных пикселей по вертикали для данного изображения
        /// </summary>
        /// <param name="input">Изображение для анализа</param>
        /// <returns>Нормализованная гистограмма</returns>
        public static float[] GetHystogram(Bitmap input)
        {
            _width = input.Width;
            _height = input.Height;
            var bytes = new float[_width, _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    bytes[x, y] = input.GetPixel(x, y).GetBrightness();
                }
            }

            return GetHystogram(bytes);
        }

        private static float[] GetNormalizedDarkPixels (bool[,] binImage)
        {
            if (binImage == null)
                throw new ArgumentNullException("binImage");

            var bins = new float[_height];

            for (var y = 0; y < _height; y++)
            {
                int left = 0;
                int right = 0;

                bins[y] = 0;

                for (var x = 0; x < _width; x++)
                {
                    var dark = binImage[x, y];

                    bins[y] += dark ? 1 : 0;

                    if (!dark) continue;

                    if (left == 0) left = x;
                    if (x > right) right = x;
                }

                int width = right - left + 1;

                if (bins[y] < MinimalHeight) bins[y] = 0;

                bins[y] /= width;
            }

            return bins;
        }

        private static bool[,] GetBinaryzed(float[,] bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var bins = new bool[_width,_height];

            for (int x = 0; x < _width; x++)
            {
                //игнорируем начальные строчки
                for (int y = 0; y < R; y++)
                {
                    bins[x, y] = false;
                }

                for (int y = R; y < _height - R - 1; y++)
                {
                    var count = 0;

                    //подсчитаем количество пикселей в окрестности R точки x
                    for (int j = -R; j <= R; j++)
                    {
                        if (bitmap[x, y + j] < Brightness) count++;
                    }

                    //если половина пискелей черная, то это точка с существенными соседями
                    bins[x, y] = count >= R;
                }

                //игнорируем конечные строчки
                for (int y = _height - R - 1; y < _height; y++)
                {
                    bins[x, y] = false;
                }
            }

            return bins;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static float[] Smooth (float[] input)
        {
            var smooth = new float[input.Length];

            smooth[0] = input[0];
            smooth[smooth.Length - 1] = input[input.Length - 1];

            for (int i = 1; i < input.Length - 1; i++)
                smooth[i] = (input[i - 1] + input[i] + input[i + 1])/3;

            return smooth;
        }

        public static Bitmap GenerateHystogramImage (Bitmap input)
        {
            _width = input.Width;
            _height = input.Height;

            var bitmap = new Bitmap(_width, _height);
            var bytes = new float[_width,_height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    bytes[x, y] = input.GetPixel(x, y).GetBrightness();
                }
            }

            var hystogram = GetHystogram(bytes);
            var max = hystogram.Max();

            using (Graphics gr = Graphics.FromImage(bitmap))
            {
                for (int y = 0; y < _height; y++)
                {
                    float value = 200 * hystogram[y] / max;
                    gr.DrawLine(Pens.Red, new PointF(0, y), new PointF(value, y));
                }
            }

            return bitmap;
        }
    }
}
