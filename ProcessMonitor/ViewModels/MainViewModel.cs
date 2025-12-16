using ProcessMonitor.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;

namespace ProcessMonitor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private DispatcherTimer _timer;
        private bool _isAutoRefreshEnabled;
        private int _refreshInterval = 2;

        private string _filterText;
        private ProcessItem _selectedProcess;
        private ICollectionView _processesView;

        private TrackingViewModel _trackingVM = new TrackingViewModel();

        public ObservableCollection<ProcessItem> Processes { get; } = new ObservableCollection<ProcessItem>();

        public ICollectionView ProcessesView => _processesView;

        public ObservableCollection<string> SelectedProcessModules { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedProcessThreads { get; } = new ObservableCollection<string>();

        public RelayCommand KillCommand { get; }
        public RelayCommand BoostPriorityCommand { get; }
        public RelayCommand TrackCommand { get; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    _processesView.Refresh();
                }
            }
        }

        public bool IsAutoRefreshEnabled
        {
            get => _isAutoRefreshEnabled;
            set
            {
                if (SetProperty(ref _isAutoRefreshEnabled, value))
                {
                    if (_isAutoRefreshEnabled)
                        _timer.Start();
                    else
                        _timer.Stop();
                }
            }
        }

        public int RefreshInterval
        {
            get => _refreshInterval;
            set
            {
                if (SetProperty(ref _refreshInterval, value))
                {
                    _timer.Interval = TimeSpan.FromSeconds(_refreshInterval);
                }
            }
        }

        public ProcessItem SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                if (SetProperty(ref _selectedProcess, value))
                {
                    LoadDetails();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public RelayCommand RefreshCommand { get; }

        public MainViewModel()
        {
            RefreshCommand = new RelayCommand(o => LoadProcesses());

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(RefreshInterval);
            _timer.Tick += (s, e) => LoadProcesses();

            _processesView = CollectionViewSource.GetDefaultView(Processes);
            _processesView.Filter = FilterProcesses;
            KillCommand = new RelayCommand(o => KillProcess(), o => SelectedProcess != null);
            BoostPriorityCommand = new RelayCommand(o => SetHighPriority(), o => SelectedProcess != null);

            TrackCommand = new RelayCommand(o => StartTracking(), o => SelectedProcess != null);
            LoadProcesses();
        }


        private void StartTracking()
        {
            if (SelectedProcess == null) return;

            var win = System.Windows.Application.Current.Windows;
            bool isOpen = false;
            foreach (var w in win)
            {
                if (w is Views.TrackingWindow)
                {
                    ((System.Windows.Window)w).Activate();
                    isOpen = true;
                    break;
                }
            }

            if (!isOpen)
            {
                var trackingWindow = new Views.TrackingWindow();
                trackingWindow.DataContext = _trackingVM;
                trackingWindow.Show();
            }

            _trackingVM.AddTask(SelectedProcess.Name, SelectedProcess.Id);
        }

        private void LoadProcesses()
        {
            var currentIds = Processes.Select(p => p.Id).ToList();
            var systemProcesses = Process.GetProcesses();

            foreach (var proc in systemProcesses)
            {
                if (!currentIds.Contains(proc.Id))
                {
                    Processes.Add(new ProcessItem(proc));
                }
            }

            var activeIds = systemProcesses.Select(p => p.Id).ToList();
            var toRemove = Processes.Where(p => !activeIds.Contains(p.Id)).ToList();

            foreach (var item in toRemove)
            {
                Processes.Remove(item);
            }
        }

        private bool FilterProcesses(object obj)
        {
            if (obj is ProcessItem process)
            {
                if (string.IsNullOrWhiteSpace(FilterText))
                    return true;

                return process.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                       process.Id.ToString().Contains(FilterText);
            }
            return false;
        }

        private void LoadDetails()
        {
            SelectedProcessModules.Clear();
            SelectedProcessThreads.Clear();

            if (SelectedProcess == null || SelectedProcess.UnderlyingProcess.HasExited) return;

            try
            {
                foreach (ProcessModule module in SelectedProcess.UnderlyingProcess.Modules)
                {
                    SelectedProcessModules.Add(module.ModuleName);
                    if (SelectedProcessModules.Count >= 50) break;
                }
            }
            catch { SelectedProcessModules.Add("Brak dostępu do modułów"); }

            try
            {
                // Pobieramy ID wątków
                foreach (ProcessThread thread in SelectedProcess.UnderlyingProcess.Threads)
                {
                    SelectedProcessThreads.Add($"ID: {thread.Id} | Stan: {thread.ThreadState}");
                    if (SelectedProcessThreads.Count >= 50) break;
                }
            }
            catch { SelectedProcessThreads.Add("Brak dostępu do wątków"); }
        }

        private void KillProcess()
        {
            try
            {
                SelectedProcess?.UnderlyingProcess.Kill();
                LoadProcesses();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Nie udało się zabić procesu: " + ex.Message);
            }
        }

        private void SetHighPriority()
        {
            try
            {
                if (SelectedProcess != null)
                {
                    SelectedProcess.UnderlyingProcess.PriorityClass = ProcessPriorityClass.High;
                    SelectedProcess.RefreshData();
                    System.Windows.MessageBox.Show("Ustawiono wysoki priorytet");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Nie można zmienić priorytetu: " + ex.Message);
            }
        }
    }
}