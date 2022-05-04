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
        private static int[] Histogram(Bitmap bmp)
        {
            int bmpHeight = bmp.Height;
            int bmpWidth = bmp.Width;

            double[] histogram = new double[256];

            int[] red = new int[256];
            int[] green = new int[256];
            int[] blue = new int[256];

            for (int x = 0; x < bmpWidth; x++)
            {
                for (int y = 0; y < bmpHeight; y++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    red[pixel.R]++;
                    green[pixel.G]++;
                    blue[pixel.B]++;

                    int mean = (pixel.R + pixel.G + pixel.B) / 3;
                    histogram[mean]++;
                }
            }

            return histogram.Select(d => (int)d).ToArray();
        }
        
        private static byte[,] ImageTo2DByteArray(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] array = new byte[data.Width * 3 * data.Height];
            Marshal.Copy(data.Scan0, array, 0, array.Length);


            byte[,] result = new byte[data.Width * 3, data.Height];

            for (int y = 0; y < data.Height; y++)
                for (int x = 0; x < data.Width * 3 - 2; x += 3)
                {
                    int index = y * data.Width * 3 + x;

                    result[x, y] = array[index];
                    result[x + 1, y] = array[index + 1];
                    result[x + 2, y] = array[index + 2];
                }

            bmp.UnlockBits(data);
            return result;
        }

        private static int[,] ImageTo2DIntArray(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] array = new byte[data.Width * 3 * data.Height];
            Marshal.Copy(data.Scan0, array, 0, array.Length);


            int[,] result = new int[data.Width * 3, data.Height];

            for (int y = 0; y < data.Height; y++)
                for (int x = 0; x < data.Width * 3 - 2; x += 3)
                {
                    int index = y * data.Width * 3 + x;
                    
                    if(y < 2 || x < 2 || y > data.Height - 2 || x > data.Width * 3 - 5)
                    {
                        result[x, y] =
                        result[x + 1, y] =
                        result[x + 2, y] = 0;
                    }

                    result[x, y] = 
                    result[x + 1, y] = 
                    result[x + 2, y] = array[index] == byte.MaxValue ? 0 : 1;
                }

            bmp.UnlockBits(data);
            return result;
        }

        private static Bitmap ByteArrayToBitmap(byte[,] byteArray)
        {
            Bitmap result = new Bitmap(byteArray.GetLength(0) / 3, byteArray.GetLength(1));
            BitmapData data = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] array = new byte[byteArray.GetLength(0) * byteArray.GetLength(1)];

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < byteArray.GetLength(0); x++)
                {
                    int index = y * byteArray.GetLength(0) + x;
                    array[index] = byteArray[x, y];
                }
            }

            Marshal.Copy(array, 0, data.Scan0, array.Length);
            result.UnlockBits(data);

            return result;
        }

        private static Bitmap IntArrayToBitmap(int[,] intArray)
        {
            Bitmap result = new Bitmap(intArray.GetLength(0) / 3, intArray.GetLength(1));
            BitmapData data = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] array = new byte[intArray.GetLength(0) * intArray.GetLength(1)];

            for (int y = 0; y < intArray.GetLength(1); y++)
            {
                for (int x = 0; x < intArray.GetLength(0); x++)
                {
                    int index = y * intArray.GetLength(0) + x;
                    array[index] = intArray[x, y] == 2 ? byte.MinValue : byte.MaxValue;
                }
            }

            Marshal.Copy(array, 0, data.Scan0, array.Length);
            result.UnlockBits(data);

            return result;
        }

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
        
        public static byte Otsu(Bitmap bmp)
        {
            // https://www.programmerall.com/article/1106135802/

            int[] tmp = Histogram(bmp);
            double[] histogram = tmp.Select(x => (double)x).ToArray();

            int size = bmp.Height * bmp.Width;
            for (int i = 0; i < 256; i++)
            {
                histogram[i] = histogram[i] / size;
            }

            double avgValue = 0;
            for (int i = 0; i < 256; i++)
            {
                avgValue += i * histogram[i];
            }

            int threshold = 0;
            double maxVariance = 0;
            double w = 0, u = 0;
            for (int i = 0; i < 256; i++)
            {
                w += histogram[i];
                u += i * histogram[i];
                double t = avgValue * w - u;
                double variance = t * t / (w * (1 - w));

                if (variance > maxVariance)
                {
                    maxVariance = variance;
                    threshold = i;
                }
            }

            return (byte)threshold;
        }

        public static Bitmap ImageToBinaryImage(Bitmap OriginalBmp, byte threshold)
        {
            Bitmap bmp = new Bitmap(OriginalBmp);

            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );

            var bmpData = new byte[data.Width * 3 * data.Height];

            Marshal.Copy(data.Scan0, bmpData, 0, bmpData.Length);

            for (int i = 0; i < bmpData.Length; i += 3)
            {
                byte r = bmpData[i + 0];
                byte g = bmpData[i + 1];
                byte b = bmpData[i + 2];

                byte mean = (byte)((r + g + b) / 3);

                bmpData[i + 0] =
                bmpData[i + 1] =
                bmpData[i + 2] = mean > threshold ? byte.MaxValue : byte.MinValue;
            }

            Marshal.Copy(bmpData, 0, data.Scan0, bmpData.Length);

            bmp.UnlockBits(data);
            return bmp;
        }

        public static Bitmap KMM_Thinning(Bitmap bmp)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            var byteArray = ImageTo2DByteArray(bmp);
            int[,] thinningArray = ImageTo2DIntArray(bmp);

            for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
            {
                for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                {
                    if (thinningArray[x, y] == 1)
                    {
                        if (thinningArray[x - 3, y - 1] == 0 ||
                            thinningArray[x - 3, y + 1] == 0 ||
                            thinningArray[x + 3, y + 1] == 0 ||
                            thinningArray[x + 3, y - 1] == 0)
                         
                            thinningArray[x, y] = 
                            thinningArray[x + 1, y] = 
                            thinningArray[x + 2, y] = 3;

                        if (thinningArray[x - 3, y] == 0 ||
                            thinningArray[x + 3, y] == 0 ||
                            thinningArray[x, y - 1] == 0 ||
                            thinningArray[x, y + 1] == 0)

                            thinningArray[x, y] =
                            thinningArray[x + 1, y] =
                            thinningArray[x + 2, y] = 2;
                    }
                }
            }


            return IntArrayToBitmap(thinningArray);
        }
    }
}
