/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Xml.Serialization;

namespace StorylineEditor.Common
{
    [XmlRoot]
    public class BaseVm : Notifier
    {
        public BaseVm()
        {
            id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss_fffffff}", GetType().Name, DateTime.Now);
            name = null;
            description = null;
            actorName = null;
            classPathName = null;

            isVisible = true;
        }

        #region PROPS

        protected string id;

        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected string name;

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected string description;

        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected string actorName;

        public string ActorName
        {
            get => actorName;
            set
            {
                if (actorName != value)
                {
                    actorName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected string classPathName;

        public string ClassPathName
        {
            get => classPathName;
            set
            {
                if (classPathName != value)
                {
                    classPathName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected bool isVisible;
        [XmlIgnore]
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (value != isVisible)
                {
                    isVisible = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        #endregion

        public virtual bool IsValid => !string.IsNullOrEmpty(id);

        public virtual void NotifyIsValidChanged() { Notify(nameof(IsValid)); }

        public virtual bool PassFilter(string filter) => 
            name != null && name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || 
            description != null && description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
            actorName != null && actorName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
            classPathName != null && classPathName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

        protected void OnSearchFilterChanged(string filter) => IsVisible = string.IsNullOrEmpty(filter) || PassFilter(filter);

        public BaseVm Clone(BaseVm Parent)
        {
            var result = CustomByteConverter.CreateByName(GetType().Name, Parent);
            CloneInternalData(result);
            return result;
        }

        public T Clone<T>(BaseVm Parent) where T : BaseVm => Clone(Parent) as T;

        protected virtual void CloneInternalData(BaseVm destObj)
        {
            if (destObj != null)
            {
                destObj.name = name;
                destObj.description = description;
                destObj.actorName = actorName;
                destObj.classPathName = classPathName;
            }
        }
    }

    public abstract class BaseVm<T> : BaseVm where T : BaseVm
    {
        private T parent;

        [XmlIgnore]
        public T Parent { get => parent; set { var oldValue = parent; parent = value; RefreshSubscribtions(oldValue, value); } }

        public virtual void SetupParenthood() { }

        protected virtual void RefreshSubscribtions(T oldValue, T newValue) { }

        public BaseVm(T inParent) : base() { Parent = inParent; }

        public override bool IsValid => base.IsValid && Parent != null;

        public override void NotifyIsValidChanged() { base.NotifyIsValidChanged(); if (Parent != null) Parent.NotifyIsValidChanged(); }
    }

    public abstract class BaseNamedVm<T> : BaseVm<T> where T : BaseVm
    {
        public BaseNamedVm(T inParent) : base(inParent) { }

        public override bool IsValid => base.IsValid && !string.IsNullOrEmpty(name);
    }
}