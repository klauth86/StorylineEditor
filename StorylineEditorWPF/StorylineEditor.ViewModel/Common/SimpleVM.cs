﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
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