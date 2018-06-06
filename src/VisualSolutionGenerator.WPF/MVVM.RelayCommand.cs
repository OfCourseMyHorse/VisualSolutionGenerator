using System;
using System.Windows.Input;

namespace VisualSolutionGenerator
{
    [System.Diagnostics.DebuggerDisplay("{_execute}")]
    public class RelayCommand : ICommand
    {
        // http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030

        #region lifecycle

        public RelayCommand(Action execute) : this(null, execute, null) { }

        public RelayCommand(Action execute, Action<Exception> error) : this(null, execute, error) { }

        public RelayCommand(Func<bool> canExecute, Action execute) : this(canExecute, execute, null) { }

        public RelayCommand(Func<bool> canExecute, Action execute, Action<Exception> error)
        {
            _canExecute = canExecute;
            _execute = execute;
            _onException = error;
            _UseOwnEventHandler = false;
        }

        public RelayCommand(Action execute, System.ComponentModel.INotifyPropertyChanged canExecuteSource, string canExecuteProperty)
        {
            _execute = execute;
            System.Reflection.PropertyInfo prop = canExecuteSource.GetType().GetProperty(canExecuteProperty);
            _canExecute = () => Convert.ToBoolean(prop.GetValue(canExecuteSource));
            canExecuteSource.PropertyChanged += (s, ev) =>
                {
                    if (ev.PropertyName == canExecuteProperty || string.IsNullOrEmpty(ev.PropertyName))
                    {
                        var handler = _CanExecuteChanged;
                        // This needs to be marshalled onto the dispatcher thread because it interacts with dependency properties
                        if (handler != null) System.Windows.Application.Current.Dispatcher.Invoke(() => handler(this, EventArgs.Empty));
                    }
                };
            _UseOwnEventHandler = true;
        }

        public RelayCommand(ICommand cmd, System.ComponentModel.INotifyPropertyChanged canExecuteSource, string canExecuteProperty)
        {
            _execute = () => cmd.Execute(null);
            System.Reflection.PropertyInfo prop = canExecuteSource.GetType().GetProperty(canExecuteProperty);
            _canExecute = () => cmd.CanExecute(null) && Convert.ToBoolean(prop.GetValue(canExecuteSource));
            cmd.CanExecuteChanged += (s, ev) =>
                {
                    _CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                };
            canExecuteSource.PropertyChanged += (s, ev) =>
                {
                    if (ev.PropertyName == canExecuteProperty || string.IsNullOrEmpty(ev.PropertyName))
                    {
                        _CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
            _UseOwnEventHandler = true;
        }

        #endregion

        #region data

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;
        private readonly Action<Exception> _onException;

        private readonly bool _UseOwnEventHandler;

        private event EventHandler _CanExecuteChanged;

        #endregion        

        #region ICommand Members

        [System.Diagnostics.DebuggerStepThrough]
        public bool CanExecute(object parameter) { return _canExecute == null ? true : _canExecute(); }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_UseOwnEventHandler) _CanExecuteChanged += value;
                else CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_UseOwnEventHandler) _CanExecuteChanged -= value;
                else CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            if (_onException == null) _execute();
            else try { _execute(); } catch (Exception ex) { _onException(ex); }
        }

        #endregion // ICommand Members
    }

    [System.Diagnostics.DebuggerDisplay("{_execute}")]
    public class RelayCommand<T> : ICommand
    {
        // http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030

        #region lifecycle

        public RelayCommand(Action<T> execute) : this(execute, null) { }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute"); _canExecute = canExecute;
            _UseOwnEventHandler = false;
        }

        public RelayCommand(Action<T> execute, System.ComponentModel.INotifyPropertyChanged canExecuteSource, string canExecuteProperty)
        {
            _execute = execute;
            System.Reflection.PropertyInfo prop = canExecuteSource.GetType().GetProperty(canExecuteProperty);
            _canExecute = t => Convert.ToBoolean(prop.GetValue(canExecuteSource));
            canExecuteSource.PropertyChanged += (s, ev) =>
                {
                    if (ev.PropertyName == canExecuteProperty || string.IsNullOrEmpty(ev.PropertyName))
                    {
                        var handler = _CanExecuteChanged;
                        // This needs to be marshalled onto the dispatcher thread because it interacts with dependency properties
                        if (handler != null) System.Windows.Application.Current.Dispatcher.Invoke(() => handler(this, EventArgs.Empty));
                    }
                };
            _UseOwnEventHandler = true;
        }

        #endregion

        #region data

        readonly Action<T> _execute;

        readonly Predicate<T> _canExecute;

        private readonly bool _UseOwnEventHandler;

        private event EventHandler _CanExecuteChanged;

        #endregion

        #region ICommand Members

        [System.Diagnostics.DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_UseOwnEventHandler) _CanExecuteChanged += value;
                else CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_UseOwnEventHandler) _CanExecuteChanged -= value;
                else CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter) { _execute((T)parameter); }

        #endregion // ICommand Members
    }

    [System.Diagnostics.DebuggerDisplay("{Name} {_execute}")]
    public class NamedRelayCommand : RelayCommand
    {
        public NamedRelayCommand(string name, Action execute, Func<bool> canExecute = null) : base(canExecute, execute, null) { Name = name; }

        public string Name { get; private set; }
    }

    [System.Diagnostics.DebuggerDisplay("{Name} {_execute}")]
    public class NamedRelayCommand<T> : RelayCommand<T>
    {
        public NamedRelayCommand(string name, Action<T> execute) : base(execute) { Name = name; }

        public NamedRelayCommand(string name, Action<T> execute, Predicate<T> canExecute) : base(execute, canExecute) { Name = name; }

        public string Name { get; private set; }
    }

    public class DelegatedCommand : ICommand
    {
        public DelegatedCommand(Action<ICommand> action, ICommand baseCommand)
            : this((cmd, arg) => action(cmd), baseCommand)
        {
        }

        public DelegatedCommand(Action<ICommand, object> action, ICommand baseCommand)
        {
            _Action = action ?? throw new ArgumentNullException("action");
            _BaseCommand = baseCommand ?? throw new ArgumentNullException("baseCommand");
        }

        private readonly Action<ICommand, object> _Action;
        private readonly ICommand _BaseCommand;

        public bool CanExecute(object parameter)
        {
            return _BaseCommand.CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { _BaseCommand.CanExecuteChanged += value; }
            remove { _BaseCommand.CanExecuteChanged -= value; }
        }

        public void Execute(object parameter)
        {
            _Action(_BaseCommand, parameter);
        }
    }

    /// <summary>
    /// Normally if a command can't be executed we want its <c>CanExecute</c> to return false.
    /// However, rarely we want a command which always appears to be executable but which may nop if it turns out that it wasn't.
    /// </summary>
    public class DelegateOrNopCommand : ICommand
    {
        public DelegateOrNopCommand(ICommand delegatedCommand)
        {
            _DelegatedCommand = delegatedCommand ?? throw new ArgumentNullException(nameof(delegatedCommand));
        }

        #region Data

        private readonly ICommand _DelegatedCommand;

        #endregion

        #region ICommand implementation

        // Unnecessary: it never changes.
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            if (_DelegatedCommand.CanExecute(parameter)) _DelegatedCommand.Execute(parameter);
        }

        #endregion
    }
}
