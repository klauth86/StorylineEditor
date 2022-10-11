/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
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

    public class JournalEntryVm : BaseVm<TreePlayerHistoryVm>
    {
        public JournalEntryVm(TreePlayerHistoryVm parent, long additionalTicks) : base(parent, additionalTicks)
        {
            Tree = null;
            KnownNodes = new ObservableCollection<Node_BaseVm>();
            PassedNodes = new ObservableCollection<Node_BaseVm>();
        }

        public JournalEntryVm() : this(null, 0) { }

        public TreeVm Tree { get; set; }

        public ObservableCollection<Node_BaseVm> KnownNodes { get; set; }

        public ObservableCollection<Node_BaseVm> PassedNodes { get; set; }

        protected ICommand removeKnownNodeCommand;
        public ICommand RemoveKnownNodeCommand =>
            removeKnownNodeCommand ?? (removeKnownNodeCommand = new RelayCommand<Node_BaseVm>((node) => { KnownNodes.Remove(node); }, (node) => node != null));

        public Node_BaseVm KnownNodeToAdd { get => null; set { if (value != null) KnownNodes.Add(value); } }

        protected ICommand addPassedNodeCommand;
        public ICommand AddPassedNodeCommand =>
            addPassedNodeCommand ?? (addPassedNodeCommand = new RelayCommand<Node_BaseVm>((node) => { PassedNodes.Add(node); }, (node) => node != null && !PassedNodes.Contains(node)));

        protected ICommand removePassedNodeCommand;
        public ICommand RemovePassedNodeCommand =>
            removePassedNodeCommand ?? (removePassedNodeCommand = new RelayCommand<Node_BaseVm>((node) => { PassedNodes.Remove(node); }, (node) => node != null && PassedNodes.Contains(node)));
    }

    public class RelationEntryVm : BaseVm<TreePlayerHistoryVm>
    {
        public RelationEntryVm(TreePlayerHistoryVm parent, long additionalTicks) : base(parent, additionalTicks)
        {
            Character = null;
            DeltaRelation = 0;
        }

        public RelationEntryVm() : this(null, 0) { }

        public CharacterVm Character { get; set; }

        public float DeltaRelation { get; set; }
    }

    public class TreePlayerHistoryVm : BaseVm<FullContextVm>
    {
        public TreePlayerHistoryVm(FullContextVm parent, long additionalTicks) : base(parent, additionalTicks)
        {
            Inventory = new ObservableCollection<FolderedVm>();

            PassedDialogsAndReplicas = new ObservableCollection<TreePathVm>();

            JournalEntries = new ObservableCollection<JournalEntryVm>();
            
            JournalRecords = new ObservableCollection<TreeVm>();

            RelationEntries = new ObservableCollection<RelationEntryVm>();

            Characters = new ObservableCollection<CharacterVm>();

            GenderToPlay = 1;
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

        public float GetRelation(FolderedVm foldered)
        {
            if (foldered is CharacterVm character)
            {
                float result = character.InitialRelationMale;

                if (Characters.Contains(character))
                {
                    result += RelationEntries[Characters.IndexOf(character)].DeltaRelation;
                }

                return result;
            }

            return 0;
        }

        public ObservableCollection<TreePathVm> PassedDialogsAndReplicas { get; private set; }

        protected ICommand removeDialogsAndReplicasCommand;
        public ICommand RemoveDialogsAndReplicasCommand =>
            removeDialogsAndReplicasCommand ?? (removeDialogsAndReplicasCommand = new RelayCommand<TreePathVm>((item) => { PassedDialogsAndReplicas.Remove(item); }, (item) => item != null));

        public FolderedVm DialogOrReplicaToAdd { get => null; set { if (value != null) PassedDialogsAndReplicas.Add(new TreePathVm(this, 0) { Tree = (TreeVm)value }); } }

        public ObservableCollection<JournalEntryVm> JournalEntries { get; private set; }
        public ObservableCollection<TreeVm> JournalRecords { get; private set; }


        public FolderedVm JournalEntryToAdd
        {
            get => null; set
            {
                if (value is TreeVm tree)
                {
                    if (!JournalRecords.Contains(tree))
                    {
                        JournalRecords.Add(tree);
                        JournalEntries.Add(new JournalEntryVm(this, 0) { Tree = tree });
                    }
                }
            }
        }

        protected ICommand removeJournalEntryCommand;
        public ICommand RemoveJournalEntryCommand =>
            removeJournalEntryCommand ?? (removeJournalEntryCommand = new RelayCommand<JournalEntryVm>((journalEntry) =>
            {
                JournalEntries.Remove(journalEntry);
                JournalRecords.Remove(journalEntry.Tree);
            }, (journalEntry) => journalEntry != null));


        public ObservableCollection<RelationEntryVm> RelationEntries { get; }
        public ObservableCollection<CharacterVm> Characters { get; }

        public FolderedVm RelationEntryToAdd
        {
            get => null; set
            {
                if (value is CharacterVm character)
                {
                    if (!Characters.Contains(character))
                    {
                        Characters.Add(character);
                        RelationEntries.Add(new RelationEntryVm(this, 0) { Character = character });
                    }
                }
            }
        }

        protected ICommand removeRelationEntryCommand;
        public ICommand RemoveRelationEntryCommand =>
            removeRelationEntryCommand ?? (removeRelationEntryCommand = new RelayCommand<RelationEntryVm>((relationEntry) =>
            {
                RelationEntries.Remove(relationEntry);
                Characters.Remove(relationEntry.Character);
            }, (relationEntry) => relationEntry != null));

        protected int genderToPlay;
        public int GenderToPlay
        {
            get => genderToPlay;
            set
            {
                if (value != genderToPlay)
                {
                    genderToPlay = value;
                    NotifyWithCallerPropName();                
                }
            }
        }
    }
}