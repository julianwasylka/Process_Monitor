using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using ProcessManager.Models;

namespace ProcessManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _filterText;
        private ProcessItem _selectedProcess;
        private ICollectionView _processesView;

        public ObservableCollection<ProcessItem> Processes { get; } = new ObservableCollection<ProcessItem>();

        public ICollectionView ProcessesView => _processesView;

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

        public ProcessItem SelectedProcess
        {
            get => _selectedProcess;
            set => SetProperty(ref _selectedProcess, value);
        }

        public RelayCommand RefreshCommand { get; }

        public MainViewModel()
        {
            RefreshCommand = new RelayCommand(o => LoadProcesses());

            _processesView = CollectionViewSource.GetDefaultView(Processes);
            _processesView.Filter = FilterProcesses;

            LoadProcesses();
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
    }
}