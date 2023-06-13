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