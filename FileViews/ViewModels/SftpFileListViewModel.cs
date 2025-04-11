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

        public SftpFileListViewModel(string host, int port, string username, string password)
            : base(new SftpFileService(host, port, username, password))
        {
            _fileService = (SftpFileService)FileService;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            CurrentPath = "/";
        }

        protected override void ExecuteConnect(object parameter) // Sửa chữ ký để khớp
        {
            try
            {
                StatusMessage = "Connecting...";
                FileService.Connect();
                CurrentPath = _fileService.DefaultPath ?? "/"; // Dùng thư mục nhà
                RefreshCommand.Execute(null);
                StatusMessage = "Connected successfully.";
                IsConnected = true; // Cập nhật trạng thái kết nối
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
    }
}