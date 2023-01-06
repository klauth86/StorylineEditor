/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
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

    public class HistoryItemVM : SimpleVM<BaseM, HistoryVM>
    {
        public HistoryItemVM(BaseM model, HistoryVM parent) : base(model, parent) { }

        public override string Id => throw new NotImplementedException();
    }

    public class DialogEntryVM : HistoryItemVM
    {
        public DialogEntryVM(BaseM model, HistoryVM parent) : base(model, parent)
        {
            Nodes = new ObservableCollection<BaseM>();
            _nodesCVSInit = false;
            _nodesCVS = new CollectionViewSource();
        }

        protected bool _nodesCVSInit;

        protected CollectionViewSource _nodesCVS;
        public CollectionViewSource NodesCVS
        {
            get
            {
                if (_nodesCVS.View == null)
                {
                    _nodesCVS.Source = GetModel<GraphM>().nodes;

                    _nodesCVS.View.Filter = OnNodesFilter;
                    _nodesCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                    _nodesCVS.View.MoveCurrentTo(null);

                    _nodesCVSInit = true;
                }

                return _nodesCVS;
            }
        }

        public ObservableCollection<BaseM> Nodes { get; }

        protected string nodesFilter;
        public string NodesFilter
        {
            set
            {
                if (value != nodesFilter)
                {
                    nodesFilter = value;
                    NodesCVS.View?.Refresh();
                }
            }
        }

        public BaseM Node { get => null; set => AddNode(value); }

        private bool OnNodesFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(nodesFilter) || model.PassFilter(nodesFilter);
            }
            return false;
        }
        public void AddNode(BaseM node)
        {
            if (_nodesCVSInit)
            {
                Nodes.Add(node);

                //ShowAvailabilityAdorners();
            }
        }
        public void RemoveNode(BaseM node)
        {
            if (Nodes.Remove(node))
            {
                //ShowAvailabilityAdorners();
            }
        }

        protected ICommand removeNodeCommand;
        public ICommand RemoveNodeCommand => removeNodeCommand ?? (removeNodeCommand = new RelayCommand<BaseM>((node) => RemoveNode(node), (node) => node != null));
    }

    public class QuestEntryVM : HistoryItemVM
    {
        public QuestEntryVM(BaseM model, HistoryVM parent) : base(model, parent)
        {
            KnownNodes = new ObservableCollection<BaseM>();
            _knownNodesCVSInit = false;
            _knownNodesCVS = new CollectionViewSource();

            PassedNodes = new ObservableCollection<BaseM>();
        }

        #region KNOWN

        protected bool _knownNodesCVSInit;

        protected CollectionViewSource _knownNodesCVS;
        public CollectionViewSource KnownNodesCVS
        {
            get
            {
                if (_knownNodesCVS.View == null)
                {
                    _knownNodesCVS.Source = GetModel<GraphM>().nodes;

                    _knownNodesCVS.View.Filter = OnKnownNodesFilter;
                    _knownNodesCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                    _knownNodesCVS.View.MoveCurrentTo(null);

                    _knownNodesCVSInit = true;
                }

                return _knownNodesCVS;
            }
        }

        public ObservableCollection<BaseM> KnownNodes { get; }

        protected string knownNodesFilter;
        public string KnownNodesFilter
        {
            set
            {
                if (value != knownNodesFilter)
                {
                    knownNodesFilter = value;
                    KnownNodesCVS.View?.Refresh();
                }
            }
        }

        public BaseM KnownNode { get => null; set => AddKnownNode(value); }

        private bool OnKnownNodesFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return (string.IsNullOrEmpty(knownNodesFilter) || model.PassFilter(knownNodesFilter)) && KnownNodes.All((knownNode) => knownNode.id != model.id);
            }
            return false;
        }
        public void AddKnownNode(BaseM knownNode)
        {
            if (_knownNodesCVSInit)
            {
                if (!KnownNodes.Contains(knownNode))
                {
                    KnownNodes.Add(knownNode);

                    _knownNodesCVS.View.Refresh();

                    //ShowAvailabilityAdorners();
                }
            }
        }
        public void RemoveKnownNode(BaseM knownNode)
        {
            if (KnownNodes.Remove(knownNode))
            {
                _knownNodesCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }

        protected ICommand removeKnownNodeCommand;
        public ICommand RemoveKnownNodeCommand => removeKnownNodeCommand ?? (removeKnownNodeCommand = new RelayCommand<BaseM>((knownNode) => RemoveKnownNode(knownNode), (knownNode) => knownNode != null));

        #endregion

        public ObservableCollection<BaseM> PassedNodes { get; }

        public void AddPassedNode(BaseM node)
        {
            if (!PassedNodes.Contains(node))
            {
                PassedNodes.Add(node);

                //Parent?.ShowAvailabilityAdorners();
            }
        }

        public void RemovePassedNode(BaseM node)
        {
            if (PassedNodes.Remove(node))
            {
                //Parent?.ShowAvailabilityAdorners();
            }
        }

        protected ICommand addPassedNodeCommand;
        public ICommand AddPassedNodeCommand => addPassedNodeCommand ?? (addPassedNodeCommand = new RelayCommand<BaseM>((node) => AddPassedNode(node), (node) => node != null && !PassedNodes.Contains(node)));

        protected ICommand removePassedNodeCommand;
        public ICommand RemovePassedNodeCommand => removePassedNodeCommand ?? (removePassedNodeCommand = new RelayCommand<BaseM>((node) => RemovePassedNode(node), (node) => node != null && PassedNodes.Contains(node)));
    }

    public class CharacterEntryVM : HistoryItemVM
    {
        public CharacterEntryVM(BaseM model, HistoryVM parent) : base(model, parent)
        {
            DeltaRelation = 0;
        }

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
    }

    public class HistoryVM : Notifier, IDisposable
    {
        public static readonly Random Random = new Random();

        public HistoryVM()
        {
            CharacterEntries = new ObservableCollection<CharacterEntryVM>();
            _charactersCVSInit = false;
            _charactersCVS = new CollectionViewSource();

            Inventory = new ObservableCollection<BaseM>();
            _itemsCVSInit = false;
            _itemsCVS = new CollectionViewSource();

            QuestEntries = new ObservableCollection<QuestEntryVM>();
            _questsCVSInit = false;
            _questsCVS = new CollectionViewSource();

            DialogEntries = new ObservableCollection<DialogEntryVM>();
            _dialogsCVSInit = false;
            _dialogsCVS = new CollectionViewSource();

            FullMode = false;
            Gender = GENDER.MALE;
            Duration = 4;
            TimeLeft = 0;
        }

        #region CHARACTERS

        protected bool _charactersCVSInit;

        protected CollectionViewSource _charactersCVS;
        public CollectionViewSource CharactersCVS
        {
            get
            {
                if (_charactersCVS.View == null)
                {
                    _charactersCVS.Source = ActiveContextService.Characters;

                    _charactersCVS.View.Filter = OnCharactersFilter;
                    _charactersCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                    _charactersCVS.View.MoveCurrentTo(null);

                    _charactersCVSInit = true;
                }

                return _charactersCVS;
            }
        }

        public ObservableCollection<CharacterEntryVM> CharacterEntries { get; }

        protected string charactersFilter;
        public string CharactersFilter
        {
            set
            {
                if (value != charactersFilter)
                {
                    charactersFilter = value;
                    CharactersCVS.View?.Refresh();
                }
            }
        }

        public BaseM Character { get => null; set => AddCharacter(value); }

        private bool OnCharactersFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return (string.IsNullOrEmpty(charactersFilter) || model.PassFilter(charactersFilter)) && CharacterEntries.All((characterEntry) => characterEntry.Model.id != model.id) && model.id != CharacterM.PLAYER_ID;
            }
            return false;
        }
        public void AddCharacter(BaseM character)
        {
            if (_charactersCVSInit)
            {
                foreach (var characterEntry in CharacterEntries)
                {
                    if (characterEntry.Model == character)
                    {
                        return;
                    }
                }

                CharacterEntries.Add(new CharacterEntryVM(character, this));

                _charactersCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }
        public void RemoveCharacter(BaseM character)
        {
            List<CharacterEntryVM> characterEntriesToRemove = new List<CharacterEntryVM>();

            foreach (var characterEntry in CharacterEntries)
            {
                if (characterEntry.Model == character)
                {
                    characterEntriesToRemove.Add(characterEntry);
                }
            }

            if (characterEntriesToRemove.Count > 0)
            {
                foreach (var characterEntryToRemove in characterEntriesToRemove)
                {
                    CharacterEntries.Remove(characterEntryToRemove);
                }

                _charactersCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }

        protected ICommand removeCharacterCommand;
        public ICommand RemoveCharacterCommand => removeCharacterCommand ?? (removeCharacterCommand = new RelayCommand<BaseM>((character) => RemoveCharacter(character), (character) => character != null));

        public float GetRelation(BaseM character)
        {
            //////if (viewModel is CharacterVM characterViewModel)
            //////{
            //////    float result = gender == GENDER.MALE ? characterViewModel.Model.initialRelation : characterViewModel.Model.initialRelationFemale;

            //////    if (Characters.Contains(viewModel))
            //////    {
            //////        result += CharacterEntries[Characters.IndexOf(viewModel)].DeltaRelation;
            //////    }

            //////    return result;
            //////}

            return 0;
        }

        #endregion

        #region INVENTORY

        protected bool _itemsCVSInit;

        protected CollectionViewSource _itemsCVS;
        public CollectionViewSource ItemsCVS
        {
            get
            {
                if (_itemsCVS.View == null)
                {
                    _itemsCVS.Source = ActiveContextService.Items;

                    _itemsCVS.View.Filter = OnItemsFilter;
                    _itemsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                    _itemsCVS.View.MoveCurrentTo(null);

                    _itemsCVSInit = true;
                }

                return _itemsCVS;
            }
        }
        
        public ObservableCollection<BaseM> Inventory { get; }

        protected string itemsFilter;
        public string ItemsFilter
        {
            set
            {
                if (value != itemsFilter)
                {
                    itemsFilter = value;
                    ItemsCVS.View?.Refresh();
                }
            }
        }

        public BaseM Item { get => null; set => PickUpItem(value); }

        private bool OnItemsFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return (string.IsNullOrEmpty(itemsFilter) || model.PassFilter(itemsFilter)) && !Inventory.Contains(model);
            }
            return false;
        }
        public void PickUpItem(BaseM item)
        {
            if (_itemsCVSInit)
            {
                Inventory.Add(item);

                ItemsCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }
        public void DropItem(BaseM item)
        {
            if (Inventory.Remove(item))
            {
                ItemsCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }

        protected ICommand removeItemCommand;
        public ICommand RemoveItemCommand => removeItemCommand ?? (removeItemCommand = new RelayCommand<BaseM>((item) => DropItem(item), (item) => item != null));

        #endregion

        #region JOURNAL

        protected bool _questsCVSInit;

        protected CollectionViewSource _questsCVS;
        public CollectionViewSource QuestsCVS
        {
            get
            {
                if (_questsCVS.View == null)
                {
                    _questsCVS.Source = ActiveContextService.Quests;

                    _questsCVS.View.Filter = OnQuestsFilter;
                    _questsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                    _questsCVS.View.MoveCurrentTo(null);

                    _questsCVSInit = true;
                }

                return _questsCVS;
            }
        }

        public ObservableCollection<QuestEntryVM> QuestEntries { get; }

        protected string questsFilter;
        public string QuestsFilter
        {
            set
            {
                if (value != questsFilter)
                {
                    questsFilter = value;
                    QuestsCVS.View?.Refresh();
                }
            }
        }

        public BaseM Quest { get => null; set => AddQuest(value); }

        private bool OnQuestsFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return (string.IsNullOrEmpty(questsFilter) || model.PassFilter(questsFilter)) && QuestEntries.All((questEntry) => questEntry.Model.id != model.id);
            }
            return false;
        }
        public void AddQuest(BaseM quest)
        {
            if (_questsCVSInit)
            {
                foreach (var questEntry in QuestEntries)
                {
                    if (questEntry.Model == quest)
                    {
                        return;
                    }
                }

                QuestEntries.Add(new QuestEntryVM(quest, this));

                _questsCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }
        public void RemoveQuest(BaseM quest)
        {
            List<QuestEntryVM> questEntriesToRemove = new List<QuestEntryVM>();

            foreach (var questEntry in QuestEntries)
            {
                if (questEntry.Model == quest)
                {
                    questEntriesToRemove.Add(questEntry);
                }
            }

            if (questEntriesToRemove.Count > 0)
            {
                foreach (var questEntryToRemove in questEntriesToRemove)
                {
                    QuestEntries.Remove(questEntryToRemove);
                }

                _questsCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }

        protected ICommand removeQuestCommand;
        public ICommand RemoveQuestCommand => removeQuestCommand ?? (removeQuestCommand = new RelayCommand<BaseM>((quest) => RemoveQuest(quest), (quest) => quest != null));

        #endregion

        #region DIALOGS

        protected bool _dialogsCVSLocked;

        protected bool _dialogsCVSInit;

        protected CollectionViewSource _dialogsCVS;
        public CollectionViewSource DialogsCVS
        {
            get
            {
                if (_dialogsCVS.View == null)
                {
                    _dialogsCVS.Source = ActiveContextService.DialogsAndReplicas;

                    _dialogsCVS.View.Filter = OnDialogsFilter;
                    _dialogsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                    _dialogsCVS.View.MoveCurrentTo(null);

                    _dialogsCVSInit = true;
                }

                return _dialogsCVS;
            }
        }

        public ObservableCollection<DialogEntryVM> DialogEntries { get; }

        protected string dialogsFilter;
        public string DialogsFilter
        {
            set
            {
                if (value != dialogsFilter)
                {
                    dialogsFilter = value;
                    DialogsCVS.View?.Refresh();
                }
            }
        }

        public BaseM Dialog { get => null; set => AddDialog(value); }

        private bool OnDialogsFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return (string.IsNullOrEmpty(dialogsFilter) || model.PassFilter(dialogsFilter));
            }
            return false;
        }
        public void AddDialog(BaseM dialog)
        {
            if (_dialogsCVSInit)
            {
                if (!_dialogsCVSLocked)
                {
                    _dialogsCVSLocked = true;

                    DialogEntries.Add(new DialogEntryVM(dialog, this));

                    _dialogsCVS.View.Refresh();

                    //ShowAvailabilityAdorners();

                    _dialogsCVSLocked = false;
                }
            }
        }
        public void RemoveDialog(DialogEntryVM dialogEntry)
        {
            if (DialogEntries.Remove(dialogEntry))
            {
                _dialogsCVS.View.Refresh();

                //ShowAvailabilityAdorners();
            }
        }

        protected ICommand removeDialogCommand;
        public ICommand RemoveDialogCommand => removeDialogCommand ?? (removeDialogCommand = new RelayCommand<DialogEntryVM>((dialogEntry) => RemoveDialog(dialogEntry), (dialogEntry) => dialogEntry != null));

        #endregion

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

        protected object playerContext;
        public object PlayerContext
        {
            get => playerContext;
            set
            {
                if (value != playerContext)
                {
                    ActiveContextService.ActiveGraph.SetPlayerContext(playerContext, value);

                    playerContext = value;
                    Notify(nameof(PlayerContext));

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        protected ICommand playCommand;
        public ICommand PlayCommand => playCommand ?? (playCommand = new RelayCommand(() =>
        {
            if (StartGraph == null)
            {
                StartGraph = ActiveContextService.ActiveGraph;
                StartNode = StartGraph.SelectionNode;
            }

            if (ActiveContextService.ActiveGraph.Id != StartGraph.Id)
            {
                ICollection_Base collectionBase = (ICollection_Base)ActiveContextService.ActiveTab;

                bool hasSwitched = SwitchToGraph(StartGraph.Id);
                INode startNode = ActiveContextService.ActiveGraph.FindNode(StartNode.Id) ?? ActiveContextService.ActiveGraph.GenerateNode(StartNode.Id);

                if (hasSwitched && startNode != null)
                {
                    OnStartMovement(StartNode);
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                OnStartMovement(StartNode);
            }
        }, () => PlayerContext == null));

        protected void OnStartMovement(IPositioned positioned)
        {
            ActiveNode = null;
            PlayerContext = new PlayerContext_TransitionVM();
            ActiveContextService.ActiveGraph.MoveTo(positioned, (taskStatus) => { if (taskStatus == TaskStatus.RanToCompletion) { OnFinishMovement(positioned); } });
        }

        protected void OnFinishMovement(IPositioned positioned)
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
                        INode targetNode = ActiveContextService.ActiveGraph.FindNode(TargetId);
                        if (targetNode != null)
                        {
                            OnFinishMovement(targetNode);
                        }
                    }
                }
            }
        }

        protected bool SwitchToGraph(string graphId)
        {
            ICollection_Base collectionBase = (ICollection_Base)ActiveContextService.ActiveTab;
            return collectionBase.AddToSelectionById(graphId, true);
        }

        private void PlayNode()
        {
            if (ActiveNode is Node_DialogVM dialogNodeViewModel || ActiveNode is Node_ReplicaVM replicaNodeViewModel)
            {
                const int stepCount = 64;
                int stepDuration = Convert.ToInt32(Math.Round(Duration * 1000) / stepCount);

                MonoTaskFacade.Start((token, alpha) =>
                {
                    if (token.IsCancellationRequested) return TaskStatus.Canceled;

                    if (Math.Abs(alpha - 1) < 0.01) return TaskStatus.RanToCompletion;

                    ActiveContextService.ActiveGraph.TickPlayer(alpha);

                    TimeLeft = (1 - alpha) * Duration;

                    return TaskStatus.Running;
                },
                TimeSpan.FromMilliseconds(stepDuration),
                1.0 / stepCount,
                (taskStatus) =>
                {
                    if (taskStatus == TaskStatus.RanToCompletion)
                    {
                        ActiveContextService.ActiveGraph.TickPlayer(1);

                        TimeLeft = 0;

                        GoToNext();
                    }
                }, null);
            }
            else
            {
                GoToNext();
            }
        }

        private void GoToNext()
        {
            if (ActiveNode is Node_GateVM gateNode)
            {
                PlayerContext = null;

                bool hasTargetAndExit = gateNode.TargetDialog != null && gateNode.TargetExitNode != null;
                bool hasSwitched = SwitchToGraph(gateNode.TargetDialog.id);
                INode startNode = ActiveContextService.ActiveGraph.FindNode(gateNode.TargetExitNode.id) ?? ActiveContextService.ActiveGraph.GenerateNode(gateNode.TargetExitNode.id);

                if (hasTargetAndExit && hasSwitched && startNode != null)
                {
                    OnStartMovement(startNode);
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                NextPaths = ActiveContextService.ActiveGraph.GetNext(ActiveNode.Id);

                FilterByContext(NextPaths);

                ////// TODO Filter nodes and paths that are not available in current context for full mode

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
                    else if (NextPaths.All((pair) => pair.Value.Last().CharacterId == CharacterM.PLAYER_ID))
                    {
                        HashSet<INode> choices = new HashSet<INode>();

                        foreach (var key in NextPaths.Keys)
                        {
                            INode node = ActiveContextService.ActiveGraph.GenerateNode(key);
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
        }

        private void FilterByContext(Dictionary<string, List<IPositioned>> nextPaths)
        {
            List<string> keysToRemove = new List<string>();

            foreach (var key in nextPaths.Keys)
            {
                foreach (var positioned in nextPaths[key])
                {
                    INode node = ActiveContextService.ActiveGraph.FindNode(positioned.Id);
                    
                    if (node != null)
                    {
                        if (node.Gender != Gender && node.Gender > 0) // Filtered by Gender
                        {
                            keysToRemove.Add(key);
                        }
                    }
                }
            }

            foreach (var keyToRemove in keysToRemove)
            {
                nextPaths.Remove(keyToRemove);
            }
        }

        private void MoveThroughPath()
        {
            PlayerContext = new PlayerContext_TransitionVM();

            if (NextPaths[TargetId].Count > 0)
            {
                IPositioned next = NextPaths[TargetId].First();
                ActiveContextService.ActiveGraph.MoveTo(next, (taskStatus) => { if (taskStatus == TaskStatus.RanToCompletion) OnFinishMovement(next); });
            }
        }

        protected ICommand pauseCommand;
        public ICommand PauseCommand => pauseCommand ?? (pauseCommand = new RelayCommand(() => { }, () => PlayerContext != null));

        protected ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(() => { Stop(); }, () => PlayerContext != null));

        private void Stop()
        {
            MonoTaskFacade.Stop();

            TargetId = null;
            NextPaths?.Clear();

            PlayerContext = null;
            ActiveNode = null;
        }

        public void Dispose() { Reset(); }

        public void Reset()
        {
            Stop();

            StartNode = null;
            StartGraph = null;

            _dialogsCVS.View.MoveCurrentTo(null);
            _dialogsCVS.View.SortDescriptions.Clear();
            _dialogsCVS.View.Filter = null;

            _dialogsCVS.Source = null;
            _dialogsCVSInit = false;

            _questsCVS.View.MoveCurrentTo(null);
            _questsCVS.View.SortDescriptions.Clear();
            _questsCVS.View.Filter = null;

            _questsCVS.Source = null;
            _questsCVSInit = false;

            _itemsCVS.View.MoveCurrentTo(null);
            _itemsCVS.View.SortDescriptions.Clear();
            _itemsCVS.View.Filter = null;

            _itemsCVS.Source = null;
            _itemsCVSInit = false;

            _charactersCVS.View.MoveCurrentTo(null);
            _charactersCVS.View.SortDescriptions.Clear();
            _charactersCVS.View.Filter = null;

            _charactersCVS.Source = null;
            _charactersCVSInit = false;
        }

        public IGraph StartGraph { get; set; }
        public INode StartNode { get; set; }
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