using NotepadApp.Models;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using NotepadApp.Commands;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Win32;
using NotepadApp.Views;
using System.Windows;
using NotepadApp.Events;

namespace NotepadApp.ViewModels
{
    public class NotepadViewModel : INotifyPropertyChanged
    {
        private NotepadModel _model;
        private bool _suppressLineUpdate = false;
        private string _text = string.Empty;
        private bool _isStatusBarVisible = true;
        private double _fontSize = 14;
        private string _zoomLevel = "100%";
        private string _cursorPosition = string.Empty;
        private string _findText = string.Empty;
        private int _lastFindIndex = 0;
        private ObservableCollection<string> _lineNumbers = new ObservableCollection<string>();

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ToggleStatusBarCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand DefaultZoomCommand { get; }
        public ICommand OpenFindCommand => new RelayCommand(_ =>
        {
            var findWindow = new FindWindow();
            findWindow.Show();
        });

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public NotepadViewModel()
        {
            _model = new NotepadModel();
            NewCommand = new RelayCommand(NewFile);
            OpenCommand = new RelayCommand(OpenFile);
            SaveCommand = new RelayCommand(SaveFile);
            SaveAsCommand = new RelayCommand(SaveAs);
            ToggleStatusBarCommand = new RelayCommand(ToggleStatusBar);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            DefaultZoomCommand = new RelayCommand(DefaultZoom);
        }

        ~NotepadViewModel()
        {
            
        }

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                    if (!_suppressLineUpdate)
                        UpdateLineNumbers();
                }
            }
        }

        public ObservableCollection<string> LineNumbers
        {
            get => _lineNumbers;
            set { _lineNumbers = value; OnPropertyChanged(); }
        }

        public bool IsStatusBarVisible
        {
            get => _isStatusBarVisible;
            set { _isStatusBarVisible = value; OnPropertyChanged(); }
        }

        private void ToggleStatusBar() => IsStatusBarVisible = !IsStatusBarVisible;

        public string ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(); }
        }

        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    ZoomLevel = $"{_fontSize * 100.0 / 14:0}%";
                    OnPropertyChanged();
                }
            }
        }

        public string CursorPosition
        {
            get => _cursorPosition;
            set
            {
                if (_cursorPosition != value)
                {
                    _cursorPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        private void UpdateLineNumbers()
        {
            var lines = _text?.Split('\n').Length ?? 1;

            // Reuse existing collection to avoid UI redraw
            if (LineNumbers.Count > lines)
            {
                while (LineNumbers.Count > lines)
                    LineNumbers.RemoveAt(LineNumbers.Count - 1);
            }
            else
            {
                for (int i = LineNumbers.Count + 1; i <= lines; i++)
                    LineNumbers.Add(i.ToString());
            }
        }

        public int SelectionStart
        {
            get => _selectionStart;
            set
            {
                _selectionStart = value;
                OnPropertyChanged();
            }
        }

        public int SelectionLength
        {
            get => _selectionLength;
            set
            {
                _selectionLength = value;
                OnPropertyChanged();
            }
        }

        // This method is used to load the text from a MemoryStream
        public void LoadTextFromStream(MemoryStream memoryStream)
        {
            _suppressLineUpdate = true;

            // Reset memory stream position to 0
            memoryStream.Position = 0;

            using (StreamReader reader = new StreamReader(memoryStream))
            {
                var builder = new StringBuilder();

                // Read line by line to avoid high memory usage
                while (!reader.EndOfStream)
                {
                    builder.AppendLine(reader.ReadLine());
                }

                // Store the text (builder contains all lines now)
                Text = builder.ToString();
            }

            _suppressLineUpdate = false;
            UpdateLineNumbers();
        }

        private void NewFile()
        {
            Text = string.Empty;
            ZoomLevel = "100%";
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                _suppressLineUpdate = true;

                using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var builder = new StringBuilder();

                        while (!reader.EndOfStream)
                        {
                            builder.AppendLine(reader.ReadLine());
                        }

                        Text = builder.ToString();
                    }
                }

                _suppressLineUpdate = false;
                UpdateLineNumbers();
            }
        }

        private void SaveFile()
        {
            using (var memoryStream = new MemoryStream())
            {
                _model.SaveTextToStream(memoryStream);
                // Use memoryStream to save the data to a file or other storage
            }
        }

        private void SaveAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, Text ?? string.Empty);
            }
        }

        private void ZoomIn() => FontSize += 2;

        private void ZoomOut() => FontSize = FontSize > 6 ? FontSize - 2 : FontSize;

        private void DefaultZoom() => FontSize = 14;

        public void UpdateCursorPosition(int caretIndex, string text)
        {
            int line = text.Take(caretIndex).Count(c => c == '\n') + 1;
            int lastLineBreak = text.LastIndexOf('\n', caretIndex - 1 >= 0 ? caretIndex - 1 : 0);
            int col = caretIndex - (lastLineBreak + 1);
            CursorPosition = $"Ln {line}, Col {col + 1}";
        }

        public void AdjustFontSizeFromMouseWheel(int delta)
        {
            if (delta > 0)
                ZoomIn();
            else
                ZoomOut();
        }

    }
}
