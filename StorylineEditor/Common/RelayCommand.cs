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

namespace StorylineEditor.Common {
    public class RelayCommand<T> : ICommand {
        private static bool CanExecute(T parameter) {
            return true;
        }

        readonly Action<T> _execute;
        readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null) {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute ?? CanExecute;
        }
        public bool CanExecute(object parameter) {
            return _canExecute(TranslateParameter(parameter));
        }
        public event EventHandler CanExecuteChanged {
            add {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }
        public void Execute(object parameter) {
            _execute(TranslateParameter(parameter));
        }
        private T TranslateParameter(object parameter) {
            T value;
            if (parameter != null && typeof(T).IsEnum)
                value = (T)Enum.Parse(typeof(T),
                (string)parameter);
            else
                value = (T)parameter;
            return value;
        }
    }

    public class RelayCommand : RelayCommand<object> {
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        : base(obj => execute(), canExecute == null ? null : new Func<object, bool>(obj => canExecute())) {
        }
    }
}