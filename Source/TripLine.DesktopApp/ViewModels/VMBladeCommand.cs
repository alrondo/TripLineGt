using System;
using System.Diagnostics;
using System.Windows.Input;

namespace TripLine.DesktopApp.ViewModels
{
    public class VMBladeCommand : ICommand
    {
        #region Fields

        readonly Action _execute = null;
        readonly Func<bool> _canExecute = null;

        private string _content;

        #endregion // Fields

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        #region Constructors

        public VMBladeCommand(Action execute)
            : this(execute, null, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="content"></param>
        public VMBladeCommand(Action execute, Func<bool> canExecute, string content)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
            _content = content;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        #endregion // ICommand Members
    }
}