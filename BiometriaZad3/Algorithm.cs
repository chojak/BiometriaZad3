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
            System.Diagnostics.Debug.WriteLine(bmp.PixelFormat);

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

        public static Bitmap Fill(Bitmap bmp, System.Windows.Point position, Color color, int tolerance, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            position.X = Math.Floor(position.X);
            position.Y = Math.Floor(position.Y);

            var byteArray = ImageTo2DByteArray(bmp);
            var newByteArray = ImageTo2DByteArray(bmp);
            int counter = 0;
            bool isRangeEnabled = range == 0 ? false : true;


            var pixelRed = byteArray[(int)position.X * 3, (int)position.Y];
            var pixelGreen = byteArray[(int)position.X * 3 + 1, (int)position.Y];
            var pixelBlue = byteArray[(int)position.X * 3 + 2, (int)position.Y];

            HashSet<System.Windows.Point> pixelsToChange = new HashSet<System.Windows.Point>();
            Stack<System.Windows.Point> neighbours = new Stack<System.Windows.Point>();

            neighbours.Push(new System.Windows.Point((double)(int)position.X, (double)(int)position.Y));

            while (neighbours.Count > 0)
            {
                var pixel = neighbours.Pop();
                
                pixelsToChange.Add(pixel);

                if (isRangeEnabled)
                    if (counter < range)
                        counter++;
                    else
                        break;

                System.Windows.Point? topNeighbour = pixel.Y > 0 ? new System.Windows.Point(pixel.X, pixel.Y -1) : null;
                System.Windows.Point? bottomNeighbour = byteArray.GetLength(1) - 1 > pixel.Y ? new System.Windows.Point(pixel.X, pixel.Y + 1) : null;
                System.Windows.Point? leftNeighbour = pixel.X > 0 ? new System.Windows.Point(pixel.X - 1, pixel.Y) : null;
                System.Windows.Point? rightNeighbour = byteArray.GetLength(0) / 3 - 1> pixel.X ? new System.Windows.Point(pixel.X + 1, pixel.Y) : null;

                if (topNeighbour.HasValue && !pixelsToChange.Contains(topNeighbour.Value))
                {
                    int currentRed = byteArray[(int)(topNeighbour.Value.X * 3), (int)topNeighbour.Value.Y];
                    int currentGreen = byteArray[(int)(topNeighbour.Value.X * 3) + 1, (int)topNeighbour.Value.Y];
                    int currentBlue = byteArray[(int)(topNeighbour.Value.X * 3) + 2, (int)topNeighbour.Value.Y];

                    if (Math.Abs(currentRed - pixelRed) < tolerance && 
                        Math.Abs(currentGreen - pixelGreen) < tolerance &&
                        Math.Abs(currentBlue - pixelBlue) < tolerance)
                    {
                        neighbours.Push((System.Windows.Point)topNeighbour);
                    }
                }

                if (leftNeighbour.HasValue && !pixelsToChange.Contains(leftNeighbour.Value))
                {
                    int currentRed = byteArray[(int)(leftNeighbour.Value.X * 3), (int)leftNeighbour.Value.Y];
                    int currentGreen = byteArray[(int)(leftNeighbour.Value.X * 3) + 1, (int)leftNeighbour.Value.Y];
                    int currentBlue = byteArray[(int)(leftNeighbour.Value.X * 3) + 2, (int)leftNeighbour.Value.Y];

                    if (Math.Abs(currentRed - pixelRed) < tolerance &&
                        Math.Abs(currentGreen - pixelGreen) < tolerance &&
                        Math.Abs(currentBlue - pixelBlue) < tolerance)
                    {
                        neighbours.Push((System.Windows.Point)leftNeighbour);
                    }
                }

                if (bottomNeighbour.HasValue && !pixelsToChange.Contains(bottomNeighbour.Value))
                {
                    int currentRed = byteArray[(int)(bottomNeighbour.Value.X * 3), (int)bottomNeighbour.Value.Y];
                    int currentGreen = byteArray[(int)(bottomNeighbour.Value.X * 3) + 1, (int)bottomNeighbour.Value.Y];
                    int currentBlue = byteArray[(int)(bottomNeighbour.Value.X * 3) + 2, (int)bottomNeighbour.Value.Y];

                    if (Math.Abs(currentRed - pixelRed) < tolerance &&
                        Math.Abs(currentGreen - pixelGreen) < tolerance &&
                        Math.Abs(currentBlue - pixelBlue) < tolerance)
                    {
                        neighbours.Push((System.Windows.Point)bottomNeighbour);
                    }
                }

                if (rightNeighbour.HasValue && !pixelsToChange.Contains(rightNeighbour.Value))
                {
                    int currentRed = byteArray[(int)(rightNeighbour.Value.X * 3), (int)rightNeighbour.Value.Y];
                    int currentGreen = byteArray[(int)(rightNeighbour.Value.X * 3) + 1, (int)rightNeighbour.Value.Y];
                    int currentBlue = byteArray[(int)(rightNeighbour.Value.X * 3) + 2, (int)rightNeighbour.Value.Y];

                    if (Math.Abs(currentRed - pixelRed) < tolerance &&
                        Math.Abs(currentGreen - pixelGreen) < tolerance &&
                        Math.Abs(currentBlue - pixelBlue) < tolerance)
                    {
                        neighbours.Push((System.Windows.Point)rightNeighbour);
                    }
                }
            }

            foreach (var pixel in pixelsToChange)
            {
                newByteArray[(int)(pixel.X * 3), (int)pixel.Y] = color.R;
                newByteArray[(int)(pixel.X * 3) + 1, (int)pixel.Y] = color.G;
                newByteArray[(int)(pixel.X * 3) + 2, (int)pixel.Y] = color.B;
            }
            return ByteArrayToBitmap(newByteArray);
        }
        
        public static Bitmap FillAll(Bitmap bmp, System.Windows.Point position, Color color, int tolerance, int range)
        {
            if (bmp == null)
            {
                return new Bitmap(1, 1);
            }

            position.X = Math.Floor(position.X);
            position.Y = Math.Floor(position.Y);

            var byteArray = ImageTo2DByteArray(bmp);
            var newByteArray = ImageTo2DByteArray(bmp);
            int counter = 0;
            bool isRangeEnabled = range == 0 ? false : true;


            var pixelRed = byteArray[(int)position.X * 3, (int)position.Y];
            var pixelGreen = byteArray[(int)position.X * 3 + 1, (int)position.Y];
            var pixelBlue = byteArray[(int)position.X * 3 + 2, (int)position.Y];

            for (int y = 0; y < byteArray.GetLength(1); y++)
            {
                for (int x = 0; x < byteArray.GetLength(0); x += 3)
                {
                    var currentRed = byteArray[x, y];
                    var currentGreen = byteArray[x + 1, y];
                    var currentBlue = byteArray[x + 2, y];

                    if (Math.Abs(currentRed - pixelRed) < tolerance &&
                       Math.Abs(currentGreen - pixelGreen) < tolerance &&
                       Math.Abs(currentBlue - pixelBlue) < tolerance)
                    {
                        newByteArray[x, y] = color.R;
                        newByteArray[x + 1, y] = color.G;
                        newByteArray[x + 2, y] = color.B;
                        counter++;

                        if (isRangeEnabled && counter == range)
                            return ByteArrayToBitmap(newByteArray);
                    }
                }
            }
            return ByteArrayToBitmap(newByteArray);
        }
    }
}
