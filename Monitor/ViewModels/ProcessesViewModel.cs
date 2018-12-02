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
using System.Runtime.InteropServices;

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

        protected long _memoryLoad = 0;
        public long MemoryLoad
        {
            get => _memoryLoad;
        }

       
        protected string _processNameTemplate = "";
        public string ProcessNameTemplate
        {
            get => _processNameTemplate;
            set
            {
                _processNameTemplate = value.Trim().ToLowerInvariant();
            }
        }

        protected ObservableSortedDictionary<int, Process> _processDictionary;
        protected Dictionary<int, bool> _processUpdateDiff;
        protected bool isAliveFlag = true;
       
        public ObservableSortedDictionary<int, Process> ProcessDictionary
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

        protected static void EndProcessCommandHandler(object arg)
        {
            var process = arg as Process;
            
            process?.Kill();
        }

       

        public event PropertyChangedEventHandler PropertyChanged;

        public ProcessesViewModel()
        {
            Update();
        }

        
        public void Update()
        {
            var update = Process.GetProcesses();
            
            UpdateMemoryLoad();

            if (_processDictionary is null)
            {
                _processDictionary = new ObservableSortedDictionary<int, Process>(new ProcessDictEntryComparer());
                _processUpdateDiff = new Dictionary<int, bool>();
                
                foreach (var p in update)
                {
                    if (ProcessNameTemplateCheck(p.ProcessName))
                    {
                        _processDictionary.Add(p.Id, p);
                        _processUpdateDiff.Add(p.Id, true);
                    }
                }
                return;
            }

            isAliveFlag = !isAliveFlag;
            foreach (var newProcessRecord in update)
            {
                if (ProcessNameTemplateCheck(newProcessRecord.ProcessName))
                {
                    if (_processDictionary.ContainsKey(newProcessRecord.Id))
                    {
                        var oldProcessRecord = _processDictionary[newProcessRecord.Id];
                        _processUpdateDiff[newProcessRecord.Id] = isAliveFlag;
                        if (!CompareProcesses(oldProcessRecord, newProcessRecord))
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(delegate
                            {
                                _processDictionary[newProcessRecord.Id] = newProcessRecord;// = newProcessRecord;
                            });
                        }
                    }
                    else
                    {
                        _processUpdateDiff[newProcessRecord.Id] = isAliveFlag;
                        System.Windows.Application.Current.Dispatcher.Invoke(delegate
                        {
                            try
                            {
                                _processDictionary.Add(newProcessRecord.Id, newProcessRecord);
                            }
                            catch (ArgumentException e)
                            {
                                var d = e.Data;
                            }
                        });
                    }
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

            OnPropertyChanged("Count");
        }

        public void UpdateWatches()
        {

        }

        protected bool ProcessNameTemplateCheck(string processName)
        {
            if (_processNameTemplate.Length == 0)
                return true;
            return processName.ToLowerInvariant().StartsWith(_processNameTemplate);
        }

        protected void UpdateMemoryLoad()
        {
            decimal phav = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            decimal tot = PerformanceInfo.GetTotalMemoryInMiB();
            decimal percentFree = ((decimal)phav / (decimal)tot) * 100;
            _memoryLoad = (int)(100m - percentFree);
            OnPropertyChanged("MemoryLoad");
        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
