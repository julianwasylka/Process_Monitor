using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ProcessMonitor.ViewModels;

namespace ProcessMonitor.Models
{
    public class TrackedTask : ViewModelBase
    {
        private CancellationTokenSource _cts;
        private readonly int _processId;

        public string ProcessName { get; }
        public DateTime StartTrackingTime { get; }
        public ObservableCollection<string> MemoryHistory { get; } = new ObservableCollection<string>();

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _elapsedTime;
        public string ElapsedTime
        {
            get => _elapsedTime;
            set => SetProperty(ref _elapsedTime, value);
        }

        public RelayCommand StopTrackingCommand { get; }

        public TrackedTask(string processName, int processId, Action<TrackedTask> onStopped)
        {
            ProcessName = processName;
            _processId = processId;
            StartTrackingTime = DateTime.Now;
            Status = "Śledzenie aktywne";

            _cts = new CancellationTokenSource();
            StopTrackingCommand = new RelayCommand(o =>
            {
                StopTracking();
                onStopped?.Invoke(this);
            });

            Task.Run(() => MonitorProcess(_cts.Token));
        }

        public void StopTracking()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                Status = "Zatrzymano";
            }
        }

        private async Task MonitorProcess(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var diff = DateTime.Now - StartTrackingTime;
                    ElapsedTime = $"{diff.Hours:00}:{diff.Minutes:00}:{diff.Seconds:00}";

                    try
                    {
                        var process = System.Diagnostics.Process.GetProcessById(_processId);
                        if (process.HasExited)
                        {
                            Status = "Proces zakończony";
                            break;
                        }

                        double memMB = process.WorkingSet64 / 1024.0 / 1024.0;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            MemoryHistory.Add($"[{DateTime.Now:HH:mm:ss}] {memMB:F2} MB");
                            if (MemoryHistory.Count > 10) MemoryHistory.RemoveAt(0);
                        });
                    }
                    catch
                    {
                        Status = "Proces niedostępny";
                        break;
                    }

                    await Task.Delay(2000, token);
                }
            }
            catch (TaskCanceledException)
            {
                Status = "Anulowano zadanie";
            }
        }
    }
}