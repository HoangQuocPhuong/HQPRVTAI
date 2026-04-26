using System.Windows.Input;

namespace HQPRVTAI.Infrastructure;

public class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T?,Task> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (_isExecuting)
            return false;

        if (parameter is T typedParameter)
        {
            return _canExecute?.Invoke(typedParameter) ?? true;
        }

        return _canExecute?.Invoke(default) ?? true;
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            if (parameter is T typedParameter)
            {
                await _execute(typedParameter);
            }
            else
            {
                await _execute(default);
            }
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }


    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}