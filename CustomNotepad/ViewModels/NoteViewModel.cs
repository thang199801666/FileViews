using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public class NoteViewModel : INotifyPropertyChanged
{
    private string _content;

    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged();
        }
    }

    public ICommand ClearCommand { get; }

    public NoteViewModel()
    {
        ClearCommand = new RelayCommand(Clear);
    }

    private void Clear()
    {
        Content = string.Empty;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
