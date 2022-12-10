using StorylineEditor.Model;
using StorylineEditor.ViewModel.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class TreePathVM : SimpleVM<HistoryVM>
    {
        public TreePathVM(HistoryVM parent, ICallbackContext callbackContext) : base(parent, callbackContext)
        {
            IsActive = true;
            Graph = null;
            PassedNodes = new ObservableCollection<Notifier>();
        }

        protected bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value != isActive)
                {
                    isActive = value;
                    //Parent?.ShowAvailabilityAdorners();
                }
            }
        }

        public Notifier Graph { get; set; }

        public ObservableCollection<Notifier> PassedNodes { get; }

        public void AddNode(Notifier node)
        {
            PassedNodes.Add(node); //Parent?.ShowAvailabilityAdorners();
        }

        public void RemoveNode(Notifier node)
        {
            if (PassedNodes.Remove(node))
            {
                //Parent?.ShowAvailabilityAdorners();
            }
        }

        public Notifier NodeToAdd { get => null; set => AddNode(value); }

        protected ICommand removePassedNodeCommand;
        public ICommand RemovePassedNodeCommand => removePassedNodeCommand ?? (removePassedNodeCommand = new RelayCommand<Notifier>((node) => RemoveNode(node), (node) => node != null));

        public override string Id => null;
        public override string Title => null;
        public override string Stats => null;
    }

    public class JournalEntryVM : SimpleVM<HistoryVM>
    {
        public JournalEntryVM(HistoryVM parent, ICallbackContext callbackContext) : base(parent, callbackContext)
        {
            Quest = null;
            KnownNodes = new ObservableCollection<Notifier>();
            PassedNodes = new ObservableCollection<Notifier>();
        }

        public Notifier Quest { get; set; }

        public ObservableCollection<Notifier> KnownNodes { get; }

        public void AddKnownNode(Notifier node)
        {
            if (!KnownNodes.Contains(node))
            {
                KnownNodes.Add(node); //Parent?.ShowAvailabilityAdorners();
            }
        }

        public void RemoveKnownNode(Notifier node)
        {
            if (KnownNodes.Remove(node))
            { //Parent?.ShowAvailabilityAdorners();
            }
        }

        public Notifier KnownNodeToAdd { get => null; set => AddKnownNode(value); }

        protected ICommand removeKnownNodeCommand;
        public ICommand RemoveKnownNodeCommand => removeKnownNodeCommand ?? (removeKnownNodeCommand = new RelayCommand<Notifier>((node) => RemoveKnownNode(node), (node) => node != null));

        public ObservableCollection<Notifier> PassedNodes { get; }

        public void AddPassedNode(Notifier node)
        {
            if (!PassedNodes.Contains(node))
            {
                PassedNodes.Add(node); //Parent?.ShowAvailabilityAdorners();
            }
        }

        public void RemovePassedNode(Notifier node)
        {
            if (PassedNodes.Remove(node))
            { //Parent?.ShowAvailabilityAdorners();
            }
        }

        protected ICommand addPassedNodeCommand;
        public ICommand AddPassedNodeCommand => addPassedNodeCommand ?? (addPassedNodeCommand = new RelayCommand<Notifier>((node) => AddPassedNode(node), (node) => node != null && !PassedNodes.Contains(node)));

        protected ICommand removePassedNodeCommand;
        public ICommand RemovePassedNodeCommand => removePassedNodeCommand ?? (removePassedNodeCommand = new RelayCommand<Notifier>((node) => RemovePassedNode(node), (node) => node != null && PassedNodes.Contains(node)));

        public override string Id => null;
        public override string Title => null;
        public override string Stats => null;
    }

    public class RelationEntryVM : SimpleVM<HistoryVM>
    {
        public RelationEntryVM(HistoryVM parent, ICallbackContext callbackContext) : base(parent, callbackContext)
        {
            Character = null;
            DeltaRelation = 0;
        }

        public Notifier Character { get; set; }

        protected float deltaRelation;
        public float DeltaRelation
        {
            get => deltaRelation;
            set
            {
                if (value != deltaRelation)
                {
                    deltaRelation = value;
                    //Parent?.ShowAvailabilityAdorners();
                }
            }
        }

        public override string Id => null;
        public override string Title => null;
        public override string Stats => null;
    }

    public class HistoryVM : SimpleVM<StorylineVM>
    {
        public HistoryVM(StorylineVM parent, ICallbackContext callbackContext) : base(parent, callbackContext)
        {
            Inventory = new ObservableCollection<Notifier>();

            PassedDialogsAndReplicas = new ObservableCollection<TreePathVM>();

            JournalEntries = new ObservableCollection<JournalEntryVM>();
            JournalRecords = new ObservableCollection<Notifier>();

            RelationEntries = new ObservableCollection<RelationEntryVM>();
            Characters = new ObservableCollection<Notifier>();

            GenderToPlay = 1;
        }

        public ObservableCollection<Notifier> Inventory { get; }

        public void PickUpItem(Notifier item)
        {
            Inventory.Add(item);
            //ShowAvailabilityAdorners();
        }
        public void DropItem(Notifier item)
        {
            if (Inventory.Remove(item))
            {
                //ShowAvailabilityAdorners();
            }
        }

        public Notifier ItemToAdd { get => null; set => PickUpItem(value); }

        protected ICommand removeItemCommand;
        public ICommand RemoveItemCommand => removeItemCommand ?? (removeItemCommand = new RelayCommand<Notifier>((item) => DropItem(item), (item) => item != null));

        public ObservableCollection<TreePathVM> PassedDialogsAndReplicas { get; }

        public void AddDialogTree(TreePathVM treePath)
        {
            PassedDialogsAndReplicas.Add(treePath);
            //ShowAvailabilityAdorners();
        }
        public void RemoveDialogTreePath(TreePathVM treePath)
        {
            if (PassedDialogsAndReplicas.Remove(treePath))
            {
                //ShowAvailabilityAdorners();
            }
        }

        public Notifier DialogOrReplicaToAdd { get => null; set { if (value != null) AddDialogTree(new TreePathVM(this, Model) { Graph = value }); } }

        protected ICommand removeDialogsAndReplicasCommand;
        public ICommand RemoveDialogsAndReplicasCommand => removeDialogsAndReplicasCommand ?? (removeDialogsAndReplicasCommand = new RelayCommand<TreePathVM>((treePath) => RemoveDialogTreePath(treePath), (treePath) => treePath != null));

        public ObservableCollection<JournalEntryVM> JournalEntries { get; }
        public ObservableCollection<Notifier> JournalRecords { get; }

        public JournalEntryVM AddJournalTree(Notifier tree)
        {
            if (!JournalRecords.Contains(tree))
            {
                JournalRecords.Add(tree);

                JournalEntryVM journalEntry = new JournalEntryVM(this, Model) { Quest = tree };
                JournalEntries.Add(journalEntry);

                //ShowAvailabilityAdorners();

                return journalEntry;
            }

            return JournalEntries[JournalRecords.IndexOf(tree)];
        }
        public void RemoveJournalEntry(JournalEntryVM journalEntry)
        {
            if (JournalEntries.Remove(journalEntry))
            {
                JournalRecords.Remove(journalEntry.Quest);

                //ShowAvailabilityAdorners();
            }
        }

        public Notifier JournalEntryToAdd { get => null; set => AddJournalTree(value); }

        protected ICommand removeJournalEntryCommand;
        public ICommand RemoveJournalEntryCommand => removeJournalEntryCommand ?? (removeJournalEntryCommand = new RelayCommand<JournalEntryVM>((journalEntry) => RemoveJournalEntry(journalEntry), (journalEntry) => journalEntry != null));

        public ObservableCollection<RelationEntryVM> RelationEntries { get; }

        public ObservableCollection<Notifier> Characters { get; }

        public float GetRelation(Notifier viewModel)
        {
            if (viewModel is CharacterVM characterViewModel)
            {
                float result = genderToPlay == GENDER.MALE ? characterViewModel.Model.initialRelation : characterViewModel.Model.initialRelationFemale;

                if (Characters.Contains(viewModel))
                {
                    result += RelationEntries[Characters.IndexOf(viewModel)].DeltaRelation;
                }

                return result;
            }

            return 0;
        }

        public RelationEntryVM AddCharacter(Notifier viewModel)
        {
            if (!Characters.Contains(viewModel))
            {
                Characters.Add(viewModel);

                RelationEntryVM relationEntry = new RelationEntryVM(this, Model) { Character = viewModel };
                RelationEntries.Add(relationEntry);

                //ShowAvailabilityAdorners();

                return relationEntry;
            }

            return RelationEntries[Characters.IndexOf(viewModel)];
        }

        public void RemoveRelationEntry(RelationEntryVM relationEntry)
        {
            if (RelationEntries.Remove(relationEntry))
            {
                Characters.Remove(relationEntry.Character);

                //ShowAvailabilityAdorners();
            }
        }

        public Notifier RelationEntryToAdd { get => null; set => AddCharacter(value); }

        protected ICommand removeRelationEntryCommand;
        public ICommand RemoveRelationEntryCommand => removeRelationEntryCommand ?? (removeRelationEntryCommand = new RelayCommand<RelationEntryVM>((relationEntry) => RemoveRelationEntry(relationEntry), (relationEntry) => relationEntry != null));

        protected int genderToPlay;
        public int GenderToPlay
        {
            get => genderToPlay;
            set
            {
                if (value != genderToPlay)
                {
                    genderToPlay = value;
                    Notify(nameof(GenderToPlay));
                }
            }
        }

        public override string Id => null;
        public override string Title => null;
        public override string Stats => null;
    }
}