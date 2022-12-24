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
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class PlayerContext_ChoiceVM : Notifier
    {
        protected readonly HistoryVM _parent;

        public PlayerContext_ChoiceVM(HistoryVM parent, HashSet<INode> choices)
        {
            _parent = parent;
            Choices = choices;
        }

        public override string Id => null;

        public HashSet<INode> Choices { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<INode>((node) => { _parent.TargetId = node.Id; }, (node) => node != null && Choices.Contains(node)));
    }

    public class PlayerContext_ErrorVM : Notifier
    {
        protected readonly HistoryVM _parent;

        public PlayerContext_ErrorVM(HistoryVM parent, string description)
        {
            _parent = parent;
            Description = description;
        }

        public override string Id => null;

        public string Description { get; set; }
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
        public static readonly Random Random = new Random();

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
                    ActiveGraph?.SetPlayerContext(playerContext, value);

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

            ActiveGraph.MoveTo(StartNode, (taskStatus) => { if (taskStatus == TaskStatus.RanToCompletion) { OnFinishMovement(StartNode); } });
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
            if (ActiveNode is Node_DialogVM dialogNodeViewModel || ActiveNode is Node_ReplicaVM replciaNodeViewModel)
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
            else
            {
                GoNext();
            }
        }

        private void GoNext()
        {
            NextPaths = ActiveGraph.GetNext(ActiveNode.Id);

            // TODO Filter nodes and paths that are not available in current context for full mode

            if (NextPaths.Count == 1)
            {
                TargetId = NextPaths.First().Key;
            }
            else if (NextPaths.Count > 0)
            {
                if (ActiveNode is Node_RandomVM randomNode)
                {
                    var targetIds = NextPaths.Keys.ToList();
                    TargetId = targetIds[Random.Next(targetIds.Count)];
                }
                else if (ActiveNode is Node_GateVM gateNode)
                {
                    if (gateNode.TargetDialog != null && gateNode.TargetExitNode != null)
                    {
                        // TODO Gates
                    }
                }
                else if (NextPaths.All((pair) => pair.Value.Last().CharacterId == CharacterM.PLAYER_ID))
                {
                    HashSet<INode> choices = new HashSet<INode>();

                    foreach (var key in NextPaths.Keys)
                    {
                        INode node = ActiveGraph.GenerateNode(key);
                        if (node != null)
                        {
                            choices.Add(node);
                        }
                    }

                    PlayerContext = new PlayerContext_ChoiceVM(this, choices);
                }
                else
                {
                    ////// TODO

                    string description = "Дочерние вершины не подходят ни под одну из ситуаций:" + Environment.NewLine;
                    description += Environment.NewLine;

                    description += "- " + "После Случайной вершины (⇝) возможен любой состав дочерних вершин..." + Environment.NewLine;
                    description += Environment.NewLine;

                    description += "- " + "Если НЕ Случайная вершина (💬, ⇴) имеет одну актуальную (удовлетворяющую полу и своим предикатам) дочернюю вершину, то этой вершиной может быть любая вершина кроме вершины Транзит (⇴) с несколькими актуальными (удовлетворяющие полу и своим предикатам) дочерними вершинами..." + Environment.NewLine;
                    description += Environment.NewLine;

                    description += "- " + "Если НЕ Случайная вершина (💬, ⇴) имеет более одной актуальной (удовлетворяющей полу и своим предикатам) дочерней вершины, то эти вершины должны быть либо вершинами Основного персонажа (💬), либо Транзитом (⇴) на вершины Основного персонажа (💬) (ситуация ВЫБОР ИГРОКА)..." + Environment.NewLine;
                    description += Environment.NewLine;

                    PlayerContext = new PlayerContext_ErrorVM(this, description);
                }
            }
            else
            {
                Stop();
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
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(() => { Stop(); }, () => PlayerContext != null));

        private void Stop()
        {
            TaskFacade.StopMonoTask();

            TargetId = null;
            NextPaths?.Clear();

            PlayerContext = null;

            ActiveNode = null;
            ActiveGraph = null;
        }

        public IGraph StartGraph { get; set; }
        public INode StartNode { get; set; }
        public IGraph ActiveGraph { get; set; }
        public INode ActiveNode { get; set; }

        Dictionary<string, List<IPositioned>> NextPaths { get; set; }

        private string targetId;
        public string TargetId
        {
            get => targetId;
            set
            {
                if (targetId != value)
                {
                    targetId = value;
                    if (targetId != null) MoveThroughPath();
                }
            }
        }

        public override string Id => null;
    }
}