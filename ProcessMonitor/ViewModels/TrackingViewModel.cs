using System.Collections.ObjectModel;
using ProcessMonitor.Models;

namespace ProcessMonitor.ViewModels
{
    public class TrackingViewModel : ViewModelBase
    {
        public ObservableCollection<TrackedTask> TrackedTasks { get; } = new ObservableCollection<TrackedTask>();

        public void AddTask(string name, int id)
        {
            var task = new TrackedTask(name, id, (t) =>
            {
                // TrackedTasks.Remove(t); 
            });
            TrackedTasks.Add(task);
        }
    }
}