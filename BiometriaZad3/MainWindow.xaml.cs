using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiometriaZad3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string Source;
        Bitmap OriginalBitmap;
        Bitmap BinaryBitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
            if (openFileDialog.ShowDialog() == true)
            {
                Source = openFileDialog.FileName;
                OriginalBitmap = new Bitmap(Source);
                Image.Source = Algorithm.BitmapToImageSource(OriginalBitmap);
            }
        }

        private void OriginalImage_Click(object sender, RoutedEventArgs e)
        {
            BinaryBitmap = new Bitmap(OriginalBitmap);
            Image.Source = Algorithm.BitmapToImageSource(BinaryBitmap);
        }

        private void BinaryImage_Click(object sender, RoutedEventArgs e)
        {
            BinaryBitmap = Algorithm.ImageToBinaryImage(OriginalBitmap, (byte)Math.Floor(ThresholdSlider.Value));
            Image.Source = Algorithm.BitmapToImageSource(BinaryBitmap);
        }

        private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ThresholdTextBlock.Text = "Threshold: " + Math.Floor(ThresholdSlider.Value).ToString();
        }

        private void Otsu_Click(object sender, RoutedEventArgs e)
        {
            int threshold = Algorithm.Otsu(OriginalBitmap);

            ThresholdSlider.Value = threshold;
            ThresholdTextBlock.Text = "Threshold: " + Math.Floor(ThresholdSlider.Value).ToString();
        }

        private void KMM_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.KMM_Thinning(BinaryBitmap));
        }
    }
}
