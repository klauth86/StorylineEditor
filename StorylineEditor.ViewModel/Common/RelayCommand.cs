/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
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