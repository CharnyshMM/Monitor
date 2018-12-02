using Monitor.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Timers;
using System.Diagnostics;

namespace Monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProcessesViewModel ProcessesViewModel;
        Timer Timer;
        public MainWindow()
        {
            InitializeComponent();
            ProcessesViewModel = new ProcessesViewModel();
            DataContext = ProcessesViewModel;

            var Timer = new Timer(2000);
            Timer.Elapsed += (obj, args) =>
            {
                ProcessesViewModel.Update();
            };

            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        private void EndClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button is null)
                return;

            int id = (int) button.Tag;
            try
            {
                
                Process.GetProcessById(id).Kill();
                ProcessesViewModel.Update();
                
            } catch (ArgumentException exc)
            {
                MessageBox.Show("Process is being ended!", "It's all right");
            }

        }
    }
}
