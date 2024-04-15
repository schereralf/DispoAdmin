﻿using System;
using System.Windows.Input;

namespace DispoBaseLib
{
    public class RelayCommand(Action execute, Func<bool> canExecute) : ICommand
    {
        private readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Func<bool> _canExecute = canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute)
            : this(execute, null) {}

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
