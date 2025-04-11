using FileViews.Helpers;
using FileViews.Models;
using FileViews.Services;
using System.Collections.ObjectModel;

namespace FileViews.ViewModels
{
    public abstract class FileListViewModelBase : ObservableObject
    {
        protected readonly IFileService FileService;
        private string _currentPath;
        private ObservableCollection<FileItem> _files;
        private string _statusMessage;
        private bool _isConnected;

        public string CurrentPath
        {
            get => _currentPath;
            set { _currentPath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FileItem> Files
        {
            get => _files;
            set { _files = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        public RelayCommand ConnectCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand OpenOrDownloadCommand { get; }

        protected FileListViewModelBase(IFileService fileService)
        {
            FileService = fileService;
            Files = new ObservableCollection<FileItem>();
            ConnectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);
            RefreshCommand = new RelayCommand(ExecuteRefresh, CanExecuteRefresh);
            OpenOrDownloadCommand = new RelayCommand(ExecuteOpenOrDownload, CanExecuteOpenOrDownload);
        }

        protected virtual void ExecuteConnect(object parameter)
        {
            throw new NotImplementedException("ExecuteConnect must be implemented in derived class.");
        }

        protected virtual bool CanExecuteConnect(object parameter)
        {
            return true;
        }

        protected virtual void ExecuteRefresh(object parameter)
        {
            try
            {
                //StatusMessage = "Refreshing...";
                Files.Clear();
                var files = FileService.ListFiles(CurrentPath);
                foreach (var file in files)
                {
                    Files.Add(file);
                }
                StatusMessage = $"Current Folder: {CurrentPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Refresh failed: {ex.Message}";
            }
        }

        protected virtual bool CanExecuteRefresh(object parameter)
        {
            return IsConnected;
        }

        protected virtual void ExecuteOpenOrDownload(object parameter)
        {
            if (parameter is FileItem file)
            {
                try
                {
                    if (file.IsDirectory)
                    {
                        CurrentPath = file.FullPath;
                        RefreshCommand.Execute(null);
                    }
                    else
                    {
                        FileService.OpenFile(file);
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Operation failed: {ex.Message}";
                }
            }
        }

        protected virtual bool CanExecuteOpenOrDownload(object parameter)
        {
            return parameter is FileItem && IsConnected;
        }
    }
}