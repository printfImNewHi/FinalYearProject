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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {
        string username;
        string domain;
        string password;


        public LogIn()
        {
            InitializeComponent();
        }

        public string getUsername()
        {
            username = txtUsername.Text;
            return username;
        }

        public string getDomain()
        {
            domain = txtDomain.Text;
            return domain;
        }

        public string getPassword()
        {
            password = txtPassword.Text;
            return password;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
    }
}
