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
        public BaseVm(long additionalTicks)
        {
            DateTime now = DateTime.Now;
            id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", GetType().Name, now, now.Ticks, additionalTicks);
            name = null;
            description = null;
            actorName = null;
            classPathName = null;

            isVisible = true;
        }

        public BaseVm() : this(0) { }

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
                    NotifyNameChanged();
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

        public virtual void NotifyNameChanged() { Notify(nameof(Name)); }

        public virtual void NotifyItemNameChanged(BaseVm renamedVm) { }

        public virtual void NotifyIsValidChanged() { Notify(nameof(IsValid)); }

        public virtual bool PassFilter(string filter) => 
            name != null && name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || 
            description != null && description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
            actorName != null && actorName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
            classPathName != null && classPathName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

        protected void OnSearchFilterChanged(string filter) => IsVisible = string.IsNullOrEmpty(filter) || PassFilter(filter);

        public virtual bool OnRemoval() { return true; }

        public BaseVm Clone(BaseVm Parent, long additionalTicks)
        {
            var result = CustomByteConverter.CreateByName(GetType().Name, Parent, additionalTicks);
            CloneInternalData(result, additionalTicks);
            return result;
        }

        public T Clone<T>(BaseVm Parent, long additionalTicks) where T : BaseVm => Clone(Parent, additionalTicks) as T;

        protected virtual void CloneInternalData(BaseVm destObj, long additionalTicks)
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

        public BaseVm(T inParent, long additionalTicks) : base(additionalTicks) { Parent = inParent; }

        public BaseVm(T inParent) : base() { Parent = inParent; }

        public override bool IsValid => base.IsValid && Parent != null;

        public override void NotifyIsValidChanged() { base.NotifyIsValidChanged(); Parent?.NotifyIsValidChanged(); }

        public override void NotifyNameChanged() { base.NotifyNameChanged(); Parent?.NotifyItemNameChanged(this); }

        public override bool OnRemoval() { Parent = null; return base.OnRemoval(); }
    }

    public abstract class BaseNamedVm<T> : BaseVm<T> where T : BaseVm
    {
        public BaseNamedVm(T inParent, long additionalTicks) : base(inParent, additionalTicks) { }

        public BaseNamedVm(T inParent) : base(inParent) { }

        public override bool IsValid => base.IsValid && !string.IsNullOrEmpty(name);
    }
}