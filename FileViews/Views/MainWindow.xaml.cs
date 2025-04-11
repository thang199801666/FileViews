using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileViews.ViewModels;

namespace FileViews.Views
{
    public partial class MainWindow : Window
    {
        private readonly SftpFileListViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new SftpFileListViewModel(
                host: "localhost",
                port: 2223,
                username: "myuser",
                password: "mypassword"
            );
            DataContext = _viewModel;

            // Gán giá trị mặc định cho PasswordBox
            passwordBox.Password = _viewModel.Password;
            passwordBox.PasswordChanged += (s, e) => _viewModel.Password = passwordBox.Password;
        }

    }
}