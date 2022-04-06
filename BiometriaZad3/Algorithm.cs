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
            if(numbers.Length <= 1)
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
            if(numbers.Length <= 1)
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
            List<int> reds = new List<int>();
            List<int> greens = new List<int>();
            List<int> blues = new List<int>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    reds.Clear();
                    greens.Clear();
                    blues.Clear();  

                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < bmp.Height)
                            for (int xx = (x - range) * 3; xx <= (x + range) * 3; xx+=3)
                            {
                                if (xx >= 0 && xx < bmp.Width * 3)
                                {
                                    reds.Add(byteArray[xx, y]);
                                    greens.Add(byteArray[xx + 1, y]);
                                    blues.Add(byteArray[xx + 2, y]);
                                }
                            }
                    }
                    reds.Sort();
                    greens.Sort();
                    blues.Sort();

                    newBmp.SetPixel(x, y, Color.FromArgb(reds[reds.Count() / 2], greens[greens.Count() / 2], blues[blues.Count() / 2]));        
                }
            }

            return newBmp;
        }
        public static Bitmap PixelateImage(Bitmap bmp, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range /= 2;
            int averageRed = 0;
            int averageGreen= 0;
            int averageBlue = 0;
            int counter = 0;

            for (int y = 0; y < bmp.Height; y+= range)
            {
                for (int x = 0; x < bmp.Width; x += range)
                {
                    averageRed = 0;
                    averageGreen = 0;
                    averageBlue = 0;
                    counter = 0;

                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < bmp.Height)
                            for (int xx = (x - range) * 3; xx <= (x + range) * 3; xx += 3)
                            {
                                if (xx >= 0 && xx < bmp.Width * 3)
                                {
                                    averageRed += byteArray[xx, y];
                                    averageGreen += byteArray[xx + 1, y];
                                    averageBlue += byteArray[xx + 2, y];
                                    counter++;
                                }
                            }
                    }

                    averageRed /= counter;
                    averageGreen /= counter;
                    averageBlue /= counter; 

                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        if (yy >= 0 && yy < bmp.Height)
                            for (int xx = (x - range) * 3; xx <= (x + range) * 3; xx += 3)
                            {
                                if (xx >= 0 && xx < bmp.Width * 3)
                                {
                                    newBmp.SetPixel(xx / 3, yy, Color.FromArgb(averageRed, averageGreen, averageBlue));
                                }
                            }
                    }
                }
            }

            return newBmp;
        }
        public static Bitmap KuwaharaFilter(Bitmap bmp, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
            var byteArray = ImageTo2DByteArray(bmp);
            range /= 2;

            for (int y = range; y < bmp.Height - range; y++)
            {
                for (int x = range; x < bmp.Width - range; x++)
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
                    double varianceRed = 1000;
                    double varianceGreen = 1000;
                    double varianceBlue = 1000;

                    for (int yy = y - range; yy <= y + range; ++yy)
                    {
                        for (int xx = (x - range) * 3; xx <= (x + range) * 3; xx += 3)
                        {
                            if (yy <= y && xx / 3 <= x) 
                            {
                                reds0[counter0] = byteArray[xx, y];
                                greens0[counter0] = byteArray[xx + 1, y];
                                blues0[counter0] = byteArray[xx + 2, y];
                                counter0++;
                            }

                            if (yy <= y && xx / 3 >= x)
                            {
                                reds1[counter1] = byteArray[xx, y];
                                greens1[counter1] = byteArray[xx + 1, y];
                                blues1[counter1] = byteArray[xx + 2, y];
                                counter1++;
                            }

                            if (yy >= y && xx / 3 <= x)
                            {
                                reds2[counter2] = byteArray[xx, y];
                                greens2[counter2] = byteArray[xx + 1, y];
                                blues2[counter2] = byteArray[xx + 2, y];
                                counter2++;
                            }

                            if (yy >= y && xx / 3 >= x)
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

                    newBmp.SetPixel(x, y, Color.FromArgb((int)meanRed, (int)meanGreen, (int)meanBlue));
                }
            }

            return newBmp;
        }
    } 
}
