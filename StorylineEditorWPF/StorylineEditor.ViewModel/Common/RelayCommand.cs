/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Common
{
    public class RelayCommand<T> : ICommand
    {
        private static bool canExecuteAlwaysTrue(T parameter) => true;

        private readonly Action<T> _execute;

        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(_execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(_canExecute));
        }

        public RelayCommand(Action<T> execute) : this(execute, canExecuteAlwaysTrue) { }

        public event EventHandler CanExecuteChanged
        {
            add { if (_canExecute != null) CommandManager.RequerySuggested += value; }
            remove { if (_canExecute != null) CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute(TranslateParameter(parameter));

        public void Execute(object parameter) => _execute(TranslateParameter(parameter));

        private T TranslateParameter(object parameter)
        {
            return parameter == null ? default(T) : (T)(typeof(T).IsEnum ? Enum.Parse(typeof(T), (string)parameter) : parameter);
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute, Func<bool> canExecute)
            : base(execute == null ? null : new Action<object>((obj) => execute()), canExecute == null ? null : new Func<object, bool>(obj => canExecute())) { }

        public RelayCommand(Action execute)
            : base(execute == null ? null : new Action<object>((obj) => execute())) { }
    }
}