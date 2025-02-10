﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WoodworkManagementApp.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
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
            if (parameter == null && typeof(T).IsValueType)
                return false;

            return _canExecute?.Invoke(parameter == null ? default : (T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter == null ? default : (T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}