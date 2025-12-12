using System;
using System.Diagnostics;

namespace ProcessManager.Models
{
    public class ProcessItem
    {
        public Process UnderlyingProcess { get; }

        public int Id { get; }
        public string Name { get; }
        public long MemorySizeBytes { get; private set; }
        public int ThreadCount { get; private set; }
        public DateTime? StartTime { get; private set; }
        public string Priority { get; private set; }

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
            catch (System.ComponentModel.Win32Exception)
            {
                hasExited = false;
            }
            catch
            {
                hasExited = true;
            }

            if (hasExited) return;

            try { MemorySizeBytes = UnderlyingProcess.WorkingSet64; } catch { MemorySizeBytes = 0; }
            try { ThreadCount = UnderlyingProcess.Threads.Count; } catch { ThreadCount = 0; }
            try { StartTime = UnderlyingProcess.StartTime; } catch { StartTime = null; }
            try { Priority = UnderlyingProcess.PriorityClass.ToString(); } catch { Priority = "Access Denied"; }
        }
    }
}