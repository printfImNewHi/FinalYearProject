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
using Microsoft.SharePoint.Client;
using System.Security;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for GetSharpointInfo.xaml
    /// </summary>
    public partial class GetSharpointInfo : Window
    {
        public GetSharpointInfo()
        {
            InitializeComponent();
        }

        public void getInfo()
        {


            // Starting with ClientContext, the constructor requires a URL to the 
            // server running SharePoint. 
            using (ClientContext context = new ClientContext("https://galwaymayoinstitute-my.sharepoint.com/personal/10019488_gmit_ie/"))
            {
                try
                {
                    SecureString passWord = new SecureString();
                    foreach (char c in "ZqzaQG$8".ToCharArray()) passWord.AppendChar(c);//password is ZqzaQG$8
                    context.Credentials = new SharePointOnlineCredentials("10019488@gmit.ie", passWord);

                    // The SharePoint web at the URL.
                    Web web = context.Web;

                    // Retrieve all lists from the server. 
                    context.Load(web.Lists, lists => lists.Include(list => list.Title,  list => list.Id));// For each list, retrieve Title and Id. 
         
                    // Execute query. 
                    context.ExecuteQuery();

                    // Enumerate the web.Lists. 
                    foreach (Microsoft.SharePoint.Client.List list in web.Lists)
                    {
                        txtInfo.Text = txtInfo.Text + ", " + list.Title + "\n";
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to get Info");
                }
            }
        
        }

        private void btnGetInfo_Click(object sender, RoutedEventArgs e)
        {
            getInfo();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
