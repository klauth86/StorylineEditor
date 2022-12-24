/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class PlayerContext_ChoiceVM : SimpleVM<HistoryVM>
    {
        public PlayerContext_ChoiceVM(HistoryVM parent, ICallbackContext callbackContext, Dictionary<Notifier, List<Notifier>> nodesPaths) : base(parent, callbackContext)
        {
            NodesPaths = nodesPaths;
        }

        public Dictionary<Notifier, List<Notifier>> NodesPaths { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<Notifier>((node) =>
        {
            //TreeCanvas?.PrepareAndStartTransition(node, NodesPaths[node]);
        }, (node) => node != null && NodesPaths.ContainsKey(node)));

        public override string Id => null;
        public override string Title => null;
        public override string Stats => null;
    }

    public class PlayerContext_ErrorVM : SimpleVM<HistoryVM>
    {
        public PlayerContext_ErrorVM(HistoryVM parent, ICallbackContext callbackContext) : base(parent, callbackContext) { }

        public override string Id => null;
        public override string Title => null;
        public override string Stats => null;
    }

    public class PlayerContext_TransitionVM { }

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

    public class HistoryVM : Notifier
    {
        public HistoryVM()
        {
            Inventory = new ObservableCollection<Notifier>();

            PassedDialogsAndReplicas = new ObservableCollection<TreePathVM>();

            JournalEntries = new ObservableCollection<JournalEntryVM>();
            JournalRecords = new ObservableCollection<Notifier>();

            RelationEntries = new ObservableCollection<RelationEntryVM>();
            Characters = new ObservableCollection<Notifier>();

            Gender = GENDER.MALE;
            FullMode = false;
            TimeLeft = 0;
            Duration = 4;
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

        public Notifier DialogOrReplicaToAdd { get => null; set { if (value != null) AddDialogTree(new TreePathVM(this, null) { Graph = value }); } }

        protected ICommand removeDialogsAndReplicasCommand;
        public ICommand RemoveDialogsAndReplicasCommand => removeDialogsAndReplicasCommand ?? (removeDialogsAndReplicasCommand = new RelayCommand<TreePathVM>((treePath) => RemoveDialogTreePath(treePath), (treePath) => treePath != null));

        public ObservableCollection<JournalEntryVM> JournalEntries { get; }
        public ObservableCollection<Notifier> JournalRecords { get; }

        public JournalEntryVM AddJournalTree(Notifier tree)
        {
            if (!JournalRecords.Contains(tree))
            {
                JournalRecords.Add(tree);

                JournalEntryVM journalEntry = new JournalEntryVM(this, null) { Quest = tree };
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
                float result = gender == GENDER.MALE ? characterViewModel.Model.initialRelation : characterViewModel.Model.initialRelationFemale;

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

                RelationEntryVM relationEntry = new RelationEntryVM(this, null) { Character = viewModel };
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

        protected byte gender;
        public byte Gender
        {
            get => gender;
            set
            {
                if (value != gender)
                {
                    gender = value;
                    Notify(nameof(Gender));
                }
            }
        }

        protected ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand => toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand(() => Gender = (byte)(3 - Gender), () => PlayerContext == null));

        protected bool fullMode;
        public bool FullMode
        {
            get => fullMode;
            set
            {
                if (value != fullMode)
                {
                    fullMode = value;
                    Notify(nameof(FullMode));
                }
            }
        }

        protected double timeLeft;
        public double TimeLeft
        {
            get => timeLeft;
            set
            {
                if (value != timeLeft)
                {
                    timeLeft = value;
                    Notify(nameof(TimeLeft));
                }
            }
        }

        protected double duration;
        public double Duration
        {
            get => duration;
            set
            {
                if (value != duration)
                {
                    duration = value;
                    Notify(nameof(Duration));
                }
            }
        }

        protected object playerContext;
        public object PlayerContext
        {
            get => playerContext;
            set
            {
                if (value != playerContext)
                {
                    playerContext = value;
                    Notify(nameof(PlayerContext));

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        protected ICommand playCommand;
        public ICommand PlayCommand => playCommand ?? (playCommand = new RelayCommand(() =>
        {
            StartGraph = ActiveContextService.ActiveGraph;
            StartNode = StartGraph.SelectionNode;

            ActiveGraph = StartGraph;
            ActiveNode = null;

            PlayerContext = new PlayerContext_TransitionVM();

            StartGraph.MoveTo(StartNode, (taskStatus) => { if (taskStatus == TaskStatus.RanToCompletion) { OnFinishMovement(StartNode); } });
        }, () => PlayerContext == null));

        private void OnFinishMovement(IPositioned positioned)
        {
            if (positioned is INode node)
            {
                ActiveNode = node;
                PlayerContext = node;

                PlayNode();
            }
            else
            {
                if (NextPaths[TargetId].Remove(positioned))
                {
                    if (NextPaths[TargetId].Count > 0)
                    {
                        MoveThroughPath();
                    }
                    else
                    {
                        INode targetNode = ActiveGraph.FindNode(TargetId);
                        if (targetNode != null)
                        {
                            OnFinishMovement(targetNode);
                        }
                    }
                }
            }
        }

        private void PlayNode()
        {
            const int stepCount = 256;
            int stepDuration = Convert.ToInt32(Math.Round(Duration * 1000) / stepCount);

            TaskFacade.StartMonoTask((token, alpha) =>
            {
                if (token.IsCancellationRequested) return TaskStatus.Canceled;

                if (Math.Abs(alpha - 1) < 0.01) return TaskStatus.RanToCompletion;

                return TaskStatus.Running;
            },
            TimeSpan.FromMilliseconds(stepDuration),
            1.0 / stepCount,
            (taskStatus) =>
            {
                if (taskStatus == TaskStatus.RanToCompletion)
                {
                    GoNext();
                }
            }, null);
        }

        private void GoNext()
        {
            NextPaths = ActiveGraph.GetNext(ActiveNode.Id);

            // TODO Filter nodes and paths that are not available in current context for full mode

            if (NextPaths.Count > 0)
            {
                // TODO Selector dependent on node type

                TargetId = NextPaths.First().Key;

                MoveThroughPath();
            }
            else
            {
                // TODO Stop (no next nodes)
            }
        }

        private void MoveThroughPath()
        {
            PlayerContext = new PlayerContext_TransitionVM();

            if (NextPaths[TargetId].Count > 0)
            {
                IPositioned next = NextPaths[TargetId].First();
                ActiveGraph.MoveTo(next, (taskStatus) => { if (taskStatus == TaskStatus.RanToCompletion) OnFinishMovement(next); });
            }
        }

        protected ICommand pauseCommand;
        public ICommand PauseCommand => pauseCommand ?? (pauseCommand = new RelayCommand(() => { }, () => PlayerContext != null));

        protected ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(() => { }, () => PlayerContext != null));

        public IGraph StartGraph { get; set; }
        public INode StartNode { get; set; }
        public IGraph ActiveGraph { get; set; }
        public INode ActiveNode { get; set; }

        Dictionary<string, List<IPositioned>> NextPaths { get; set; }
        public string TargetId { get; set; }

        public override string Id => null;
    }
}