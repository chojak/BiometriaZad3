using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BiometriaZad3
{
    public class Algorithm
    {
        public static BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
        public static byte[,] ImageTo2DByteArray(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            byte[,] result = new byte[width * 3, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width * 3 - 2; x+=3)
                {
                    Color pixel = bmp.GetPixel(x / 3, y);

                    result[x, y] = (byte)pixel.R;
                    result[x + 1, y] = (byte)pixel.G;
                    result[x + 2, y] = (byte)pixel.B;
                }
            return result;
        }
        public static Bitmap ImageToBinaryImage(Bitmap bmp)
        {
            var byteArray = ImageTo2DByteArray(bmp);
            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < byteArray.GetLength(0); x+=3)
                {
                    int average = (byteArray[x, y] + byteArray[x + 1, y] + byteArray[x + 2, y]) / 3;
                    Color newPixel = Color.FromArgb(average, average, average); 
                    newBmp.SetPixel(x / 3, y, newPixel);
                }
            }
            return newBmp;
        }

        public static Bitmap MedianFilter(Bitmap bmp, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range /= 2;

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < byteArray.GetLength(0); x+=3)
                {
                    int min = 255, max = 0;
                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < byteArray.GetLength(1))
                            for (int xx = x - range; xx <= x + range; xx+=3)
                            {
                                if (xx >= 0 && xx < byteArray.GetLength(0))
                                {
                                    if (byteArray[xx, yy] > max)
                                        max = byteArray[xx, yy];
                                    if (byteArray[xx, yy] < min)
                                        min = byteArray[xx, yy];
                                }
                            }
                    }

                    int mean = (max + min) / 2;
                    int currentPixel = (int)byteArray[x, y];

                    double standardDeviation = Math.Sqrt((Math.Pow((double)(currentPixel - mean), 2) + Math.Pow((double)(min - mean), 2) + Math.Pow((double)(max - mean), 2)) / 2);

                    if (currentPixel < mean - 0.25 * standardDeviation)
                    {
                        newBmp.SetPixel(x / 3, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        newBmp.SetPixel(x / 3, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return newBmp;
        }
    } 
}
