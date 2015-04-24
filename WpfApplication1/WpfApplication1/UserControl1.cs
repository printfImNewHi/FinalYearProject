using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WIA;

namespace WpfApplication1
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1(String fileName)
        {
            InitializeComponent();

            this.axAcroPDF1.LoadFile(fileName);
            //axAcroPDF1.setShowToolbar(true);
            
        }
    }
}
