/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Common;
using System;

namespace StorylineEditor.ViewModel
{
    public class BaseVM<T> : Notifier where T : class
    {
        public static event Action<T, string> ModelChangedEvent = delegate { };
        public static void OnModelChanged(T model, string propName) => ModelChangedEvent?.Invoke(model, propName);



        public readonly T Model;

        public BaseVM(T model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            ModelChangedEvent += OnModelChangedHandler;
        }

        ~BaseVM() { ModelChangedEvent -= OnModelChangedHandler; } ////// TODO Think where to unsubscribe



        private void OnModelChangedHandler(T model, string propName) { if (Model != null && Model == model) Notify(propName); }
    }
}