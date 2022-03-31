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
                BinaryBitmap = Algorithm.ImageToBinaryImage(OriginalBitmap);
                Image.Source = Algorithm.BitmapToImageSource(OriginalBitmap);
            }
        }

        private void OriginalImage_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(OriginalBitmap);
        }

        private void BinaryImage_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(BinaryBitmap);
        }

        private void BrensenAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.Bernsen(BinaryBitmap, (int)RangeSlider.Value, (int)LimitSlider.Value));
        }

        private void SauvolaAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.Sauvola(BinaryBitmap, (int)RangeSlider.Value, SauvolaKSlider.Value, (int)SauvolaRSlider.Value));
        }
        
        private void NiblackAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.Niblack(BinaryBitmap, (int)RangeSlider.Value, SauvolaKSlider.Value));
        }

        private void PhansalkarAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.Phanskalar(BinaryBitmap, (int)RangeSlider.Value, PhanskalarP.Value, PhanskalarQ.Value, PhanskalarK.Value, PhanskalarR.Value));
        }

        private void RangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RangeLabel.Content = "Range: " + Math.Round(RangeSlider.Value, 2);
        }

        private void LimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrensenLabel.Content = "Limit: " + Math.Round(LimitSlider.Value, 2);    
        }

        private void SauvolaKSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            KLabel.Content = "Sauvola and Niblack k: " + Math.Round(SauvolaKSlider.Value, 2);
        }

        private void SauvolaRSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RLabel.Content = "Savoula R: " + Math.Round(SauvolaRSlider.Value, 2);   
        }

        private void PhanskalarP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PhanskalarPLabel.Content = "P: " + Math.Round(PhanskalarP.Value, 2);
        }

        private void PhanskalarQ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PhanskalarQLabel.Content = "Q: " + Math.Round(PhanskalarQ.Value, 2);
        }
         
        private void PhanskalarK_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PhanskalarKLabel.Content = "K: " + Math.Round(PhanskalarK.Value, 2);
        }
         
        private void PhanskalarR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PhanskalarRLabel.Content = "R: " + Math.Round(PhanskalarR.Value, 2);
        }
    }
}
