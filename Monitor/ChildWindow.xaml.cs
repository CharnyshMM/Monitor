using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Monitor
{
    /// <summary>
    /// Interaction logic for ChildWindow.xaml
    /// </summary>
    public partial class ChildWindow : Window
    {
        public ChildWindow()
        {
            InitializeComponent();
            this.Closed += (obj, args) =>
            {
                Properties.Settings.Default.Save();
            };
        }

        private void SnapshotsPathTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "txt";
            openFileDialog.CheckFileExists = false;
            openFileDialog.Title = "Select or type new file name to write snapshots to";
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                if (fileName != null || fileName != "")
                {
                    Properties.Settings.Default.SnapshotsPath = openFileDialog.FileName;
                }
            }
        }
    }
}
