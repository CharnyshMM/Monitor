using DrWPF.Windows.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Monitor.ViewModels
{
    class ProcessWatchesViewModel : INotifyPropertyChanged
    {
        protected ObservableDictionary<int, Process> _processDictionary;
        public ObservableDictionary<int, Process> ProcessDictionary
        {
            get
            {
                return _processDictionary;
            }
        }

        protected Process _selectedProcess;
        public Process SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged("SelectedProcess");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void Update()
        {
            foreach (var id in _processDictionary.Keys)
            {
                
                Process process;
                try
                {
                    process = Process.GetProcessById(id);
                    
                }
                catch (ArgumentException e)
                {
                    // it's Ok. In watch mode there can be processes that are ended already but they have to stay in the list 'till you exclude them pushing remove
                    // button. But they are not running so the above code raises Exception. 
                    // This is like watching var in Debugger when the var is disposed;
                    continue;
                }
                System.Windows.Application.Current.Dispatcher.Invoke(delegate
                {
                    _processDictionary[id] = process;// = newProcessRecord;
                });
                    
            }
        }

        public void AddProcessToWatch(int id)
        {
            var process = Process.GetProcessById(id);
            
            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExited;
            
            AddProcessToWatch(process);
        }

        private void OnProcessExited(object p, EventArgs args) 
        {
            // this govnocod is made to recieve a process object that has ExitTime & ExitCode set... I didn't find another solution
            var pro = p as Process;
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                _processDictionary[pro.Id] = pro;
            });
        }

        public void AddProcessToWatch(Process process)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                _processDictionary.Add(process.Id, process);// = newProcessRecord;
            });
        }

        public void RemoveWatchedProcess(int id)
        {
            _processDictionary[id].Exited -= OnProcessExited;
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                _processDictionary.Remove(id);
            });
        }


        public ProcessWatchesViewModel()
        {
            _processDictionary = new ObservableDictionary<int, Process>();
            Update();
        }
    }
}
