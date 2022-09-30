/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Collections.ObjectModel;
using System.Windows.Input;
using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.Views;

namespace StorylineEditor.ViewModels
{
    public class TreePathVm : BaseVm<TreePlayerHistoryVm>
    {
        public TreePathVm(TreePlayerHistoryVm parent, long additionalTicks) : base(parent, additionalTicks)
        {
            IsActive = true;
            Tree = null;
            PassedNodes = new ObservableCollection<Node_BaseVm>();
        }

        public TreePathVm() : this(null, 0) { }

        public bool IsActive { get; set; }

        public TreeVm Tree { get; set; }

        public ObservableCollection<Node_BaseVm> PassedNodes { get; set; }

        protected ICommand removePassedNodeCommand;
        public ICommand RemovePassedNodeCommand =>
            removePassedNodeCommand ?? (removePassedNodeCommand = new RelayCommand<Node_BaseVm>((node) => { PassedNodes.Remove(node); }, (node) => node != null));

        public Node_BaseVm NodeToAdd { get => null; set { if (value != null) PassedNodes.Add(value); } }
    }

    public class JournalNodeStateVm : BaseVm<JournalPathVm>
    {
        public JournalNodeStateVm(JournalPathVm parent, long additionalTicks) : base(parent, additionalTicks) { }

        public JournalNodeStateVm() : this(null, 0) { }

        public bool IsChecked { get; set; }
    }

    public class JournalPathVm : BaseVm<TreePlayerHistoryVm>
    {
        public JournalPathVm(TreePlayerHistoryVm parent, long additionalTicks) : base(parent, additionalTicks)
        {
            JournalNodesStates = new ObservableCollection<JournalNodeStateVm>();
        }

        public JournalPathVm() : this(null, 0) { }

        public override bool PassFilter(string filter)
        {
            if (!base.PassFilter(filter)) return false;

            if (Parent != null)
            {
                foreach (var journalPath in Parent.Journal)
                {
                    if (journalPath?.Tree == Tree) return false;
                }
            }

            return true;
        }

        private TreeVm tree;
        public TreeVm Tree
        {
            get => tree;
            set
            {
                if (value != tree)
                {
                    tree = value;
                    NotifyWithCallerPropName();

                    foreach (var node in tree.NodesTraversal())
                    {
                        if (node != null) JournalNodesStates.Add(new JournalNodeStateVm(this, 0) { Name = node.Name });
                    }
                }
            }
        }

        public ObservableCollection<JournalNodeStateVm> JournalNodesStates { get; set; }
    }

    public class TreePlayerHistoryVm : BaseVm<FullContextVm>
    {
        public TreePlayerHistoryVm(FullContextVm parent, long additionalTicks) : base(parent, additionalTicks)
        {
            Inventory = new ObservableCollection<FolderedVm>();

            PassedDialogsAndReplicas = new ObservableCollection<TreePathVm>();

            Journal = new ObservableCollection<JournalPathVm>();
        }

        public void ShowAvailabilityAdorners() { GlobalFilterHelper.ShowAvailabilityAdorners = true; }

        public void HideAvailabilityAdorners() { GlobalFilterHelper.ShowAvailabilityAdorners = false; }

        public TreePlayerHistoryVm() : this(null, 0) { }

        public ObservableCollection<FolderedVm> Inventory { get; private set; }

        public void PickUpItem(FolderedVm item)
        {
            Inventory.Add(item);
            ShowAvailabilityAdorners();
        }

        public void DropItem(FolderedVm item)
        {
            if (Inventory.Remove(item))
            {
                ShowAvailabilityAdorners();
            }
        }

        public ItemVm ItemToAdd
        {
            get => null;
            set { if (value != null) PickUpItem(value); }
        }

        protected ICommand removeItemCommand;
        public ICommand RemoveItemCommand => removeItemCommand ?? (removeItemCommand = new RelayCommand<FolderedVm>(
            (item) => DropItem(item), (item) => item != null));

        public bool HasItem(ItemVm item) => Inventory.Contains(item);

        public ObservableCollection<TreePathVm> PassedDialogsAndReplicas { get; private set; }

        protected ICommand removeDialogsAndReplicasCommand;
        public ICommand RemoveDialogsAndReplicasCommand =>
            removeDialogsAndReplicasCommand ?? (removeDialogsAndReplicasCommand = new RelayCommand<TreePathVm>((item) => { PassedDialogsAndReplicas.Remove(item); }, (item) => item != null));

        public FolderedVm DialogOrReplicaToAdd { get => null; set { if (value != null) PassedDialogsAndReplicas.Add(new TreePathVm(this, 0) { Tree = (TreeVm)value }); } }

        public ObservableCollection<JournalPathVm> Journal { get; private set; }

        protected ICommand removeJournalCommand;
        public ICommand RemoveJournalCommand =>
            removeJournalCommand ?? (removeJournalCommand = new RelayCommand<JournalPathVm>((item) => { Journal.Remove(item); }, (item) => item != null));

        protected ICommand addJournalCommand;
        public ICommand AddJournalCommand =>
            addJournalCommand ?? (addJournalCommand = new RelayCommand<FolderedVm>((item) => { Journal.Add(new JournalPathVm(this, 0) { Tree = (TreeVm)item }); }, (item) => item != null));
    }
}