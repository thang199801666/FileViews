using FileViews.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FileViews.Views
{
    public partial class MainWindow : Window
    {
        private readonly SftpFileListViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new SftpFileListViewModel("ohpcvn", 22, "thannguyen", "noequalvn");
            DataContext = _viewModel;

            // Cập nhật Password từ PasswordBox khi người dùng nhập
            passwordBox.PasswordChanged += (s, e) => _viewModel.Password = passwordBox.Password;
        }

        // Xử lý sự kiện MouseDoubleClick (nếu cần)
        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewModel.OpenOrDownloadCommand.CanExecute(null))
            {
                _viewModel.OpenOrDownloadCommand.Execute(((ListView)sender).SelectedItem);
            }
        }
    }
}