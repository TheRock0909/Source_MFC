using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Source_MFC.Utils
{
    public class Command : ICommand
    {
        Action<object> _executeMethod;
        Func<object, bool> _canexecuteMethod;
        string Name = string.Empty;

        public Command(Action<object> executeMethod) : this(executeMethod, null)
        {

        }

        public Command(Action<object> executeMethod, Func<object, bool> canexecuteMethod)
        {
            this._executeMethod = executeMethod;
            this._canexecuteMethod = canexecuteMethod;
        }

        public Command()
        {

        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            bool rtn = true;
            if ( null != _canexecuteMethod)
            {
                rtn = _canexecuteMethod(parameter);                
            }
            return rtn;
        }

        public void Execute(object parameter)
        {
            _executeMethod(parameter);
        }
    }

    
    public class TreeData : Notifier
    {
        private string name;
        public string b_Name
        {
            get {
                return name;
            }
            set {
                name = value;
                OnPropertyChanged();
            }
        }

        private string parent;
        public string b_Parent
        {
            get {
                return parent;
            }
            set {
                parent = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TreeData> children = new ObservableCollection<TreeData>();
        public ObservableCollection<TreeData> b_Children
        {
            get {
                return children;
            }
            set {
                children = value;
                OnPropertyChanged();
            }
        }

    }

    public class AnotherCommandImplementation : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public AnotherCommandImplementation(Action<object> execute) : this(execute, null)
        {
        }

        public AnotherCommandImplementation(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute ?? (x => true);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add {
                CommandManager.RequerySuggested += value;
            }
            remove {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Refresh()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
