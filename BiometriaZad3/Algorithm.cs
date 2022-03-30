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
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            byte[] bytes = new byte[height * data.Stride];
            try
            {
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            byte[,] result = new byte[width, height];
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    int offset = y * data.Stride + x * 3;
                    result[x, y] = (byte)((bytes[offset + 0] + bytes[offset + 1] + bytes[offset + 2]) / 3);
                }
            return result;
        }
        public static Bitmap ImageToBinaryImage(Bitmap bmp)
        {
            var byteArray = ImageTo2DByteArray(bmp);
            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < byteArray.GetLength(0); x++)
                {
                    Color newPixel = Color.FromArgb(byteArray[x, y], byteArray[x, y], byteArray[x, y]);
                    newBmp.SetPixel(x, y, newPixel);
                }
            }
            return newBmp;
        }
        public static Bitmap Bernsen(Bitmap bmp, int range, int limit)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            {
                //for (int y = 0; y < bmp.Height; y++)
                //{
                //    for (int x = 0; x < bmp.Width; x++)
                //    {
                //        int leftTopX = (x - range / 2) >= 0 ? (x - range / 2) : 0;
                //        int leftTopY = (y - range / 2) >= 0 ? (y - range / 2) : 0;

                //        leftTopX = (x + range / 2) > bmp.Width ? (leftTopX - (x + range / 2)) : leftTopX;
                //        leftTopY = (y + range / 2) > bmp.Height ? (leftTopY - (y + range / 2)) : leftTopY;

                //        int rangeX = (x - range / 2) >= 0 ? range : range + (x - range / 2);
                //        int rangeY = (y - range / 2) >= 0 ? range : range + (y - range / 2);

                //        rangeX = (x + range / 2) > bmp.Width ? (rangeX - ((x + range / 2) - bmp.Width)) : rangeX;
                //        rangeY = (y + range / 2) > bmp.Height ? (rangeY - ((y + range / 2) - bmp.Height - 1)) : rangeY;

                //        if ((x + range / 2) > (bmp.Width - 9))
                //        {
                //            System.Diagnostics.Debug.WriteLine(y);
                //        }

                //        //var rec = new Rectangle(leftTopX, leftTopY, rangeX, rangeY);
                //        //var xd = bmp.Clone(rec, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //    }

                //}
                //return bmp;
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range /= 2;

            for (int y = 0; y < byteArray.GetLength(1); ++y)
            {
                for (int x = 0; x < byteArray.GetLength(0); ++x)
                {
                    int min = 255, max = 0;
                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < bmp.Height)
                            for (int xx = x - range; xx <= x + range; ++xx)
                            {
                                if (xx >= 0 && xx < bmp.Width)
                                {
                                    if (byteArray[xx, yy] > max)
                                        max = byteArray[xx, yy];
                                    if (byteArray[xx, yy] < min)
                                        min = byteArray[xx, yy];
                                }
                            }
                    }

                    int mean = (max + min) / 2;
                    int contrast = max - min;
                    int currentPixel = (int)byteArray[x, y];

                    if (contrast < limit)
                    {
                        if (mean >= 128)
                            newBmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                        else
                            newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        if (currentPixel >= mean)
                            newBmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                        else
                            newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                }
            }

            return newBmp;
        }

        // k - constant value in range 0.2...0.5 (default = 0.2)
        // R - dynamic range of standard deviation (default = 128)
        public static Bitmap Sauvola(Bitmap bmp, int range, double k, int R)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range /= 2;

            for (int y = 0; y < byteArray.GetLength(1); ++y)
            {
                for (int x = 0; x < byteArray.GetLength(0); ++x)
                {
                    int min = 255, max = 0;
                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < bmp.Height)
                            for (int xx = x - range; xx <= x + range; ++xx)
                            {
                                if (xx >= 0 && xx < bmp.Width)
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


                    if (currentPixel > mean * (1 + k * standardDeviation / R - 1))
                    {
                        newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        newBmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return newBmp;
        }

        // k - constant value (default = 0.2)
        public static Bitmap Niblack(Bitmap bmp, int range, double k)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range /= 2;

            for (int y = 0; y < byteArray.GetLength(1); ++y)
            {
                for (int x = 0; x < byteArray.GetLength(0); ++x)
                {
                    int min = 255, max = 0;
                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < bmp.Height)
                            for (int xx = x - range; xx <= x + range; ++xx)
                            {
                                if (xx >= 0 && xx < bmp.Width)
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


                    if (currentPixel < mean - k * standardDeviation)
                    {
                        newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        newBmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return newBmp;
        }
        
        // 
        public static Bitmap Phanskalar(Bitmap bmp, int range, double p, double q, double k, double R)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range /= 2;

            for (int y = 0; y < byteArray.GetLength(1); ++y)
            {
                for (int x = 0; x < byteArray.GetLength(0); ++x)
                {
                    int min = 255, max = 0;
                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < bmp.Height)
                            for (int xx = x - range; xx <= x + range; ++xx)
                            {
                                if (xx >= 0 && xx < bmp.Width)
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
                    double threshold = mean * (1 + p * Math.Exp(-q * mean) + k * ((standardDeviation / R) - 1));

                    if (currentPixel > threshold)
                    {
                        newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        newBmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return newBmp;
        }
    }
}
