using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Input;
using Monitor.Utilities;

namespace Monitor.ViewModels
{
    class ProcessesViewModel : INotifyPropertyChanged
    {
        class ProcessPIDComparer : IEqualityComparer<Process>
        {
            public bool Equals(Process x, Process y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Id == y.Id;
   
            }

            public int GetHashCode(Process obj)
            {
                return obj?.Id.GetHashCode() ?? 0;
            }
        }

        public static bool CompareProcesses(Process x, Process y)
        {
            if (x.Id != y.Id)
                return false;
            
            if (x.BasePriority != y.BasePriority)
                return false;
            if (x.PrivateMemorySize64 != y.PrivateMemorySize64)
                return false;
            if (x.Responding != y.Responding)
                return false;
            return true;
        }

        protected ObservableCollection<Process> _processObservableCollection;
        protected Dictionary<long, Process> _processDictionary;
        public ObservableCollection<Process> ProcessList { get => _processObservableCollection; }

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

        protected static void EndProcessCommandHandler(object arg)
        {
            var process = arg as Process;
            
            process?.Kill();
        }

        protected ExtendedCommand _endProcess;

        public ExtendedCommand EndProcess
        {
            get
            {
                if (_endProcess is null)
                   _endProcess = new ExtendedCommand(EndProcessCommandHandler, null);
                return _endProcess;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ProcessesViewModel()
        {
            Update();
        }

        
        public void Update()
        {

            //_processDictionary = new Dictionary<long, Process>();
            //var update = Process.GetProcesses();
            //foreach (var p in update)
            //{
            //    processes.Add(p.Id, p);
            //}
            var update = Process.GetProcesses();

            if (_processObservableCollection is null)
            {
                _processObservableCollection = new ObservableCollection<Process>(update);
                _processObservableCollection.OrderBy(p => p.ProcessName);
                OnPropertyChanged("ProcessList");
                return;
            }

            foreach (var newProcessRecord in update)
            {
                if (_processObservableCollection.Contains(newProcessRecord, new ProcessPIDComparer()))
                {
                    var oldProcessRecord = _processObservableCollection.Single(p => p.Id == newProcessRecord.Id);
                    int index = _processObservableCollection.IndexOf(oldProcessRecord);
                    if (!CompareProcesses(oldProcessRecord, newProcessRecord))
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            _processObservableCollection[index] = newProcessRecord;
                        });
                    }
                }
                else
                {
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        _processObservableCollection.Add(newProcessRecord);
                    });
                }

            }
            
            OnPropertyChanged("ProcessList");
        }

        

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
