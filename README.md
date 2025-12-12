# Process Monitor

A desktop application written in C# (WPF) designed to monitor and manage Windows system processes. The project demonstrates the implementation of the MVVM pattern, asynchronous programming, and multithreading without using external MVVM frameworks.

## Features

* **Process List:** View active processes with sorting, filtering, and refreshing capabilities (manual and automatic).
* **Process Details:** Master/Detail view displaying threads and modules for the selected process.
* **Management:** Change process priority and terminate (kill) processes.
* **Monitoring (Tracking):** Asynchronous real-time memory usage tracking for selected processes.
* **History:** Dedicated window showing the history of tracked tasks.

## Technologies and Concepts

* **C# / .NET**
* **WPF (Windows Presentation Foundation)**
* **MVVM (Model-View-ViewModel):** Manual implementation using `INotifyPropertyChanged` and `ICommand`.
* **Asynchronous Programming:** Utilization of `Task` and `async/await` for non-blocking UI operations.
* **UI Components:** Custom `UserControl`, DataGrid, Data Binding.
