using NotepadApp.Events;
using System.Windows;

namespace NotepadApp.Views
{
    public partial class FindWindow : Window
    {
        public FindWindow()
        {
            InitializeComponent();
            SearchTextBox.Focus();
        }

        private void FindNextButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

}
