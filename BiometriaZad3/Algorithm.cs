using System;
using System.Collections.Generic;
using System.Drawing;
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
        public static Bitmap ImageToBinaryImage(Bitmap bmp)
        {
            Bitmap newBmp = new Bitmap(bmp);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    var meanValue = (pixel.R + pixel.G + pixel.B) / 3;

                    Color newPixel = Color.FromArgb(meanValue, meanValue, meanValue);
                   
                    newBmp.SetPixel(x, y, newPixel);
                }
            }
            return newBmp;
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
