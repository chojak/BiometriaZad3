﻿using Microsoft.Win32;
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

        private void RangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RangeLabel.Content = "Range: " + Math.Round(RangeSlider.Value, 2);
        }

        private void MedianFilter_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.MedianFilter(OriginalBitmap, (int)RangeSlider.Value));
        }

        private void PixelateFilter_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.PixelateImage(OriginalBitmap, (int)RangeSlider.Value));
        }

        private void KuwaharaFilter_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.KuwaharaFilter(OriginalBitmap, (int)RangeSlider.Value));
        }

        private void SobelFilter_Click(object sender, RoutedEventArgs e)
        {
            double[,] matrixX = new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            double[,] matrixY = new double[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            int threshold = (int)ThresholdSlider.Value;

            if (ColorCheck.IsChecked ?? false)
            {
                Image.Source = Algorithm.BitmapToImageSource(Algorithm.LinearColorFilter(OriginalBitmap, (int)RangeSlider.Value, matrixX, matrixY, threshold));
            }
            else
            {
                Image.Source = Algorithm.BitmapToImageSource(Algorithm.LinearFilter(BinaryBitmap, (int)RangeSlider.Value, matrixX, matrixY, threshold));
            }
        }

        private void MinRGB_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Algorithm.BitmapToImageSource(Algorithm.MinRgb(OriginalBitmap));
        }

        private void PredatorFilter_Click(object sender, RoutedEventArgs e)
        {
            Bitmap tmp = new Bitmap(OriginalBitmap);
            tmp = Algorithm.PixelateImage(OriginalBitmap, (int)RangeSlider.Value);
            tmp = Algorithm.MinRgb(tmp);

            double[,] matrixX = new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            double[,] matrixY = new double[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            int threshold = (int)ThresholdSlider.Value;

            tmp = Algorithm.LinearColorFilter(tmp, (int)RangeSlider.Value, matrixX, matrixY, threshold);

            Image.Source = Algorithm.BitmapToImageSource(tmp);
        }
    }
}
