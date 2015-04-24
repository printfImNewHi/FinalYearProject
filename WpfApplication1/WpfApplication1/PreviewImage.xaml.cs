using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for PreviewImage.xaml
    /// </summary>
    /// 
    public partial class PreviewImage : Window
    {
        //Image list and folder 
        List<Image> lstImages = new List<Image>();
        string folderName = @"C:\ScannerFolder\";
        
        public PreviewImage()
        {
            InitializeComponent();
            DisplayImages();
            DisplayFolder(folderName);
        }

        //Displays the list of files to be scanned 
        public void DisplayFolder(string folderPath)
        {
            try
            {
                string[] files = System.IO.Directory.GetFiles(folderPath);

                for (int x = 0; x < files.Length; x++)
                {
                    listView1.Items.Add(files[x]);
                }
            }
            catch
            {
                MessageBox.Show("Folder Empty");
            }

        }

        //Display list of images from those files
        public void DisplayImages()
        {
            try
            {
                List<string> lstFileNames = new List<string>(System.IO.Directory.EnumerateFiles(folderName, "*.tiff"));
                foreach (string fileName in lstFileNames)
                {
                    //Console.Out.WriteLine("files are " + fileName);
                    //imgTemp = new Image();
                    //imgTemp.Source = new BitmapImage(new Uri(fileName));
                    //imgTemp.Height = imgTemp.Width = 100;
                    //lstImages.Add(imgTemp);
                }
            }
            catch
            {

            }
        }

        //Close window
        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
