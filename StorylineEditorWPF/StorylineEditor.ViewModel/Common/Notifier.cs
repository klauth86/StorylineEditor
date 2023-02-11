﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel;

namespace StorylineEditor.ViewModel.Common
{
    public abstract class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public static event EventHandler FilterChanged = delegate { };

        public static event Action<string> OnFirstFilterChangedPass = delegate { };

        public static event Action<string> OnSecondFilterChangedPass = delegate { };

        protected static string filter;
        public static string Filter
        {
            get => filter;
            set
            {
                if (value != filter)
                {
                    filter = value;
                    FilterChanged(null, EventArgs.Empty);

                    OnFirstFilterChangedPass.Invoke(filter);
                    OnSecondFilterChangedPass.Invoke(filter);
                }
            }
        }

        public abstract string Id { get; }
        public virtual bool IsFolder => false;

        protected bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    Notify(nameof(IsSelected));
                }
            }
        }

        protected bool isCut;
        public bool IsCut
        {
            get => isCut;
            set
            {
                if (value != isCut)
                {
                    isCut = value;
                    Notify(nameof(IsCut));
                }
            }
        }

        protected bool isFilterPassed;
        public bool IsFilterPassed
        {
            get => isFilterPassed;
            set
            {
                if (value != isFilterPassed)
                {
                    isFilterPassed = value;
                    Notify(nameof(IsFilterPassed));
                }
            }
        }
    }
}