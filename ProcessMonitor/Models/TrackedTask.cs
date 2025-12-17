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

        private readonly string _activeStatus = "Śledzenie aktywne";
        private readonly string _cancelledStatus = "Zatrzymano";
        private readonly string _stoppedStatus = "Zakończono";
        private readonly string _noAccessStatus = "Proces niedostępny";

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
            Status = _activeStatus;

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
                Status = _cancelledStatus;
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

                    var process = System.Diagnostics.Process.GetProcessById(_processId);
                    if (process.HasExited)
                    {
                        break;
                    }

                    double memMB = process.WorkingSet64 / 1024.0 / 1024.0;

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        MemoryHistory.Add($"[{DateTime.Now:HH:mm:ss}] {memMB:F2} MB");
                        if (MemoryHistory.Count > 10) MemoryHistory.RemoveAt(0);
                    });

                    await Task.Delay(2000, token);
                }
            }
            catch (TaskCanceledException)
            {
                Status = _cancelledStatus;
            }
            catch (Exception)
            {
                Status = _noAccessStatus;
            }
            finally
            {
                if (Status == _activeStatus)
                {
                    Status = _stoppedStatus;
                }
            }
        }
    }
}