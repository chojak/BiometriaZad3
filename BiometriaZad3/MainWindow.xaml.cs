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
                Image.Width = BinaryBitmap.Width;
                Image.Height = BinaryBitmap.Height;
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

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.GetPosition(Image));

            if (GlobalCheckBox.IsChecked != true)
                Image.Source = Algorithm.BitmapToImageSource(Algorithm.Fill(OriginalBitmap, e.GetPosition(Image), System.Drawing.Color.FromArgb(0, 0, 0), (int)ToleranceSlider.Value, int.Parse(RangeTextbox.Text)));
            else
                Image.Source = Algorithm.BitmapToImageSource(Algorithm.FillAll(OriginalBitmap, e.GetPosition(Image), System.Drawing.Color.FromArgb(0, 0, 0), (int)ToleranceSlider.Value, int.Parse(RangeTextbox.Text)));
        }

        private void ToleranceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ToleranceLabel.Content = "Tolerance: " + Math.Floor(ToleranceSlider.Value).ToString();
        }
    }
}
