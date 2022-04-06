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
       
        private static int AverageOfThreeNext(byte[,] byteArray, int x, int y)
        {
            return (byteArray[x, y] + byteArray[x + 1, y] + byteArray[x + 2, y]) / 3;
        }
     
        private static double variance(int[] numbers)
        {
            if (numbers.Length <= 1)
            {
                return 0;
            }

            double avg = average(numbers);

            double sumOfSquares = 0.0;

            foreach (var num in numbers)
            {
                sumOfSquares += Math.Pow((num - avg), 2.0);
            }
            return Math.Sqrt(sumOfSquares / (double)(numbers.Length - 1));
        }
     
        private static double average(int[] numbers)
        {
            if (numbers.Length <= 1)
            {
                return numbers[0];
            }

            double sum = 0;
            foreach (var x in numbers)
            {
                sum += x;
            }
            return sum / numbers.Length;
        }
      
        public static byte[,] ImageTo2DByteArray(Bitmap bmp)
        {
            System.Diagnostics.Debug.WriteLine(bmp.PixelFormat);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] array = new byte[data.Width * 3 * data.Height];
            Marshal.Copy(data.Scan0, array, 0, array.Length);

            
            byte[,] result = new byte[data.Width * 3, data.Height];

            for (int y = 0; y < data.Height; y++)
                for (int x = 0; x < data.Width * 3 - 2; x += 3)
                {
                    int index = y * data.Width * 3 + x;

                    result[x, y]     = array[index];
                    result[x + 1, y] = array[index + 1];
                    result[x + 2, y] = array[index + 2];
                }

            bmp.UnlockBits(data);
            return result;
        }

        public static Bitmap ByteArrayToBitmap(byte[,] byteArray)
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

        public static Bitmap ImageToBinaryImage(Bitmap bmp)
        {
            var byteArray = ImageTo2DByteArray(bmp);
            var newByteArray = new byte[byteArray.GetLength(0), byteArray.GetLength(1)];

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < byteArray.GetLength(0) - 2; x += 3)
                {
                    int average = (byteArray[x, y] + byteArray[x + 1, y] + byteArray[x + 2, y]) / 3;
                    newByteArray[x, y] = newByteArray[x + 1, y] = newByteArray[x + 2, y] = (byte)average;
                }
            }
            return ByteArrayToBitmap(newByteArray);
        }
     
        public static Bitmap MedianFilter(Bitmap bmp, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            var byteArray = ImageTo2DByteArray(bmp);
            var newByteArray = new byte[byteArray.GetLength(0), byteArray.GetLength(1)];

            range /= 2;
            List<int> reds = new List<int>();
            List<int> greens = new List<int>();
            List<int> blues = new List<int>();

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < byteArray.GetLength(0); x += 3)
                {
                    reds.Clear();
                    greens.Clear();
                    blues.Clear();

                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < byteArray.GetLength(1))
                            for (int xx = x - range * 3; xx < x + range * 3 - 2; xx += 3)
                            {
                                if (xx >= 0 && xx < byteArray.GetLength(0))
                                {
                                    reds.Add(byteArray[xx, yy]);
                                    greens.Add(byteArray[xx + 1, yy]);
                                    blues.Add(byteArray[xx + 2, yy]);
                                }
                            }
                    }

                    reds.Sort();
                    greens.Sort();
                    blues.Sort();

                    newByteArray[x, y]      = (byte)reds[reds.Count() / 2];
                    newByteArray[x + 1, y]  = (byte)greens[greens.Count() / 2];
                    newByteArray[x + 2, y]  = (byte)blues[blues.Count() / 2];
                }
            }

            return ByteArrayToBitmap(newByteArray);
        }
   
        public static Bitmap PixelateImage(Bitmap bmp, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            var byteArray = ImageTo2DByteArray(bmp);
            var newByteArray = new byte[byteArray.GetLength(0), byteArray.GetLength(1)];

            range /= 2;
            int averageRed = 0;
            int averageGreen = 0;
            int averageBlue = 0;
            int counter = 0;

            for (int y = 0; y < byteArray.GetLength(1); y += range)
            {
                for (int x = 0; x < byteArray.GetLength(0); x += range * 3)
                {
                    averageRed = 0;
                    averageGreen = 0;
                    averageBlue = 0;
                    counter = 0;

                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < byteArray.GetLength(1))
                            for (int xx = x - range * 3; xx <= x + range * 3; xx += 3)
                            {
                                if (xx >= 0 && xx < byteArray.GetLength(0))
                                {
                                    averageRed += byteArray[xx, yy];
                                    averageGreen += byteArray[xx + 1, yy];
                                    averageBlue += byteArray[xx + 2, yy];
                                    counter++;
                                }
                            }
                    }

                    averageRed /= counter;
                    averageGreen /= counter;
                    averageBlue /= counter;

                     for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < byteArray.GetLength(1))
                            for (int xx = x - range * 3; xx <= x + range * 3; xx += 3)
                            {
                                if (xx >= 0 && xx < byteArray.GetLength(0))
                                {
                                    newByteArray[xx, yy] = (byte)averageRed;
                                    newByteArray[xx + 1, yy] = (byte)averageGreen;
                                    newByteArray[xx + 2, yy] = (byte)averageBlue;
                                }
                            }
                    }
                }
            }
            return ByteArrayToBitmap(newByteArray);
        }
      
        public static Bitmap KuwaharaFilter(Bitmap bmp, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            var byteArray = ImageTo2DByteArray(bmp);
            var newByteArray = new byte[byteArray.GetLength(0), byteArray.GetLength(1)];
            range /= 2;

            for (int y = range; y < byteArray.GetLength(1) - range; y++)
            {
                for (int x = range * 3; x < byteArray.GetLength(0) - range * 3; x+=3)
                {
                    int[] reds0, reds1, reds2, reds3, greens0, greens1, greens2, greens3, blues0, blues1, blues2, blues3;
                    reds0 = new int[(range + 1) * (range + 1)];
                    reds1 = new int[(range + 1) * (range + 1)];
                    reds2 = new int[(range + 1) * (range + 1)];
                    reds3 = new int[(range + 1) * (range + 1)];
                    greens0 = new int[(range + 1) * (range + 1)];
                    greens1 = new int[(range + 1) * (range + 1)];
                    greens2 = new int[(range + 1) * (range + 1)];
                    greens3 = new int[(range + 1) * (range + 1)];
                    blues0 = new int[(range + 1) * (range + 1)];
                    blues1 = new int[(range + 1) * (range + 1)];
                    blues2 = new int[(range + 1) * (range + 1)];
                    blues3 = new int[(range + 1) * (range + 1)];

                    int counter0, counter1, counter2, counter3;
                    counter0 = counter1 = counter2 = counter3 = 0;

                    double meanRed = 0;
                    double meanGreen = 0;
                    double meanBlue = 0;
                    double varianceRed = 2000;
                    double varianceGreen = 2000;
                    double varianceBlue = 2000;

                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        for (int xx = x - range * 3; xx <= x + range * 3; xx += 3)
                        {
                            if (yy <= y && xx <= x)
                            {
                                reds0[counter0] = byteArray[xx, y];
                                greens0[counter0] = byteArray[xx + 1, y];
                                blues0[counter0] = byteArray[xx + 2, y];
                                counter0++;
                            }

                            if (yy <= y && xx >= x)
                            {
                                reds1[counter1] = byteArray[xx, y];
                                greens1[counter1] = byteArray[xx + 1, y];
                                blues1[counter1] = byteArray[xx + 2, y];
                                counter1++;
                            }

                            if (yy >= y && xx <= x)
                            {
                                reds2[counter2] = byteArray[xx, y];
                                greens2[counter2] = byteArray[xx + 1, y];
                                blues2[counter2] = byteArray[xx + 2, y];
                                counter2++;
                            }

                            if (yy >= y && xx >= x)
                            {
                                reds3[counter3] = byteArray[xx, y];
                                greens3[counter3] = byteArray[xx + 1, y];
                                blues3[counter3] = byteArray[xx + 2, y];
                                counter3++;
                            }
                        }
                    }

                    // check for red
                    {
                        if (varianceRed > variance(reds0))
                        {
                            varianceRed = variance(reds0);
                            meanRed = average(reds0);
                        }

                        if (varianceRed > variance(reds1))
                        {
                            varianceRed = variance(reds1);
                            meanRed = average(reds1);
                        }

                        if (varianceRed > variance(reds2))
                        {
                            varianceRed = variance(reds2);
                            meanRed = average(reds2);
                        }

                        if (varianceRed > variance(reds3))
                        {
                            varianceRed = variance(reds3);
                            meanRed = average(reds3);
                        }
                    }

                    // check for green
                    {
                        if (varianceGreen > variance(greens0))
                        {
                            varianceGreen = variance(greens0);
                            meanGreen = average(greens0);
                        }

                        if (varianceGreen > variance(greens1))
                        {
                            varianceGreen = variance(greens1);
                            meanGreen = average(greens1);
                        }

                        if (varianceGreen > variance(greens2))
                        {
                            varianceGreen = variance(greens2);
                            meanGreen = average(greens2);
                        }

                        if (varianceGreen > variance(greens3))
                        {
                            varianceGreen = variance(greens3);
                            meanGreen = average(greens3);
                        }
                    }

                    // check for blue
                    {
                        if (varianceBlue > variance(blues0))
                        {
                            varianceBlue = variance(blues0);
                            meanBlue = average(blues0);
                        }

                        if (varianceBlue > variance(blues1))
                        {
                            varianceBlue = variance(blues1);
                            meanBlue = average(blues1);
                        }

                        if (varianceBlue > variance(blues2))
                        {
                            varianceBlue = variance(blues2);
                            meanBlue = average(blues2);
                        }

                        if (varianceBlue > variance(blues3))
                        {
                            varianceBlue = variance(blues3);
                            meanBlue = average(blues3);
                        }
                    }

                    newByteArray[x, y] = (byte)meanRed;
                    newByteArray[x + 1, y] = (byte)meanGreen;
                    newByteArray[x + 2, y] = (byte)meanBlue;
                }
            }

            return ByteArrayToBitmap(newByteArray);
        }
        
        public static Bitmap LinearFilter(Bitmap bmp, int range, double[,] xDirectionKernel, double[,] yDirectionKernel, int threshold, double? bias)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range = 2;

            for (int y = range; y < bmp.Height - range; ++y)
            {
                for (int x = range; x < bmp.Width - range; ++x)
                {
                    double magX = 0;
                    double magY = 0;

                    for (int yy = 0; yy < 3; yy++)
                    {
                        for (int xx = 0; xx < 3; xx++)
                        {
                            int xn = x + xx - 2;
                            int yn = y + yy - 2;

                            magX += byteArray[xn * 3, yn] * xDirectionKernel[xx, yy];
                            magY += byteArray[xn * 3, yn] * yDirectionKernel[xx, yy];
                        }
                    }

                    int value = bias == null ? (int)Math.Sqrt(magX * magX + magY * magY) : (int)(magX * bias);
                    //value = value > threshold ? 255 : 0;
                    value = value > 255 ? 255 : value < 0 ? 0 : value;
                    newBmp.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            }
            return newBmp;
        }
    
        public static Bitmap LinearColorFilter(Bitmap bmp, int range, double[,] xDirectionKernel, double[,] yDirectionKernel, int threshold, double? bias)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range = 2;

            for (int y = range; y < bmp.Height - range; ++y)
            {
                for (int x = range; x < bmp.Width - range; ++x)
                {
                    double magXred = 0;
                    double magXgreen = 0;
                    double magXblue = 0;

                    double magYred = 0;
                    double magYgreen = 0;
                    double magYblue = 0;

                    for (int yy = 0; yy < 3; yy++)
                    {
                        for (int xx = 0; xx < 3; xx++)
                        {
                            int xn = x + xx - 2;
                            int yn = y + yy - 2;

                            magXred += byteArray[xn * 3, yn] * xDirectionKernel[xx, yy];
                            magXgreen += byteArray[xn * 3 + 1, yn] * xDirectionKernel[xx, yy];
                            magXblue += byteArray[xn * 3 + 2, yn] * xDirectionKernel[xx, yy];

                            magYred += byteArray[xn * 3, yn] * yDirectionKernel[xx, yy];
                            magYgreen += byteArray[xn * 3 + 1, yn] * yDirectionKernel[xx, yy];
                            magYblue += byteArray[xn * 3 + 2, yn] * yDirectionKernel[xx, yy];
                        }
                    }

                    int valueRed = (int)Math.Sqrt(magXred * magXred + magYred * magYred);
                    int valueGreen = (int)Math.Sqrt(magXgreen * magXgreen + magYgreen * magYgreen);
                    int valueBlue = (int)Math.Sqrt(magXblue * magXblue + magYblue * magYblue);

                    valueRed = valueRed > 255 ? 255 : valueRed < 0 ? 0 : valueRed;
                    valueGreen = valueGreen > 255 ? 255 : valueGreen < 0 ? 0 : valueGreen;    
                    valueBlue = valueBlue > 255 ? 255 : valueBlue < 0 ? 0 : valueBlue;  

                    newBmp.SetPixel(x, y, Color.FromArgb(valueRed, valueGreen, valueBlue));
                }
            }
            return newBmp;
        }
   
        public static Bitmap MinRgb(Bitmap bmp)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int red =   byteArray[x * 3, y];
                    int green = byteArray[x * 3 + 1, y];
                    int blue =  byteArray[x * 3 + 2, y];

                    int min = Math.Min(red, Math.Min(green, blue));

                    red = red == min ? red : 0;
                    green = green == min ? green : 0;
                    blue = blue == min ? blue : 0;  

                    newBmp.SetPixel(x, y, Color.FromArgb(red, green, blue));
                }
            }
             
            return newBmp;
        }

    }
}
