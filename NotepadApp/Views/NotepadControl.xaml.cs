using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NotepadApp.Commands;
using NotepadApp.Events;
using NotepadApp.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace NotepadApp.Controls
{
    public partial class NotepadControl : UserControl
    {
        private string _filePath = string.Empty;
        public string FilePath { get => _filePath; set => _filePath = value; }
        public event EventHandler<string> SaveRequested;
        public RelayCommand SaveCommand { get; private set; }
        public NotepadViewModel ViewModel { get; set; }
        public NotepadControl()
        {
            InitializeComponent();
            ViewModel = new NotepadViewModel();
            DataContext = ViewModel;
        }

        // Optional: Method to set text from an external memory stream
        public void SetTextFromStream(System.IO.MemoryStream memoryStream)
        {
            var viewModel = (NotepadViewModel)this.DataContext;
            viewModel.LoadTextFromStream(memoryStream);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveRequested?.Invoke(this, Editor.Text);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem != null)
            {
                switch (menuItem.Header.ToString())
                {
                    case "_New":
                        NewFile();
                        break;

                    case "_Open...":
                        OpenFile();
                        break;

                    case "_Save":
                        SaveFile();
                        break;

                    case "Save _As...":
                        SaveAsFile();
                        break;

                    case "Page Set_up...":
                        PageSetup();
                        break;

                    case "_Print...":
                        PrintFile();
                        break;

                    case "E_xit":
                        ExitApplication();
                        break;

                    case "_Undo":
                        UndoAction();
                        break;

                    case "_Cut":
                        CutAction();
                        break;

                    case "_Copy":
                        CopyAction();
                        break;

                    case "_Paste":
                        PasteAction();
                        break;

                    case "_Delete":
                        DeleteAction();
                        break;

                    case "Find...":
                        FindText();
                        break;

                    case "Find Next":
                        FindNextText();
                        break;

                    case "Replace...":
                        ReplaceText();
                        break;

                    case "Go To...":
                        GoToLine();
                        break;

                    case "Select _All":
                        SelectAllText();
                        break;

                    case "Time/Date":
                        InsertTimeDate();
                        break;

                    case "_Word Wrap":
                        ToggleWordWrap();
                        break;

                    case "_Font...":
                        ChangeFont();
                        break;

                    case "_Status Bar":
                        ToggleStatusBar();
                        break;

                    case "Zoom In":
                        ZoomIn();
                        break;

                    case "Zoom Out":
                        ZoomOut();
                        break;

                    case "Restore Default Zoom":
                        RestoreDefaultZoom();
                        break;

                    case "_View Help":
                        ViewHelp();
                        break;

                    case "_About Notepad":
                        AboutApp();
                        break;
                }
            }
        }

        private void StatusBarMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Toggle is handled by the MenuItem binding, so this might be optional
        }

        private void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (DataContext is NotepadViewModel vm)
                {
                    if (e.Delta > 0)
                        vm.ZoomInCommand.Execute(null);
                    else
                        vm.ZoomOutCommand.Execute(null);

                    e.Handled = true;
                }
            }
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SharedScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        private void TextScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            
        }

        private void Editor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ViewModel.AdjustFontSizeFromMouseWheel(e.Delta);
                e.Handled = true;
            }
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
                ViewModel.UpdateCursorPosition(tb.CaretIndex, tb.Text);
        }






        // Define methods here:

        private void NewFile() { /* New File logic */ }
        private void OpenFile() { /* Open File logic */ }
        private void SaveFile() { /* Save File logic */ }
        private void SaveAsFile() { /* Save As logic */ }
        private void PageSetup() { /* Page Setup logic */ }
        private void PrintFile() { /* Print logic */ }
        private void ExitApplication() { /* Exit logic */ }
        private void UndoAction() { /* Undo logic */ }
        private void CutAction() { /* Cut logic */ }
        private void CopyAction() { /* Copy logic */ }
        private void PasteAction() { /* Paste logic */ }
        private void DeleteAction() { /* Delete logic */ }
        private void FindText() { /* Find logic */ }
        private void FindNextText() { /* Find Next logic */ }
        private void ReplaceText() { /* Replace logic */ }
        private void GoToLine() { /* Go To logic */ }
        private void SelectAllText() { /* Select All logic */ }
        private void InsertTimeDate() { /* Insert Time/Date logic */ }
        private void ToggleWordWrap() { /* Word Wrap logic */ }
        private void ChangeFont() { /* Font change logic */ }
        private void ToggleStatusBar() { /* Status Bar toggle logic */ }
        private void ZoomIn() { /* Zoom In logic */ }
        private void ZoomOut() { /* Zoom Out logic */ }
        private void RestoreDefaultZoom() { /* Default Zoom logic */ }
        private void ViewHelp() { /* Help view logic */ }
        private void AboutApp() { /* About logic */ }
    }
}
