using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BiometriaZad3
{
    public class Algorithm
    {
        public static byte[,] ImageTo2DByteArray(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            byte[] bytes = new byte[height * data.Stride];
            try
            {
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            byte[,] result = new byte[height, width];
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    int offset = y * data.Stride + x * 3;
                    result[y, x] = (byte)((bytes[offset + 0] + bytes[offset + 1] + bytes[offset + 2]) / 3);
                }
            return result;
        }
        private static Bitmap ByteArrayToBitmap(byte[] rgbValues, Bitmap bmp)
        {
            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height * 3, bmp.PixelFormat);  
            Rectangle rect = new Rectangle(0, 0, newBmp.Width, newBmp.Height);
            BitmapData bmpData = newBmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);

            newBmp.UnlockBits(bmpData);

            System.Diagnostics.Debug.WriteLine(newBmp.Width + " " + newBmp.Height);

            return newBmp;
        }
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
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
        public static Bitmap ImageToWhiteBlack(Bitmap bmp)
        {
            var byteBmp = ImageTo2DByteArray(bmp);

            byte[] tablicabajtuw = new byte[byteBmp.GetLength(0) * byteBmp.GetLength(1) * 3 * 3];

            for (int y = 0; y < byteBmp.GetLength(0) * 3; y+=3)
            {
                for (int x = 0; x < byteBmp.GetLength(1) * 3 - 3; x+=3)
                {
                    tablicabajtuw[y * byteBmp.GetLength(1) + x] =
                    tablicabajtuw[y * byteBmp.GetLength(1) + x + 1] = 
                    tablicabajtuw[y * byteBmp.GetLength(1) + x + 2] = byteBmp[y / 3, x / 3];
                }
            }

            return ByteArrayToBitmap(tablicabajtuw, bmp);
        }
        public static Bitmap Bernsen(Bitmap bmp, int range, int limit)
        {
            if (bmp == null)
            {
                return null;
            }

            Bitmap newBitmap = new Bitmap(bmp);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int leftTopX = (x - range / 2) >= 0 ? (x - range / 2) : 0;
                    int leftTopY = (y - range / 2) >= 0 ? (y - range / 2) : 0;

                    leftTopX = (x + range / 2) > bmp.Width ? (leftTopX - (x + range / 2)) : leftTopX;
                    leftTopY = (y + range / 2) > bmp.Height ? (leftTopY - (y + range / 2)) : leftTopY;

                    int rangeX = (x - range / 2) >= 0 ? range : range + (x - range / 2);
                    int rangeY = (y - range / 2) >= 0 ? range : range + (y - range / 2);

                    rangeX = (x + range / 2) > bmp.Width ? (rangeX - ((x + range / 2) - bmp.Width)) : rangeX;
                    rangeY = (y + range / 2) > bmp.Height ? (rangeY - ((y + range / 2) - bmp.Height - 1)) : rangeY;

                    if ((x + range / 2) > (bmp.Width - 9))
                    {
                        System.Diagnostics.Debug.WriteLine(y);
                    }

                    //var rec = new Rectangle(leftTopX, leftTopY, rangeX, rangeY);
                    //var xd = bmp.Clone(rec, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                }
               
            }
            return newBitmap;
        }
    }
}
