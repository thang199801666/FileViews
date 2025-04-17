using System;
using System.Windows.Input;

namespace NotepadApp.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Constructor that takes an Action for methods that do not require parameters
        public RelayCommand(Action execute)
            : this(param => execute(), null)
        {
        }

        // Constructor that takes an Action<object> for methods with parameters
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        // Constructor that accepts both execute and canExecute logic
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

}
