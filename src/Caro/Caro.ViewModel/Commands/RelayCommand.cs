/*
CommandManager class is in namespace System.Windows.Input but we must add reference "PresentationCore" earlier to use it
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input; // interface ICommand

namespace Caro.ViewModel
{
    public class RelayCommand<T> : ICommand
    {
        #region Fields
        private readonly Predicate<T> _canExecute; // store condition to run command (delegate methods)
        private readonly Action<T> _execute;    // store delegate methods like add, clear, remove, etc. database
        #endregion

        #region Constructors
        /// <summary>
        /// When constructoring, pass "condition to run command" and "delegate method when running command"
        /// </summary>
        /// <param name="canExecute"></param>
        /// <param name="execute"></param>
        public RelayCommand(Predicate<T> canExecute, Action<T> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _canExecute = canExecute;
            _execute = execute;
        }
        #endregion

        #region ICommand members
        /// <summary>
        /// Condition to run command
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        /// <summary>
        /// Delegate method when running command
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        /// <summary>
        /// Create a corresponding name event to delegate
        /// </summary>
        public event EventHandler CanExecuteChanged
        {   // Add and remove to CommandManager
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion
    }
}