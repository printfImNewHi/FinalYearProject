using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WIA;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Security;
using System.Drawing;
using OnBarcode.Barcode.BarcodeScanner;
using Microsoft.VisualStudio.SharePoint;
using Microsoft.SharePoint.Client;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;




namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Folder variable and file list
        List<string> tempList = new List<string>();
        string folderName = @"C:\ScannerFolder";
        string tempFolderName = @"C:\TempScannerFolder";  
        Stopwatch sw = new Stopwatch();
        
        //Initialize
        public MainWindow()
        {
            InitializeComponent();

            if (Directory.Exists(folderName))
            {
                DeleteDirectory(folderName);
            }
        }

        //**********Button and List Click Events***************************
        //*****************************************************************

        //List Devices
        private void listDevices_Click(object sender, RoutedEventArgs e)
        {
            // Clear the ListBox.
            lbDevices.Items.Clear();
            // Create a DeviceManager instance
            var deviceManager = new DeviceManager();
            this.txtProgress.Text = "Only Scanners appear in List";
            // Loop through the list of devices and add the name to the listbox
            for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++)
            {
                //Add the device to the list if it is a scanner
                if (deviceManager.DeviceInfos[i].Type != WiaDeviceType.ScannerDeviceType)
                {
                    continue;
                }
                lbDevices.Items.Add(new Scanner(deviceManager.DeviceInfos[i]));
            }
        }

        //Scanner Click Event
        private void btnScan_Click(object sender, EventArgs e)
        {
            try
            {
                // Select the scanner from list
                var device = lbDevices.SelectedItem as Scanner;
                this.txtProgress.Text="You selected "+device+" as your Scanner";
                if (device == null)
                {
                    System.Windows.MessageBox.Show("Please select a device.", "Warning");
                    return;
                }
                // Scan
                var image = device.Scan();
                //List to hold images
                List<WIA.ImageFile> wiaImagesList = new List<WIA.ImageFile>();
                //Add to list
                wiaImagesList.Add(image);
                if (wiaImagesList.Count() == 0)
                { return; }
                this.txtProgress.Text = "Scanning in Progress...........";
                //creat pdf
                PdfDocument doc = new PdfDocument();
                string destination = @"C:\ScannerFolder\" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".pdf";
                string tempDestination = @"C:\TempScannerFolder" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".pdf";

                //create folder to store image
                System.IO.Directory.CreateDirectory(folderName);
                System.IO.Directory.CreateDirectory(tempFolderName);

                foreach (WIA.ImageFile img in wiaImagesList)
                {
                    //For each image to be scanned 
                    string tempFilename = @"C:\ScannerFolder\" + rtbResult.Text + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".tiff";
                    string filename = @"C:\TempScannerFolder\" + rtbResult.Text + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".tiff";// can get barcode from rtbResult.Text to save image as barcode 
                    
                    //save the tiff
                    image.SaveFile(filename);

                    //add image to the pdf and Custom user control and save
                    doc.Pages.Add(new PdfPage());
                    XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[0]);
                    XImage imgX = XImage.FromFile(filename);
                    xgr.DrawImage(imgX, 0, 0);

                    //save to destinatin folder
                    doc.Save(destination);

                    //doc.Save(tempFilename);
                    showPath.Text += destination;
                    tempList.Add(destination);
                    doc.Close();

                    this.txtProgress.Text = "Scanning complete, converting to PDF";

                    // User control to display pdf
                    var uc = new UserControl1(destination);
                    this.windowsFormsHost1.Child = uc;
                }

                //Convert Wia imageFile to a bitmap
                Bitmap bmp = ConvertImageToBitmap(image);
                DateTime dtStart = DateTime.Now;

                //read barcode form Bitmap
                string[] barcode = ReadBarcodeFromBitmap(bmp);

                // Show the results in a message box
                string result = string.Empty;
                if (barcode != null)
                {
                    foreach (String code in barcode){
                        result += code;
                        }
                    }
                    else{
                        result += "Failed to find a barcode.";
                    }
                    //Show barcode 
                    rtbResult.Text = result;
                    GetDocumentType();
                 }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("A scanner error occoured, check feeder");
            }
        }

        //Send to Sharpoint Click event, calls the SaveFileToSharePoint method
        private void btnSendToShare_Click(object sender, RoutedEventArgs e)
        {
            //Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate{txtProgress.SetValue(System.Windows.Controls.TextBlock.TextProperty, "50% Complete");}, null);
            this.txtProgress.Text = "Sending to Office 365 / Sharepoint";

            //Run a background worker to perform our task
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(10000);
            worker.Dispose();
            worker = null;
       
        }

        //Upload from system, via a file picker
        private void btnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            //TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //CancellationToken cancelationToken = new CancellationToken();

            string filePath = fileResult.Text;
            SaveFileToSharePoint(filePath);
 
        }

        //Open Preview scans window
        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            PreviewImage preview = new PreviewImage();
            preview.Show();
        }

      
        //*********************Methods*****************************
        //*********************************************************

        //Convert image to a PDF Document
        private static Bitmap ConvertImageToBitmap(ImageFile image)
        {
            byte[] buffer = (byte[])image.FileData.get_BinaryData();
            MemoryStream ms = new MemoryStream(buffer);
            Bitmap bmp = new Bitmap(System.Drawing.Image.FromStream(ms));
            return bmp;
        }

        //Read barcode from bitmap
        private String[] ReadBarcodeFromBitmap(Bitmap _bimapimage)
        {
            this.txtProgress.Text = "Reading Barcode......";
            System.Drawing.Bitmap objImage = _bimapimage;
            String[] barcodes = BarcodeScanner.Scan(objImage, BarcodeType.Code39);
            return barcodes;
        }

        //Delete the temp directory 
        public static  void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        //Save file path to Sharepoint
        public void SaveFileToSharePoint(string fileName)
        {
            //Login object created, variable passed passed. eg "ZqzaQG$8" will be come login.getPassword();
            //LogIn login = new LogIn();
            //login.Show();
            //login.getUsername();
            //login.getPassword();
            //login.getDomain();

            //Connect to Sharepoint Site, passing username, password and domain.
            using (ClientContext clientContext = new ClientContext("https://galwaymayoinstitute-my.sharepoint.com/personal/10019488_gmit_ie/"))
            {
                try
                {
                    SecureString passWord = new SecureString();
                    foreach (char c in "ZqzaQG$8".ToCharArray()) passWord.AppendChar(c);//password is ZqzaQG$8
                    clientContext.Credentials = new SharePointOnlineCredentials("10019488@gmit.ie", passWord);
                    Web web = clientContext.Web;
                    FileCreationInformation newFile = new FileCreationInformation();
                    newFile.Content = System.IO.File.ReadAllBytes(fileName);

                    //Name document as Time Stamp, with barcode appended at end
                    newFile.Url = "Uploaded via client on " + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + rtbResult.Text;
                    Microsoft.SharePoint.Client.List docs = web.Lists.GetByTitle("Documents");
                    Microsoft.SharePoint.Client.File uploadFile = docs.RootFolder.Files.Add(newFile);
                    clientContext.ExecuteQuery();
                    System.Windows.MessageBox.Show("Successfull upload to Office 365");

                }
                catch
                {
                    System.Windows.MessageBox.Show("Failed to upload to Office 365");
                }
            }
        }

        //Background worker saves files to Sharepoint
        public void WorkerSaveFileToSharePoint()
        {
            //Update values from thread dispacher
            //Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate{progressBar.SetValue(System.Windows.Controls.ProgressBar.ValueProperty,.5);}, null);
           
            //Login object created, variable passed passed. eg "ZqzaQG$8" will be come login.getPassword();
            //LogIn login = new LogIn();
            //login.Show();
            //login.getUsername();
            //login.getPassword();
            //login.getDomain();

            foreach (string path in tempList)
            {
                string fileName = path;

                //Connect to Sharepoint Site, passing username, password and domain.
                using (ClientContext clientContext = new ClientContext("https://galwaymayoinstitute-my.sharepoint.com/personal/10019488_gmit_ie/"))
                {
                    try
                    {
                        SecureString passWord = new SecureString();
                        foreach (char c in "ZqzaQG$8".ToCharArray()) passWord.AppendChar(c);//password is ZqzaQG$8
                        clientContext.Credentials = new SharePointOnlineCredentials("10019488@gmit.ie", passWord);
                        Web web = clientContext.Web;
                        
                        FileCreationInformation newFile = new FileCreationInformation();
                        newFile.Content = System.IO.File.ReadAllBytes(fileName);

                        //Name document as Time Stamp, with barcode appended at end
                        newFile.Url = "Uploaded via client on " + DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                        Microsoft.SharePoint.Client.List docs = web.Lists.GetByTitle("Documents");
                        Microsoft.SharePoint.Client.File uploadFile = docs.RootFolder.Files.Add(newFile);
                        clientContext.ExecuteQuery();
                        System.Windows.MessageBox.Show("Successfull upload to Office 365");

                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("Failed to upload to Office 365");
                    }
                }
                //tempList.Remove(this.path);
            }
           
        }

        //Dispose of Thread
        private static void WorkerDisposed(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("Private Worker Disposed");
        }

        //Background Worker progress changed event
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        //Background worker to send files in list to sharepoint
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Background worker, sends all files from the tempList
            WorkerSaveFileToSharePoint();
        }

        //Event raised when the backgroud worker has finished
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Windows.MessageBox.Show("Background worker complete");
        }

        //Progress bar 
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = (int)e.ProgressPercentage * 10;
            sw.Start();
            txtProgress.Text = (int)-(int)(sw.ElapsedMilliseconds / 1000) + " Secs. Remaining";

            if (progressBar.Value == 100)
            {
                progressBar.Value = 0;
                txtProgress.Text = "Proccessing";
                sw.Reset();
                sw.Stop();
            }
        }

        //Open file dialog picker
        private void OpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog(); 

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                fileResult.Text = filename;
            }
        }

        //Calculate Document type, based on a formula that work on the Barcode(Needed from Ice-Cube)
        private void GetDocumentType()
        {
            //Read barcode
            string barcode = rtbResult.Text;

            //need barcode forumla from ice*********************
            string br =  barcode.Last().ToString();
 
            switch (br)
            {
                case "T":
                    System.Windows.Forms.MessageBox.Show("From of Nomination scanned, X more pages should be processed.");
                    break;
                case "G":
                    System.Windows.Forms.MessageBox.Show("Application for Membership scanned, X more pages should be processed.");
                    break;
                case "l":
                    System.Windows.Forms.MessageBox.Show("Loan Application scanned, X more pages should be processed.");
                    break;
                case "d":
                    System.Windows.Forms.MessageBox.Show("Driveing Licence scanned, X more pages should be processed.");
                    break;
                case "x":
                    System.Windows.Forms.MessageBox.Show("X scanned, X more pages should be processed.");
                    break;
            }

        }

        //Close Application
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Open Sharepoint Document Window
        private void GetDocumentList_Click(object sender, RoutedEventArgs e)
        {
            GetSharpointInfo info = new GetSharpointInfo();
            info.Show();
        }

        //Clear the list of scans
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtProgress.Text = "Clearing list..........";
            try
            {
                foreach (string path in tempList)
                {
                    tempList.Remove(path);
                }
                System.Windows.MessageBox.Show("Scan list cleared. Ready to start again.");
            }
            catch(Exception)
            {
                this.txtProgress.Text = "Error, either list is empty or the is a problem.."; 
            }
        }

    }
}

