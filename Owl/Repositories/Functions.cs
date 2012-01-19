using System;
using System.Collections.Generic;
using System.Drawing;

using System.Drawing.Drawing2D;
using System.Linq;

namespace Owl.Repositories
{
    public static class Functions
    {
        public static GraphicsPath GeneratePathFromPoints (List<Point> points, bool closed = true)
        {
            var path = new GraphicsPath();

            if (points.Count < 2) throw new Exception("Cannot generate path");

            for (int i = 0; i < points.Count - 1; i++)
                path.AddLine(points[i], points[i + 1]);
            if (closed)
                path.CloseFigure();
            return path;
        }

        public static GraphicsPath GeneratePathFromPoints(List<DataBase.Domain.Point> points, bool closed = true)
        {
            var path = new GraphicsPath();

            if (points.Count < 3) throw new Exception("Cannot generate path");

            for (var i = 0; i < points.Count - 1; i++)
            {
                var startPoint = new PointF(points[i].X, points[i].Y);
                var endPoint = new PointF(points[i+1].X, points[i+1].Y);
                path.AddLine(startPoint,endPoint);
            }
            if (closed)
                path.CloseFigure();
            return path;
        }


        /// <summary>
        /// ������������ ������ ������� ������
        /// ����� ��������������� ������ ��������
        /// �������� � ���������� [0;1]
        /// </summary>
        /// <param name="array">������ ������� ������</param>
        public static double[] FeautureScaling(double[] array)
        {
            if (array == null) throw new ArgumentNullException("array");

            var scaledArray = array;
            //����������� ������
            double min = array.Min();
            double range = array.Max() - array.Min();
            for (int i = 0; i < array.Length; i++)
            {
                scaledArray[i] = (array[i] - min) / range;
            }
            return scaledArray;
        }

        /// <summary>
        /// ��������������� ����������� �� �������� �������������.
        /// </summary>
        /// <param name="bit">������ �������� �������������</param>
        /// <returns></returns>
        public static Bitmap RasterizeBitMassive(bool[,] bit)
        {
            int width = bit.GetLength(0);
            int height = bit.GetLength(1);
            Bitmap bitmap = new Bitmap(width, height);
            /* �������� �� ���� ��������� �������. ���������� ������ �����, ����
             * ������� ������� �������� 1, ����� ����� �����, ���������� �����
             * ������������ "�����������" �������� ���������� ��������� �������*/
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (bit[x, y])
                        bitmap.SetPixel(x, y, Color.Black);
                    else
                        bitmap.SetPixel(x, y, Color.White);
            return bitmap;
        }

        /// <summary>
        /// ������� �������� ������� ����� �� �����������.
        /// <param name="bitmap">�������� �����������.</param>
        /// <param name="ratio">����� �������.</param>
        /// <returns></returns>
        ///</summary>
        public static bool[,] BinarizeImage(Bitmap bitmap, double ratio = 0.8)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            //�������� �������
            bool[,] bit = new bool[bitmap.Width, bitmap.Height];
            //�������� �� ���� ������. ���� ������� ����� ������ <ratio>, 
            //�� � ������������� ������� ������� ������� True 
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (bitmap.GetPixel(x, y).GetBrightness() < ratio)
                        bit[x, y] = true;
            return bit;
        }
    }
}