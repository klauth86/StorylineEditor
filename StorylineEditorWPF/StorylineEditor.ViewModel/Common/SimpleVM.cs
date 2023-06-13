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

using StorylineEditor.Model;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Common
{
    public abstract class SimpleVM<T, U> : Notifier, IWithModel
        where T : class
        where U : class
    {
        public static event Action<T, string> ModelChangedEvent = delegate { };
        public static void OnModelChanged(T model, string propName) => ModelChangedEvent?.Invoke(model, propName);

        private readonly T _model;
        public T Model => _model;

        private readonly U _parent;
        public U Parent => _parent;

        public ModelType GetModel<ModelType>() where ModelType : class { return Model as ModelType; }

        public SimpleVM(T model, U parent)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _parent = parent;

            IsFilterPassed = true;
        }

        private void OnModelChangedHandler(T model, string propName) { if (Model != null && Model == model) Notify(propName); }

        protected virtual void OnFilterChangedHandler(string filter)
        {
            if (Model is BaseM baseModel) IsFilterPassed = string.IsNullOrEmpty(filter) || baseModel.PassFilter(filter);
        }



        protected ICommand registerCommand;
        public ICommand RegisterCommand => registerCommand ?? (registerCommand = new RelayCommand(() => RegisterCommandInternal()));
        protected virtual void RegisterCommandInternal()
        {
            OnFilterChangedHandler(Filter);

            OnFirstFilterChangedPass += OnFilterChangedHandler;
            ModelChangedEvent += OnModelChangedHandler;
        }



        protected ICommand unregisterCommand;
        public ICommand UnregisterCommand => unregisterCommand ?? (unregisterCommand = new RelayCommand(() => UnregisterCommandInternal()));
        protected virtual void UnregisterCommandInternal()
        {
            ModelChangedEvent -= OnModelChangedHandler;
            OnFirstFilterChangedPass -= OnFilterChangedHandler;
        }



        protected ICommand registerContextCommand;
        public ICommand RegisterContextCommand => registerContextCommand ?? (registerContextCommand = new RelayCommand(() => ActiveContext.ActiveCopyPaste = this as ICopyPaste));



        protected ICommand unregisterContextCommand;
        public ICommand UnregisterContextCommand => unregisterContextCommand ?? (unregisterContextCommand = new RelayCommand(() => ActiveContext.ActiveCopyPaste = null));
    }
}