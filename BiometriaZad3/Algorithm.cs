using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BiometriaZad3
{
    public class Algorithm
    {
        // kmm
        private static int[] TwotoFourNeighboursArray = { 3, 6, 12, 24, 48, 96, 192, 129,
                                                  7, 14, 28, 56, 112, 224, 193, 131,
                                                  15, 30, 60, 120, 240, 225, 195, 135};
        
        private static int[] KMMDeletionArray = { 3, 5, 7, 12, 13, 14, 15, 20,
                                                21, 22, 23, 28, 29, 30, 31, 48,
                                                52, 53, 54, 55, 56, 60, 61, 62,
                                                63, 65, 67, 69, 71, 77, 79, 80,
                                                81, 83, 84, 85, 86, 87, 88, 89,
                                                91, 92, 93, 94, 95, 97, 99, 101,
                                                103, 109, 111, 112, 113, 115, 116, 117,
                                                118, 119, 120, 121, 123, 124, 125, 126,
                                                127, 131, 133, 135, 141, 143, 149, 151,
                                                157, 159, 181, 183, 189, 191, 192, 193,
                                                195, 197, 199, 205, 207, 208, 209, 211,
                                                212, 213, 214, 215, 216, 217, 219, 220,
                                                221, 222, 223, 224, 225, 227, 229, 231,
                                                237, 239, 240, 241, 243, 244, 245, 246,
                                                247, 248, 249, 251, 252, 253, 254, 255 };


        // k3m
        private static int[] K3M_Array0 = { 3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56, 60, 62,
                                            63, 96, 112, 120, 124, 126, 127, 129, 131, 135,143,
                                            159, 191, 192, 193, 195, 199, 207, 223, 224, 225,
                                            227, 231, 239, 240, 241, 243, 247, 248, 249, 251,
                                            252, 253, 254 };

        private static int[] K3M_Array1 = { 7, 14, 28, 56, 112, 131, 193, 224 };
        
        private static int[] K3M_Array2 = { 7, 14, 15, 28, 30, 56, 60, 112, 120, 131, 135,
                                            193, 195, 224, 225, 240 };
        
        private static int[] K3M_Array3 = { 7, 14, 15, 28, 30, 31, 56, 60, 62, 112, 120, 124,
                                            131, 135, 143, 193, 195, 199, 224, 225, 227, 240,
                                            241, 248 };

        private static int[] K3M_Array4 = { 7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120,
                                            124, 126, 131, 135, 143, 159, 193, 195, 199, 207,
                                            224, 225, 227, 231, 240, 241, 243, 248, 249, 252 };

        private static int[] K3M_Array5 = { 7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120,
                                            124, 126, 131, 135, 143, 159, 191, 193, 195, 199,
                                            207, 224, 225, 227, 231, 239, 240, 241, 243, 248,
                                            249, 251, 252, 254 };

        private static int[] K3M_Array1Pix = {  3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56,
                                                60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131,
                                                135,143, 159, 191, 192, 193, 195, 199, 207, 223,
                                                224, 225, 227, 231, 239, 240, 241, 243, 247, 248,
                                                249, 251, 252, 253, 254 };


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
                for (int x = 0; x < intArray.GetLength(0) - 2; x += 3)
                {
                    int index = y * intArray.GetLength(0) + x;

                    if (intArray[x, y] == 0)
                    {
                        array[index] =
                        array[index + 1] =
                        array[index + 2] = (byte)(255);
                    }
                    if (intArray[x, y] == 1)
                    {
                        array[index] = (byte)(0); // B
                        array[index + 1] = (byte)(0); // G
                        array[index + 2] = (byte)(0); // R
                    }
                    if (intArray[x, y] == 2)
                    {
                        array[index] = (byte)(0);
                        array[index + 1] = (byte)(255);
                        array[index + 2] = (byte)(0);
                    }
                    if (intArray[x, y] == 3)
                    {
                        array[index] = (byte)(0);
                        array[index + 1] = (byte)(0);
                        array[index + 2] = (byte)(255);
                    }
                    if (intArray[x, y] == 4)
                    {
                        array[index] = (byte)(0);
                        array[index + 1] = (byte)(255);
                        array[index + 2] = (byte)(255);
                    }

                    //array[index] =
                    //array[index + 1] =
                    //array[index + 2] = (byte)(intArray[x, y] != 0 ? 0 : 255);
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

        public static Bitmap KMM_Thinning(Bitmap bmp, System.Windows.Controls.Image Image)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            int[,] thinningArray = ImageTo2DIntArray(bmp);
            int[,] tmpArray;
            bool IsAny2 = true;
            bool IsAny3 = true;


            Func<int[,], int, bool> Contains2DArray = (arr, value) =>
            {
                for (int y = 1; y < arr.GetLength(1) - 1; y++)
                    for (int x = 3; x < arr.GetLength(0) - 4; x += 3)
                        if (arr[x, y] == value)
                            return true;
                return false;
            };

            // sprawdza czy zostaly usuwane jakiekolwiek 2 lub 3 (czy jest juz wystarczajaco cieko)
            while (IsAny2 || IsAny3)
            {
                IsAny2 = false;
                IsAny3 = false;

                // sets 2 and 3
                for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                {
                    for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                    {
                        if (thinningArray[x, y] == 1)
                        {
                            if (thinningArray[x - 3, y - 1] == 0 || thinningArray[x - 3, y + 1] == 0 || thinningArray[x + 3, y + 1] == 0 || thinningArray[x + 3, y - 1] == 0)

                                thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 3;

                            if (thinningArray[x - 3, y] == 0 || thinningArray[x + 3, y] == 0 || thinningArray[x, y - 1] == 0 || thinningArray[x, y + 1] == 0)

                                thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 2;
                        }
                    }
                }

                tmpArray = (int[,])thinningArray.Clone();


                // sets and removes 4
                for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                {
                    for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                    {
                        int neighbourValue = 0;
                        if (thinningArray[x, y] != 0)
                        {
                            neighbourValue = thinningArray[x - 3, y - 1] != 0 ? neighbourValue + 128 : neighbourValue;
                            neighbourValue = thinningArray[x, y - 1] != 0 ? neighbourValue + 1 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y - 1] != 0 ? neighbourValue + 2 : neighbourValue;

                            neighbourValue = thinningArray[x - 3, y] != 0 ? neighbourValue + 64 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y] != 0 ? neighbourValue + 4 : neighbourValue;

                            neighbourValue = thinningArray[x - 3, y + 1] != 0 ? neighbourValue + 32 : neighbourValue;
                            neighbourValue = thinningArray[x, y + 1] != 0 ? neighbourValue + 16 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y + 1] != 0 ? neighbourValue + 8 : neighbourValue;

                            if (TwotoFourNeighboursArray.Contains(neighbourValue) && KMMDeletionArray.Contains(neighbourValue))
                            {
                                tmpArray[x, y] = tmpArray[x + 1, y] = tmpArray[x + 2, y] = 0;
                            }
                        }
                    }
                }

                thinningArray = (int[,])tmpArray.Clone();


                // removes 2
                while (Contains2DArray(thinningArray, 2))
                {
                    for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                    {
                        for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                        {
                            int neighbourValue = 0;
                            if (thinningArray[x, y] == 2)
                            {
                                neighbourValue = thinningArray[x - 3, y - 1] != 0 ? neighbourValue + 128 : neighbourValue;
                                neighbourValue = thinningArray[x, y - 1] != 0 ? neighbourValue + 1 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y - 1] != 0 ? neighbourValue + 2 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y] != 0 ? neighbourValue + 64 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y] != 0 ? neighbourValue + 4 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y + 1] != 0 ? neighbourValue + 32 : neighbourValue;
                                neighbourValue = thinningArray[x, y + 1] != 0 ? neighbourValue + 16 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y + 1] != 0 ? neighbourValue + 8 : neighbourValue;

                                if (KMMDeletionArray.Contains(neighbourValue))
                                {
                                    thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 0;
                                    IsAny2 = true;
                                }
                                else
                                {
                                    thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 1;
                                }
                            }
                        }
                    }
                }

                // removes 3
                while (Contains2DArray(thinningArray, 3))
                {
                    for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                    {
                        for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                        {
                            int neighbourValue = 0;
                            if (thinningArray[x, y] == 3)
                            {
                                neighbourValue = thinningArray[x - 3, y - 1] != 0 ? neighbourValue + 128 : neighbourValue;
                                neighbourValue = thinningArray[x, y - 1] != 0 ? neighbourValue + 1 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y - 1] != 0 ? neighbourValue + 2 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y] != 0 ? neighbourValue + 64 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y] != 0 ? neighbourValue + 4 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y + 1] != 0 ? neighbourValue + 32 : neighbourValue;
                                neighbourValue = thinningArray[x, y + 1] != 0 ? neighbourValue + 16 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y + 1] != 0 ? neighbourValue + 8 : neighbourValue;

                                if (KMMDeletionArray.Contains(neighbourValue))
                                {
                                    thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 0;
                                    IsAny3 = true;
                                }
                                else
                                {
                                    thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 1;
                                }
                            }
                        }
                    }
                }

                Image.Source = Algorithm.BitmapToImageSource(IntArrayToBitmap(thinningArray));
            }

            return IntArrayToBitmap(thinningArray);
        }

        public static Bitmap K3M_Thinning(Bitmap bmp, System.Windows.Controls.Image Image)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            List<int[]> K3M_Arrays = new List<int[]>();
            K3M_Arrays.Add(K3M_Array0);
            K3M_Arrays.Add(K3M_Array1);
            K3M_Arrays.Add(K3M_Array2);
            K3M_Arrays.Add(K3M_Array3);
            K3M_Arrays.Add(K3M_Array4);
            K3M_Arrays.Add(K3M_Array5);

            int[,] thinningArray = ImageTo2DIntArray(bmp);
            int[,] tmpArray = (int[,])thinningArray.Clone();

            bool anyPixelChanged = true;
            while (anyPixelChanged)
            {
                anyPixelChanged = false;
                for (int i = 1; i < 6; i++)
                {
                    for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                    {
                        for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                        {
                            if (thinningArray[x, y] == 1)
                            {
                                int neighbourValue = 0;
                                neighbourValue = thinningArray[x - 3, y - 1] != 0 ? neighbourValue + 128 : neighbourValue;
                                neighbourValue = thinningArray[x, y - 1] != 0 ? neighbourValue + 1 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y - 1] != 0 ? neighbourValue + 2 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y] != 0 ? neighbourValue + 64 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y] != 0 ? neighbourValue + 4 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y + 1] != 0 ? neighbourValue + 32 : neighbourValue;
                                neighbourValue = thinningArray[x, y + 1] != 0 ? neighbourValue + 16 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y + 1] != 0 ? neighbourValue + 8 : neighbourValue;

                                if (K3M_Arrays[0].Contains(neighbourValue))
                                {
                                    thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 2;
                                }
                            }
                        }
                    }

                    for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                    {
                        for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                        {
                            // if pixel is a broder checked previously
                            if (thinningArray[x, y] == 2)
                            {
                                int neighbourValue = 0;
                                neighbourValue = thinningArray[x - 3, y - 1] != 0 ? neighbourValue + 128 : neighbourValue;
                                neighbourValue = thinningArray[x,     y - 1] != 0 ? neighbourValue + 1 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y - 1] != 0 ? neighbourValue + 2 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y] !=     0 ? neighbourValue + 64 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y] !=     0 ? neighbourValue + 4 : neighbourValue;

                                neighbourValue = thinningArray[x - 3, y + 1] != 0 ? neighbourValue + 32 : neighbourValue;
                                neighbourValue = thinningArray[x,     y + 1] != 0 ? neighbourValue + 16 : neighbourValue;
                                neighbourValue = thinningArray[x + 3, y + 1] != 0 ? neighbourValue + 8 : neighbourValue;


                                if (K3M_Arrays[i].Contains(neighbourValue))
                                {
                                    anyPixelChanged = true;
                                    thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 0;
                                }
                                else
                                {
                                    thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 1;
                                }
                            }
                        }
                    }
                }
                
                if (anyPixelChanged == true)
                    continue;

                for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                {
                    for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                    {
                        if (thinningArray[x, y] == 1)
                        {
                            int neighbourValue = 0;
                            neighbourValue = thinningArray[x - 3, y - 1] != 0 ? neighbourValue + 128 : neighbourValue;
                            neighbourValue = thinningArray[x, y - 1] != 0 ? neighbourValue + 1 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y - 1] != 0 ? neighbourValue + 2 : neighbourValue;

                            neighbourValue = thinningArray[x - 3, y] != 0 ? neighbourValue + 64 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y] != 0 ? neighbourValue + 4 : neighbourValue;

                            neighbourValue = thinningArray[x - 3, y + 1] != 0 ? neighbourValue + 32 : neighbourValue;
                            neighbourValue = thinningArray[x, y + 1] != 0 ? neighbourValue + 16 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y + 1] != 0 ? neighbourValue + 8 : neighbourValue;

                            if (K3M_Arrays[0].Contains(neighbourValue))
                            {
                                thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 2;
                            }
                        }
                    }
                }

                for (int x = 3; x < thinningArray.GetLength(0) - 4; x += 3)
                {
                    for (int y = 1; y < thinningArray.GetLength(1) - 1; y++)
                    {
                        if (thinningArray[x, y] == 2)
                        {
                            int neighbourValue = 0;
                            neighbourValue = thinningArray[x - 3, y - 1] != 0 ? neighbourValue + 128 : neighbourValue;
                            neighbourValue = thinningArray[x, y - 1] != 0 ? neighbourValue + 1 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y - 1] != 0 ? neighbourValue + 2 : neighbourValue;

                            neighbourValue = thinningArray[x - 3, y] != 0 ? neighbourValue + 64 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y] != 0 ? neighbourValue + 4 : neighbourValue;

                            neighbourValue = thinningArray[x - 3, y + 1] != 0 ? neighbourValue + 32 : neighbourValue;
                            neighbourValue = thinningArray[x, y + 1] != 0 ? neighbourValue + 16 : neighbourValue;
                            neighbourValue = thinningArray[x + 3, y + 1] != 0 ? neighbourValue + 8 : neighbourValue;

                            if (K3M_Array1Pix.Contains(neighbourValue))
                            {
                                thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 0;
                                anyPixelChanged = true;
                            }
                            else
                            {
                                thinningArray[x, y] = thinningArray[x + 1, y] = thinningArray[x + 2, y] = 1;
                            }
                        }
                    }
                }
            }

            return IntArrayToBitmap(thinningArray);
        }
    }
}
