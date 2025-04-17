using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FileViews.Models;
using FileViews.Services;
using NotepadApp.Controls;
using Renci.SshNet;

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
        private bool _showHiddenFiles;
        private FileItem _selectedItem;

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

        public bool ShowHiddenFiles
        {
            get => _showHiddenFiles;
            set
            {
                if (_showHiddenFiles != value)
                {
                    _showHiddenFiles = value;
                    OnPropertyChanged();
                    if (IsConnected)
                    {
                        LoadFiles(); // Refresh list when toggled
                    }
                }
            }
        }

        public FileItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Update command states
            }
        }

        public ICommand SortCommand { get; }
        public ICommand ToggleHiddenFilesCommand { get; }
        public ICommand NavigateCommand { get; }

        public bool IsConnected { get; private set; }

        public SftpFileListViewModel(string host, int port, string username, string password)
            : base(new SftpFileService(host, port, username, password))
        {
            _fileService = (SftpFileService)FileService;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            CurrentPath = "/home/";
            SortCommand = new RelayCommand(ExecuteSort, CanExecuteSort);
            ToggleHiddenFilesCommand = new RelayCommand(ExecuteToggleHiddenFiles, CanExecuteToggleHiddenFiles);
            NavigateCommand = new RelayCommand(ExecuteNavigate, CanExecuteNavigate);
            _sortColumn = "Name";
            _sortAscending = true;
            _showHiddenFiles = false;
        }

        protected override void ExecuteConnect(object parameter)
        {
            try
            {
                StatusMessage = "Connecting...";
                FileService.Connect();
                CurrentPath = _fileService.DefaultPath ?? "/home/";
                LoadFiles();
                StatusMessage = "Connected successfully.";
                IsConnected = true;
                OnPropertyChanged(nameof(IsConnected));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection failed: {ex.Message}";
                IsConnected = false;
                OnPropertyChanged(nameof(IsConnected));
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
            LoadFiles();
        }

        private void ExecuteNavigate(object parameter)
        {
            if (parameter is not FileItem fileItem)
                return;

            if (fileItem.IsDirectory)
            {
                string newPath = NavigateToDirectory(fileItem);
                if (!string.IsNullOrEmpty(newPath))
                {
                    StatusMessage = $"Navigating to {newPath}...";
                    CurrentPath = newPath;
                    OnPropertyChanged(nameof(CurrentPath));
                    LoadFiles();
                }
            }
            else
            {
                if (fileItem.Size < 10 * 1024 * 1024) // < 10MB
                {
                    OpenFileInNotepadControl(fileItem.FullPath);
                }
                else
                {
                    StatusMessage = $"File size exceeds 10MB, cannot open in Notepad.";
                }
            }
        }

        /// <summary>
        /// Navigate to the selected directory.
        /// </summary>
        private string NavigateToDirectory(FileItem fileItem)
        {
            string newPath;

            if (fileItem.Name == "..")
            {
                // Navigate to parent directory
                string trimmedPath = CurrentPath.TrimEnd('/');
                int lastSlashIndex = trimmedPath.LastIndexOf('/');

                if (lastSlashIndex > 0)
                    newPath = trimmedPath.Substring(0, lastSlashIndex);
                else
                    newPath = "/"; // Already at root

                if (!newPath.EndsWith("/"))
                    newPath += "/";
            }
            else
            {
                // Navigate to child directory
                if (CurrentPath.EndsWith("/"))
                    newPath = $"{CurrentPath}{fileItem.Name}/";
                else
                    newPath = $"{CurrentPath}/{fileItem.Name}/";
            }

            return newPath;
        }

        /// <summary>
        /// Opens the file content in the custom NotepadControl.
        /// </summary>
        private void OpenFileInNotepadControl(string filePath)
        {
            try
            {
                var fileContent = FileService.ReadFileAsMemoryStream(filePath); //ReadFileAsText(filePath);

                var notepadControl = new NotepadControl
                {
                    FilePath = filePath
                };

                notepadControl.SetTextFromStream(fileContent);

                var window = new Window
                {
                    Title = $"Notepad - {Path.GetFileName(filePath)}",
                    Content = notepadControl,
                    Width = 800,
                    Height = 600
                };

                // Handle SaveRequested event
                notepadControl.SaveRequested += (s, newText) =>
                {
                    FileService.WriteFileAsText(filePath, newText);
                    StatusMessage = $"Saved: {filePath}";
                };

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to open file: {ex.Message}";
            }
        }


        private bool CanExecuteNavigate(object parameter)
        {
            return IsConnected && parameter is FileItem;
        }

        protected override void ExecuteOpenOrDownload(object parameter)
        {
            var fileItem = parameter as FileItem;
            if (fileItem == null)
                return;

            if (fileItem.IsDirectory)
            {
                ExecuteNavigate(fileItem);
            }
            else
            {
                
            }
        }

        private void LoadFiles()
        {
            try
            {
                Files.Clear();
                var files = FileService.ListFiles(CurrentPath);

                foreach (var file in files)
                {
                    // Load files always include ..directory
                    if (file.Name == "..") Files.Add(file);
                    if (file.Name.StartsWith("."))
                    {
                        if (_showHiddenFiles)
                        {
                            Files.Add(file);
                        }
                    }
                    else
                    {
                        Files.Add(file);
                    }
                }
                SortFiles(_sortColumn, _sortAscending);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Load failed: {ex.Message}";
            }
        }

        //public string ReadFileAsText(string remotePath, Encoding encoding = null)
        //{
        //    return FileService.ReadFileAsText(remotePath, encoding);
        //}

        private void ExecuteToggleHiddenFiles(object parameter)
        {
            ShowHiddenFiles = !ShowHiddenFiles;
        }

        private bool CanExecuteToggleHiddenFiles(object parameter)
        {
            return IsConnected;
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

                StatusMessage = $"Sorting by {_sortColumn} ({(_sortAscending ? "ascending" : "descending")})";
                SortFiles(_sortColumn, _sortAscending);
            }
        }

        private bool CanExecuteSort(object parameter)
        {
            return IsConnected && Files.Any();
        }

        private void SortFiles(string columnName, bool ascending)
        {
            var sortedList = new List<FileItem>(Files);

            switch (columnName)
            {
                case "Name":
                    sortedList = SortByName(sortedList, ascending);
                    break;
                case "Size":
                    sortedList = SortBySize(sortedList, ascending);
                    break;
                case "LastModified":
                    sortedList = SortByLastModified(sortedList, ascending);
                    break;
                case "Permissions":
                    sortedList = SortByPermissions(sortedList, ascending);
                    break;
                default:
                    sortedList = SortByName(sortedList, ascending);
                    break;
            }

            Files.Clear();
            foreach (var item in sortedList)
            {
                Files.Add(item);
            }
        }

        private List<FileItem> SortByName(List<FileItem> items, bool ascending)
        {
            var parentDir = items.FirstOrDefault(i => i.Name == "..");
            var directories = items.Where(i => i.IsDirectory && i.Name != "..").ToList();
            var files = items.Where(i => !i.IsDirectory).ToList();

            if (ascending)
            {
                directories.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                files.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                directories.Sort((a, b) => string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase));
                files.Sort((a, b) => string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase));
            }

            var result = new List<FileItem>();
            if (parentDir != null) result.Add(parentDir);
            result.AddRange(directories);
            result.AddRange(files);

            return result;
        }

        private List<FileItem> SortBySize(List<FileItem> items, bool ascending)
        {
            var parentDir = items.FirstOrDefault(i => i.Name == "..");
            var directories = items.Where(i => i.IsDirectory && i.Name != "..").ToList();
            var files = items.Where(i => !i.IsDirectory).ToList();

            if (ascending)
            {
                directories.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                files.Sort((a, b) => a.Size.CompareTo(b.Size));
            }
            else
            {
                directories.Sort((a, b) => string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase));
                files.Sort((a, b) => b.Size.CompareTo(a.Size));
            }

            var result = new List<FileItem>();
            if (parentDir != null) result.Add(parentDir);
            result.AddRange(directories);
            result.AddRange(files);

            return result;
        }

        private List<FileItem> SortByLastModified(List<FileItem> items, bool ascending)
        {
            var parentDir = items.FirstOrDefault(i => i.Name == "..");
            var directories = items.Where(i => i.IsDirectory && i.Name != "..").ToList();
            var files = items.Where(i => !i.IsDirectory).ToList();

            if (ascending)
            {
                directories.Sort((a, b) => a.LastModified.CompareTo(b.LastModified));
                files.Sort((a, b) => a.LastModified.CompareTo(b.LastModified));
            }
            else
            {
                directories.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
                files.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
            }

            var result = new List<FileItem>();
            if (parentDir != null) result.Add(parentDir);
            result.AddRange(directories);
            result.AddRange(files);

            return result;
        }

        private List<FileItem> SortByPermissions(List<FileItem> items, bool ascending)
        {
            var parentDir = items.FirstOrDefault(i => i.Name == "..");
            var directories = items.Where(i => i.IsDirectory && i.Name != "..").ToList();
            var files = items.Where(i => !i.IsDirectory).ToList();

            if (ascending)
            {
                directories.Sort((a, b) => string.Compare(a.Permissions, b.Permissions, StringComparison.Ordinal));
                files.Sort((a, b) => string.Compare(a.Permissions, b.Permissions, StringComparison.Ordinal));
            }
            else
            {
                directories.Sort((a, b) => string.Compare(b.Permissions, a.Permissions, StringComparison.Ordinal));
                files.Sort((a, b) => string.Compare(b.Permissions, a.Permissions, StringComparison.Ordinal));
            }

            var result = new List<FileItem>();
            if (parentDir != null) result.Add(parentDir);
            result.AddRange(directories);
            result.AddRange(files);

            return result;
        }
    }
}