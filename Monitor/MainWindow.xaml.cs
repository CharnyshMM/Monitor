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
using System.IO;
using Monitor.Utilities;

namespace Monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static string[] Priority = { "Idle4", "Normal8", "High13", "Realtime24" };


        ProcessesViewModel ProcessesViewModel;
        ProcessWatchesViewModel ProcessWatchesViewModel;
        Timer Timer;
        public MainWindow()
        {
            InitializeComponent();
            ProcessesViewModel = new ProcessesViewModel();
            ProcessWatchesViewModel = new ProcessWatchesViewModel();
            allProcessesTab.DataContext = ProcessesViewModel;
            watchesTab.DataContext = ProcessWatchesViewModel;

            Timer = new Timer(2000);
            Timer.Elapsed += (obj, args) =>
            {
                ProcessesViewModel.Update();
                ProcessWatchesViewModel.Update();
            };

            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        private async void MakeSnapshot()
        {
            var snapshot = ProcessWatchesViewModel.ProcessDictionary.Values.ToList();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"=======Snapshot {DateTime.Now}=======");
            foreach (var p in snapshot)
            {
                string memory = "";
                
                stringBuilder.Append($"ID {p.Id} :");
                if (!p.HasExited)
                {
                    if (Properties.Settings.Default.HumanReadableMemory)
                    {
                        memory = BytesToHumanReadableStrConverter.ConvertToHumanReadableString(p.PrivateMemorySize64);
                    }
                    else
                    {
                        memory = p.PrivateMemorySize64.ToString();
                    }
                    stringBuilder.Append($"=running= {p.ProcessName} | Mem = {memory} | {p.Responding}");
                }
                else
                {
                    stringBuilder.Append($"=EXITED= | ExitCode {p.ExitCode} at {p.ExitTime}");
                }
                stringBuilder.AppendLine();
            }
            //Properties.Settings.Default.SnapshotsPath;
            using (var file = new StreamWriter(Properties.Settings.Default.SnapshotsPath, true))
            {
                await file.WriteAsync(stringBuilder.ToString());
            }

        }

        private void EndClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            try
            {
                int id = (int) button.Tag;
                if (id == Process.GetCurrentProcess().Id)
                {
                    MessageBox.Show("Oh no! I don't want to terminate myself. Please just close me", "Monitor", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Process.GetProcessById(id).Kill();
                ProcessesViewModel.Update();
                
            }
            catch (ArgumentException exc)
            {
                MessageBox.Show("Process is being ended!", "It's all right");
            }

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            int id = (int)checkBox.Tag;
            if (ProcessWatchesViewModel.ProcessDictionary.ContainsKey(id))
            {
                checkBox.IsChecked = true;
            }
            else
            {
                checkBox.IsChecked = false;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            int id = (int)checkBox.Tag;
            if (ProcessWatchesViewModel.ProcessDictionary.ContainsKey(id))
            {
                checkBox.IsChecked = false;
                ProcessWatchesViewModel.RemoveWatchedProcess(id);
            }
            else
            {
                checkBox.IsChecked = true;
                ProcessWatchesViewModel.AddProcessToWatch(id);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            ProcessWatchesViewModel.RemoveWatchedProcess((int)button.Tag);
            ProcessWatchesViewModel.Update();
        }

        private void Snapshot_Click(object sender, RoutedEventArgs e)
        {
            MakeSnapshot();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var sets = new ChildWindow();
            sets.ShowDialog();
        }
    }
}
