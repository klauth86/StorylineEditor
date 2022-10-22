﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;

namespace StorylineEditor.ViewModel.Common
{
    public abstract class BaseVM<T> : SimpleVM<T> where T : BaseM
    {
        public BaseVM(T model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        public string Id => Model.id;

        public string Name
        {
            get => Model.name;
            set
            {
                if (Model.name != value)
                {
                    Model.name = value;
                    OnModelChanged(Model, nameof(Name));
                }
            }
        }

        public string Description
        {
            get => Model.description;
            set
            {
                if (Model.description != value)
                {
                    Model.description = value;
                    OnModelChanged(Model, nameof(Description));
                }
            }
        }
    }
}