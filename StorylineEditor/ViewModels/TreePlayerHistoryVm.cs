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

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value != isActive)
                {
                    isActive = value;
                    Parent?.ShowAvailabilityAdorners();
                }
            }
        }

        public TreeVm Tree { get; set; }

        public ObservableCollection<Node_BaseVm> PassedNodes { get; set; }

        public void AddNode(Node_BaseVm node) { PassedNodes.Add(node); Parent?.ShowAvailabilityAdorners(); }
        public void RemoveNode(Node_BaseVm node) { if (PassedNodes.Remove(node)) { Parent?.ShowAvailabilityAdorners(); } }

        public Node_BaseVm NodeToAdd { get => null; set { if (value != null) AddNode(value); } }

        protected ICommand removePassedNodeCommand;
        public ICommand RemovePassedNodeCommand => removePassedNodeCommand ??
            (removePassedNodeCommand = new RelayCommand<Node_BaseVm>((node) => RemoveNode(node), (node) => node != null));
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

        public void AddKnownNode(Node_BaseVm node) { if (!KnownNodes.Contains(node)) { KnownNodes.Add(node); Parent?.ShowAvailabilityAdorners(); } }
        public void RemoveKnownNode(Node_BaseVm node) { if (KnownNodes.Remove(node)) { Parent?.ShowAvailabilityAdorners(); } }

        public Node_BaseVm KnownNodeToAdd { get => null; set { if (value != null) AddKnownNode(value); } }

        protected ICommand removeKnownNodeCommand;
        public ICommand RemoveKnownNodeCommand => removeKnownNodeCommand ?? 
            (removeKnownNodeCommand = new RelayCommand<Node_BaseVm>((node) => RemoveKnownNode(node), (node) => node != null));



        public ObservableCollection<Node_BaseVm> PassedNodes { get; set; }

        public void AddPassedNode(Node_BaseVm node) { if (!PassedNodes.Contains(node)) { PassedNodes.Add(node); Parent?.ShowAvailabilityAdorners(); } }
        public void RemovePassedNode(Node_BaseVm node) { if (PassedNodes.Remove(node)) { Parent?.ShowAvailabilityAdorners(); } }

        protected ICommand addPassedNodeCommand;
        public ICommand AddPassedNodeCommand => addPassedNodeCommand ??
            (addPassedNodeCommand = new RelayCommand<Node_BaseVm>((node) => AddPassedNode(node), (node) => node != null && !PassedNodes.Contains(node)));

        protected ICommand removePassedNodeCommand;
        public ICommand RemovePassedNodeCommand => removePassedNodeCommand ??
            (removePassedNodeCommand = new RelayCommand<Node_BaseVm>((node) => RemovePassedNode(node), (node) => node != null && PassedNodes.Contains(node)));
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

        private float deltaRelation;
        public float DeltaRelation
        {
            get => deltaRelation;
            set
            {
                if (value != deltaRelation)
                {
                    deltaRelation = value;
                    Parent?.ShowAvailabilityAdorners();
                }
            }
        }
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



        public ObservableCollection<TreePathVm> PassedDialogsAndReplicas { get; private set; }

        public void AddDialogTree(TreePathVm treePath)
        {
            PassedDialogsAndReplicas.Add(treePath);
            ShowAvailabilityAdorners();
        }
        public void RemoveDialogTreePath(TreePathVm treePath)
        {
            if (PassedDialogsAndReplicas.Remove(treePath))
            {
                ShowAvailabilityAdorners();
            }
        }

        public FolderedVm DialogOrReplicaToAdd { get => null; set { if (value != null) AddDialogTree(new TreePathVm(this, 0) { Tree = (TreeVm)value }); } }

        protected ICommand removeDialogsAndReplicasCommand;
        public ICommand RemoveDialogsAndReplicasCommand => removeDialogsAndReplicasCommand ??
            (removeDialogsAndReplicasCommand = new RelayCommand<TreePathVm>((treePath) => RemoveDialogTreePath(treePath), (treePath) => treePath != null));



        public ObservableCollection<JournalEntryVm> JournalEntries { get; private set; }
        public ObservableCollection<TreeVm> JournalRecords { get; private set; }

        public void AddJournalTree(TreeVm tree)
        {
            if (!JournalRecords.Contains(tree))
            {
                JournalRecords.Add(tree);
                JournalEntries.Add(new JournalEntryVm(this, 0) { Tree = tree });

                ShowAvailabilityAdorners();
            }
        }
        public void RemoveJournalEntry(JournalEntryVm journalEntry)
        {
            if (JournalEntries.Remove(journalEntry))
            {
                JournalRecords.Remove(journalEntry.Tree);

                ShowAvailabilityAdorners();
            }
        }

        public FolderedVm JournalEntryToAdd
        {
            get => null; set { if (value is TreeVm tree) AddJournalTree(tree); }
        }

        protected ICommand removeJournalEntryCommand;
        public ICommand RemoveJournalEntryCommand => removeJournalEntryCommand ??
            (removeJournalEntryCommand = new RelayCommand<JournalEntryVm>((journalEntry) => RemoveJournalEntry(journalEntry), (journalEntry) => journalEntry != null));



        public ObservableCollection<RelationEntryVm> RelationEntries { get; }
        public ObservableCollection<CharacterVm> Characters { get; }

        public float GetRelation(FolderedVm foldered)
        {
            if (foldered is CharacterVm character)
            {
                float result = genderToPlay == Node_BaseVm.MALE ? character.InitialRelationMale : character.InitialRelationFemale;

                if (Characters.Contains(character))
                {
                    result += RelationEntries[Characters.IndexOf(character)].DeltaRelation;
                }

                return result;
            }

            return 0;
        }

        public void AddCharacter(CharacterVm character)
        {
            if (!Characters.Contains(character))
            {
                Characters.Add(character);
                RelationEntries.Add(new RelationEntryVm(this, 0) { Character = character });

                ShowAvailabilityAdorners();
            }
        }
        public void RemoveRelationEntry(RelationEntryVm relationEntry)
        {
            if (RelationEntries.Remove(relationEntry))
            {
                Characters.Remove(relationEntry.Character);

                ShowAvailabilityAdorners();
            }
        }

        public FolderedVm RelationEntryToAdd
        {
            get => null; set { if (value is CharacterVm character) AddCharacter(character); }
        }

        protected ICommand removeRelationEntryCommand;
        public ICommand RemoveRelationEntryCommand => removeRelationEntryCommand ?? 
            (removeRelationEntryCommand = new RelayCommand<RelationEntryVm>((relationEntry) => RemoveRelationEntry(relationEntry), (relationEntry) => relationEntry != null));



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