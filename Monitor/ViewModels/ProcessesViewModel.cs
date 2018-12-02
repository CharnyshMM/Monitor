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
using DrWPF.Windows.Data;

namespace Monitor.ViewModels
{
    class ProcessesViewModel : INotifyPropertyChanged
    {
        class ProcessDictEntryComparer : IComparer<DictionaryEntry>
        {
            public int Compare(DictionaryEntry x, DictionaryEntry y)
            {
                var p1 = x.Value as Process;
                var p2 = y.Value as Process;
                if (p1 is null || p2 is null)
                    return 0;

                int comparisonResult = p1.ProcessName.CompareTo(p2.ProcessName);
                if (comparisonResult != 0)
                    return comparisonResult;
                comparisonResult = p1.Id.CompareTo(p2.Id); 
                if (comparisonResult != 0)
                    return comparisonResult;
                comparisonResult = p1.PrivateMemorySize64.CompareTo(p2.PrivateMemorySize64);
                return comparisonResult;
                
            }
        }

        public static bool CompareProcesses(Process x, Process y)
        {
            if (x.Id != y.Id)
                return false;
            if (x.ProcessName != y.ProcessName)
                return false;

            if (x.BasePriority != y.BasePriority)
                return false;
            if (x.PrivateMemorySize64 != y.PrivateMemorySize64)
                return false;
            if (x.Responding != y.Responding)
                return false;
            return true;
        }

        public int Count
        {
            get => _processDictionary.Count();
            
        }

        protected ObservableSortedDictionary<int, Process> _processDictionary;
        protected Dictionary<int, bool> _processUpdateDiff;
        protected bool isAliveFlag = true;
       
        public ObservableSortedDictionary<int, Process> ProcessDictionary { get => _processDictionary; }

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
            var update = Process.GetProcesses();

            if (_processDictionary is null)
            {
                _processDictionary = new ObservableSortedDictionary<int, Process>(new ProcessDictEntryComparer());
                _processUpdateDiff = new Dictionary<int, bool>();
                
                foreach (var p in update)
                {
                    _processDictionary.Add(p.Id, p);
                    _processUpdateDiff.Add(p.Id, true);
                }
                return;
            }

            isAliveFlag = !isAliveFlag;
            foreach (var newProcessRecord in update)
            {
                if (_processDictionary.ContainsKey(newProcessRecord.Id))
                {
                    var oldProcessRecord = _processDictionary[newProcessRecord.Id];
                    _processUpdateDiff[oldProcessRecord.Id] = isAliveFlag;
                    if (!CompareProcesses(oldProcessRecord, newProcessRecord))
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(delegate
                        {
                            _processDictionary[newProcessRecord.Id] = newProcessRecord;
                        });
                    }
                }
                else
                {
                    _processUpdateDiff[newProcessRecord.Id] = isAliveFlag;
                    System.Windows.Application.Current.Dispatcher.Invoke(delegate
                    {
                        _processDictionary.Add(newProcessRecord.Id, newProcessRecord);
                    });
                }
            }

            var endedProcesses = _processUpdateDiff.Where(pair => pair.Value != isAliveFlag).ToList();
            foreach (KeyValuePair<int, bool> entry in endedProcesses)
            {
                if (entry.Value != isAliveFlag)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(delegate
                    {
                        _processDictionary.Remove(entry.Key);
                    });
                    _processUpdateDiff.Remove(entry.Key);
                }
            }

            

            //OnPropertyChanged("ProcessDictionary");
            OnPropertyChanged("Count");
        }

        

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
