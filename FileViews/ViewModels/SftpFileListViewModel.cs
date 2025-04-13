using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using FileViews.Models;
using FileViews.Services;

namespace FileViews.ViewModels
{
    public class SftpFileListViewModel : FileListViewModelBase
    {
        private string _host;
        private int _port;
        private string _username;
        private string _password;
        private readonly SftpFileService _fileService;
        private string _sortColumn;
        private bool _sortAscending;

        public string Host
        {
            get => _host;
            set { _host = value; OnPropertyChanged(); }
        }

        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand SortCommand { get; }

        public SftpFileListViewModel(string host, int port, string username, string password)
            : base(new SftpFileService(host, port, username, password))
        {
            _fileService = (SftpFileService)FileService;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            CurrentPath = "/";
            SortCommand = new RelayCommand(ExecuteSort, CanExecuteSort);
            _sortColumn = "Name";
            _sortAscending = true;
            // Không gọi ApplyDefaultSort ở đây vì Files chưa có dữ liệu
        }

        protected override void ExecuteConnect(object parameter)
        {
            try
            {
                StatusMessage = "Connecting...";
                FileService.Connect();
                CurrentPath = _fileService.DefaultPath ?? "/";
                LoadFiles(); // Tách logic tải dữ liệu
                StatusMessage = "Connected successfully.";
                IsConnected = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection failed: {ex.Message}";
                IsConnected = false;
            }
        }

        protected override bool CanExecuteConnect(object parameter)
        {
            return !string.IsNullOrEmpty(Host) &&
                   !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) &&
                   !IsConnected;
        }

        protected override void ExecuteRefresh(object parameter)
        {
            LoadFiles(); // Gọi lại để làm mới
        }

        private void LoadFiles()
        {
            try
            {
                StatusMessage = "Loading files...";
                Files.Clear();
                var files = FileService.ListFiles(CurrentPath);
                foreach (var file in files)
                {
                    Files.Add(file);
                }
                ApplyDefaultSort(); // Áp dụng sắp xếp sau khi có dữ liệu
                StatusMessage = "Files loaded successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Load failed: {ex.Message}";
            }
        }

        private void ExecuteSort(object parameter)
        {
            if (parameter is string columnName && !string.IsNullOrEmpty(columnName))
            {
                if (_sortColumn == columnName)
                {
                    _sortAscending = !_sortAscending;
                }
                else
                {
                    _sortColumn = columnName;
                    _sortAscending = true;
                }

                var view = CollectionViewSource.GetDefaultView(Files);
                if (view is ListCollectionView listView)
                {
                    if (_sortColumn == "Name")
                    {
                        listView.CustomSort = new CustomFileItemComparer(_sortAscending);
                    }
                    else
                    {
                        listView.SortDescriptions.Clear();
                        listView.SortDescriptions.Add(new SortDescription(_sortColumn, _sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                        listView.CustomSort = null;
                    }
                    view.Refresh(); // Đảm bảo giao diện cập nhật
                }
            }
        }

        private bool CanExecuteSort(object parameter)
        {
            return IsConnected && Files.Any();
        }

        private void ApplyDefaultSort()
        {
            var view = CollectionViewSource.GetDefaultView(Files);
            if (view is ListCollectionView listView && Files.Any())
            {
                listView.CustomSort = new CustomFileItemComparer(_sortAscending);
                view.Refresh();
            }
        }
    }

    public class CustomFileItemComparer : IComparer
    {
        private readonly bool _ascending;

        public CustomFileItemComparer(bool ascending)
        {
            _ascending = ascending;
        }

        public int Compare(object x, object y)
        {
            if (x is FileItem item1 && y is FileItem item2)
            {
                if (item1.Name == ".." && item2.Name != "..")
                    return _ascending ? -1 : 1;
                if (item2.Name == ".." && item1.Name != "..")
                    return _ascending ? 1 : -1;

                if (item1.IsDirectory && !item2.IsDirectory)
                    return _ascending ? -1 : 1;
                if (!item1.IsDirectory && item2.IsDirectory)
                    return _ascending ? 1 : -1;

                int nameComparison = string.Compare(item1.Name, item2.Name, StringComparison.OrdinalIgnoreCase);
                return _ascending ? nameComparison : -nameComparison;
            }

            return 0;
        }
    }
}