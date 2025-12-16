using System;
using System.Diagnostics;
using ProcessMonitor.ViewModels; 

namespace ProcessMonitor.Models
{
    public class ProcessItem : ViewModelBase
    {
        public Process UnderlyingProcess { get; }

        public int Id { get; }
        public string Name { get; }

        private long _memorySizeBytes;
        public long MemorySizeBytes
        {
            get => _memorySizeBytes;
            private set
            {
                if (SetProperty(ref _memorySizeBytes, value))
                {
                    OnPropertyChanged(nameof(MemoryInMB));
                }
            }
        }

        private int _threadCount;
        public int ThreadCount
        {
            get => _threadCount;
            private set => SetProperty(ref _threadCount, value);
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get => _startTime;
            private set => SetProperty(ref _startTime, value);
        }

        private string _priority;
        public string Priority
        {
            get => _priority;
            private set => SetProperty(ref _priority, value);
        }

        public string MemoryInMB => $"{(MemorySizeBytes / 1024.0 / 1024.0):F2} MB";

        public ProcessItem(Process process)
        {
            UnderlyingProcess = process;
            Id = process.Id;
            Name = process.ProcessName;
            RefreshData();
        }

        public void RefreshData()
        {
            bool hasExited = false;
            try
            {
                hasExited = UnderlyingProcess.HasExited;
            }
            catch (System.ComponentModel.Win32Exception) { hasExited = false; }
            catch { hasExited = true; }

            if (hasExited) return;

            try { MemorySizeBytes = UnderlyingProcess.WorkingSet64; } catch { MemorySizeBytes = 0; }
            try { ThreadCount = UnderlyingProcess.Threads.Count; } catch { ThreadCount = 0; }
            try { StartTime = UnderlyingProcess.StartTime; } catch { StartTime = null; }
            try { Priority = UnderlyingProcess.PriorityClass.ToString(); } catch { Priority = "Access Denied"; }
        }
    }
}