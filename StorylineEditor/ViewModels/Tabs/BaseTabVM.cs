/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    [XmlRoot]
    public abstract class BaseTabVm<T, TParent> : BaseVm<TParent>
        where T : BaseNamedVm<BaseVm<TParent>>
        where TParent : BaseVm
    {
        public BaseTabVm(TParent inParent) : base(inParent) { items = new ObservableCollection<T>(); }

        public BaseTabVm() : this(null) { }

        [XmlArray]
        protected ObservableCollection<T> items;
        public ObservableCollection<T> Items => items;

        public abstract T CreateItem(object parameter);

        ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<object>((parameter) => AddImpl(CreateItem(parameter))));
        public virtual void AddImpl(T itemToAdd) { if (itemToAdd != null) Items.Add(itemToAdd); }



        ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand<T>((item) => RemoveImpl(item), (item) => item != null));
        public virtual bool RemoveImpl(T itemToRemove) { return Items.Remove(itemToRemove); }

        public virtual void EditItem(T item) { }

        ICommand editCommand;
        public ICommand EditCommand => editCommand ?? (editCommand = new RelayCommand<T>((item) => { EditItem(item); }, (item) => item != null));

        public override void SetupParenthood()
        {
            foreach (var item in Items)
            {
                item.Parent = this;
                item.SetupParenthood();
            }
        }
    }
}