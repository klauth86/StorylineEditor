/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Service;
using StorylineEditor.ViewModel.Behaviors;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Nodes;
using StorylineEditor.ViewModel.Predicates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class PlayerContext_ChoiceVM : Notifier
    {
        protected readonly HistoryVM _parent;

        public PlayerContext_ChoiceVM(HistoryVM parent, Dictionary<INode, int> choices)
        {
            _parent = parent;
            Choices = choices;
        }

        public override string Id => null;

        public Dictionary<INode, int> Choices { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<INode>((node) => { _parent.PathIndex = Choices[node]; }));
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
        public ICommand RemoveNodeCommand => removeNodeCommand ?? (removeNodeCommand = new RelayCommand<BaseM>((node) => RemoveNode(node)));
    }

    public class QuestNodeEntry : Notifier
    {
        public BaseM Node { get; set; }

        protected bool _isPassed;
        public bool IsPassed
        {
            get => _isPassed;
            set
            {
                if (value != _isPassed)
                {
                    _isPassed = value;
                    Notify(nameof(IsPassed));
                }
            }
        }

        public override string Id => throw new NotImplementedException();
    }

    public class QuestEntryVM : HistoryItemVM
    {
        public QuestEntryVM(BaseM model, HistoryVM parent) : base(model, parent)
        {
            _knownNodesDictionary = new Dictionary<BaseM, int>();
            KnownNodes = new  ObservableCollection<QuestNodeEntry>();
            _knownNodesCVSInit = false;
            _knownNodesCVS = new CollectionViewSource();
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

        protected Dictionary<BaseM, int> _knownNodesDictionary;

        public ObservableCollection<QuestNodeEntry> KnownNodes { get; }

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
                return (string.IsNullOrEmpty(knownNodesFilter) || model.PassFilter(knownNodesFilter)) && KnownNodes.All((knownNode) => knownNode.Node.id != model.id);
            }
            return false;
        }

        public bool GetKnownNodeIsPassed(BaseM knownNode)
        {
            if (_knownNodesDictionary.ContainsKey(knownNode))
            {
                return KnownNodes[_knownNodesDictionary[knownNode]].IsPassed;
            }

            return false;
        }

        public void SetKnownNodeIsPassed(BaseM knownNode, bool isPassed)
        {
            if (_knownNodesDictionary.ContainsKey(knownNode))
            {
                KnownNodes[_knownNodesDictionary[knownNode]].IsPassed = isPassed;
            }
        }

        public bool HasKnownNode(BaseM knownNode) { return _knownNodesDictionary.ContainsKey(knownNode); }

        public void AddKnownNode(BaseM knownNode)
        {
            if (_knownNodesCVSInit)
            {
                if (!_knownNodesDictionary.ContainsKey(knownNode))
                {
                    _knownNodesDictionary.Add(knownNode, KnownNodes.Count);

                    KnownNodes.Add(new QuestNodeEntry() { Node = knownNode, IsPassed = false });
                    _knownNodesCVS.View.Refresh();
                    //ShowAvailabilityAdorners();
                }
            }
        }
        public void RemoveKnownNode(BaseM knownNode)
        {
            if (_knownNodesDictionary.ContainsKey(knownNode))
            {
                KnownNodes.RemoveAt(_knownNodesDictionary[knownNode]);
                _knownNodesCVS.View.Refresh();
                //ShowAvailabilityAdorners();

                foreach (var otherKnownNode in _knownNodesDictionary.Keys.ToList())
                {
                    if (_knownNodesDictionary[otherKnownNode] > _knownNodesDictionary[knownNode])
                    {
                        _knownNodesDictionary[otherKnownNode] = _knownNodesDictionary[otherKnownNode] - 1;
                    }
                }

                _knownNodesDictionary.Remove(knownNode);
            }
        }

        protected ICommand removeKnownNodeCommand;
        public ICommand RemoveKnownNodeCommand => removeKnownNodeCommand ?? (removeKnownNodeCommand = new RelayCommand<BaseM>((knownNode) => RemoveKnownNode(knownNode)));

        #endregion
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
                    Notify(nameof(DeltaRelation));
                    //Parent?.ShowAvailabilityAdorners();
                }
            }
        }
    }

    public class HistoryVM : Notifier, IDisposable
    {
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
            TimeLeft = 0;

            Stop();
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
                    _charactersCVS.Source = ActiveContext.Characters;

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

        public BaseM Character { get => null; set => AddCharacter_Internal(value); }

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
        protected void AddCharacter_Internal(BaseM character)
        {
            if (_charactersCVSInit)
            {
                AddCharacter(character);
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
                    _itemsCVS.Source = ActiveContext.Items;

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

        public BaseM Item { get => null; set => PickUpItem_Internal(value); }

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
            Inventory.Add(item);

            ItemsCVS.View.Refresh();

            //ShowAvailabilityAdorners();
        }
        protected void PickUpItem_Internal(BaseM item)
        {
            if (_itemsCVSInit)
            {
                PickUpItem(item);
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
                    _questsCVS.Source = ActiveContext.Quests;

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

        public BaseM Quest { get => null; set => AddQuest_Internal(value); }

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
        public void AddQuest_Internal(BaseM quest)
        {
            if (_questsCVSInit)
            {
                AddQuest(quest);
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
                    _dialogsCVS.Source = ActiveContext.DialogsAndReplicas;

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

        public BaseM Dialog { get => null; set => AddDialog_Internal(value); }

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
            DialogEntries.Add(new DialogEntryVM(dialog, this));

            _dialogsCVS.View.Refresh();

            //ShowAvailabilityAdorners();
        }
        public void AddDialog_Internal(BaseM dialog)
        {
            if (_dialogsCVSInit)
            {
                if (!_dialogsCVSLocked)
                {
                    _dialogsCVSLocked = true;

                    AddDialog(dialog);

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

        public float PlayRate
        {
            get => ConfigM.Config.PlayRate;
            set
            {
                if (value != ConfigM.Config.PlayRate)
                {
                    ConfigM.Config.PlayRate = value;
                    Notify(nameof(PlayRate));

                    ActiveContext.FileService.SaveConfig();
                }
            }
        }

        public float Duration
        {
            get => ConfigM.Config.Duration;
            set
            {
                if (value != ConfigM.Config.Duration)
                {
                    ConfigM.Config.Duration = value;
                    Notify(nameof(Duration));

                    ActiveContext.FileService.SaveConfig();
                }
            }
        }

        protected bool isDownloading;
        public bool IsDownloading
        {
            get => isDownloading;
            set
            {
                if (isDownloading != value)
                {
                    isDownloading = value;
                    Notify(nameof(IsDownloading));
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
                    ActiveContext.ActiveGraph.SetPlayerContext(playerContext, value);

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
                StartGraph = ActiveContext.ActiveGraph;
                StartNode = StartGraph.SelectionNode;
            }

            bool needToSwitch = ActiveContext.ActiveGraph.Id != StartGraph.Id;
            bool hasSwitched = needToSwitch && SwitchToGraph(StartGraph.Id);

            if (needToSwitch && hasSwitched || !needToSwitch)
            {
                OnStartMovement(StartNode);
            }
            else
            {
                Stop();
            }
        }, () => PlayerContext == null));

        protected void OnStartMovement(IPositioned positioned)
        {
            PlayerContext = new PlayerContext_TransitionVM();
            ActiveContext.ActiveGraph.MoveTo(positioned, (customStatus) => { if (customStatus == CustomStatus.RanToCompletion) { OnFinishMovement(positioned); } }, PlayRate);
        }

        protected void OnFinishMovement(IPositioned positioned)
        {
            if (PathIndex >= 0)
            {
                if (Paths[PathIndex].Remove(positioned))
                {
                    if (Paths[PathIndex].Count > 0)
                    {
                        ExecuteGameEvents(positioned.Id, false); // Enter
                        ExecuteGameEvents(positioned.Id, true); // Leave

                        MoveThroughPath();
                        return;
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException(nameof(PathIndex));
                }
            }

            INode node = ActiveContext.ActiveGraph.FindNode(positioned.Id);
            PlayerContext = node;
            StartPlayNode(node, false);
        }

        private void ExecuteGameEvents(string positionedId, bool isLeave) { ExecuteGameEvents(ActiveContext.ActiveGraph.FindNode(positionedId), isLeave); }

        private void ExecuteGameEvents(INode node, bool isLeave)
        {
            if (fullMode)
            {
                foreach (var gameEvent in node.GameEvents)
                {
                    if (gameEvent.IsExecutedOnLeave == isLeave)
                    {
                        gameEvent.Execute();
                    }
                }
            }
        }

        protected bool SwitchToGraph(string graphId)
        {
            ICollection_Base collectionBase = (ICollection_Base)ActiveContext.ActiveTab;
            return collectionBase.AddToSelectionById(graphId, true);
        }

        private void StartPlayNode(INode node, bool noAudioMode)
        {
            bool isSkipped = false;

            foreach (var behavior in ((IWithModel)node).GetModel<Node_BaseM>().behaviors)
            {
                if (BehaviorHelper.IsTrue(behavior))
                {
                    isSkipped = true;
                    break;
                }
            }

            if (isSkipped)
            {
                FinishPlayNode(node, false);
            }
            else
            {
                ExecuteGameEvents(node, false); // Enter

                if (node is IRegularNode regularNode)
                {
                    byte storageType = regularNode.FileStorageType;
                    string fileUrl = regularNode.FileHttpRef;

                    if (noAudioMode || string.IsNullOrEmpty(fileUrl) || storageType == STORAGE_TYPE.UNSET)
                    {
                        double startTimeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        double finishTimeMsec = startTimeMsec + Duration * 1000;

                        TimeLeft = Duration;

                        ActiveContext.TaskService.Start(
                            Duration * 1000,
                            (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec) =>
                            {
                                ActiveContext.ActiveGraph.TickPlayer(inDeltaTimeMsec);

                                TimeLeft -= inDeltaTimeMsec / 1000;

                                return CustomStatus.Running;
                            },
                            (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec, customStatus) =>
                            {
                                if (customStatus == CustomStatus.RanToCompletion)
                                {
                                    ActiveContext.ActiveGraph.TickPlayer(inDeltaTimeMsec);

                                    TimeLeft = 0;

                                    FinishPlayNode(node, true);
                                }

                                return customStatus;
                            }
                            , null);
                    }
                    else
                    {
                        TimeLeft = 0;
                        IsDownloading = true;

                        ActiveContext.FileService.GetFileFromStorage(
                            node.Id
                            , storageType
                            , fileUrl
                            , (sourceFilePath) =>
                            {
                                IsDownloading = false;

                                ActiveContext.SoundPlayerService.Play(
                                    sourceFilePath
                                    , () =>
                                    {
                                        ActiveContext.TaskService.Start(
                                            (double)TaskMode.DrivenByStatus
                                            , (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec) =>
                                            {
                                                ActiveContext.ActiveGraph.TickPlayer(inDeltaTimeMsec);

                                                return CustomStatus.Running;
                                            }
                                            , (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec, customStatus) => customStatus
                                            , null
                                            );
                                    }
                                    , () =>
                                    {
                                        ActiveContext.TaskService.Stop();
                                    }
                                    , (customStatus) =>
                                    {
                                        if (customStatus == CustomStatus.RanToCompletion)
                                        {
                                            FinishPlayNode(node, true);
                                        }
                                        else if (customStatus == CustomStatus.Faulted)
                                        {
                                            StartPlayNode(node, true);
                                        }
                                    }
                                );
                            }
                            , () =>
                            {
                                IsDownloading = false;

                                StartPlayNode(node, true);
                            });
                    }
                }
                else if (node is Node_DelayVM delayNode)
                {
                    double startTimeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                    double finishTimeMsec = startTimeMsec + delayNode.Delay;

                    TimeLeft = delayNode.Delay;

                    ActiveContext.TaskService.Start(
                        delayNode.Delay * 1000,
                        (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec) =>
                        {
                            ActiveContext.ActiveGraph.TickPlayer(inDeltaTimeMsec);

                            TimeLeft -= inDeltaTimeMsec / 1000;
                            return CustomStatus.Running;
                        },
                        (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec, customStatus) =>
                        {
                            if (customStatus == CustomStatus.RanToCompletion)
                            {
                                ActiveContext.ActiveGraph.TickPlayer(inDeltaTimeMsec);

                                TimeLeft = 0;
                                FinishPlayNode(node, true);
                            }

                            return customStatus;
                        }
                        , null);
                }
                else
                {
                    FinishPlayNode(node, true);
                }
            }
        }

        private void FinishPlayNode(INode node, bool executeGameEvents)
        {
            if (executeGameEvents) ExecuteGameEvents(node, true); // Leave

            if (node is Node_GateVM gateNode)
            {
                PlayerContext = null;

                bool hasTargetAndExit = gateNode.TargetDialog != null && gateNode.TargetExitNode != null;
                bool needToSwitch = hasTargetAndExit && ActiveContext.ActiveGraph.Id != StartGraph.Id;
                bool hasSwitched = SwitchToGraph(gateNode.TargetDialog.id);

                if (hasTargetAndExit && needToSwitch && hasSwitched || hasTargetAndExit && !needToSwitch)
                {
                    INode startNode = ActiveContext.ActiveGraph.FindNode(gateNode.TargetExitNode.id) ?? ActiveContext.ActiveGraph.GenerateNode(gateNode.TargetExitNode.id);
                    OnStartMovement(startNode);
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                var allPaths = ActiveContext.ActiveGraph.GetAllPaths(node.Id);

                Paths = GetAvailablePaths(allPaths);

                if (Paths.Count == 1)
                {
                    PathIndex = 0;
                }
                else if (Paths.Count > 0)
                {
                    if (node is Node_RandomVM randomNode)
                    {
                        PathIndex = RandomHelper.Next(Paths.Count);
                    }
                    else if (Paths.All((path) => path.Last().CharacterId == CharacterM.PLAYER_ID))
                    {
                        Dictionary<INode, int> choices = new Dictionary<INode, int>();

                        for (int i = 0; i < Paths.Count; i++)
                        {
                            choices.Add(ActiveContext.ActiveGraph.GenerateNode(Paths[i].Last().Id), i);
                        }

                        PlayerContext = new PlayerContext_ChoiceVM(this, choices);
                    }
                    else
                    {
                        PlayerContext = new PlayerContext_ErrorVM(this, null);
                    }
                }
                else
                {
                    Stop();
                }
            }
        }

        private List<List<IPositioned>> GetAvailablePaths(List<List<IPositioned>> paths)
        {
            List<List<IPositioned>> pathsToRemove = new List<List<IPositioned>>();

            foreach (var path in paths)
            {
                if (path.Count > 0)
                {
                    foreach (var positioned in path)
                    {
                        INode node = ActiveContext.ActiveGraph.GenerateNode(positioned.Id);

                        bool shouldBeRemoved = false;

                        if (node.Gender != Gender && node.Gender > 0) // Filtered by Gender
                        {
                            pathsToRemove.Add(path);
                            shouldBeRemoved = true;
                        }

                        if (fullMode)
                        {
                            Node_InteractiveM interactiveNodeModel = ((IWithModel)node).GetModel<Node_InteractiveM>();
                            if (interactiveNodeModel != null)
                            {
                                foreach (var predicateModel in interactiveNodeModel.predicates) // Filtered by predicates
                                {
                                    if (!PredicatesHelper.IsTrue(predicateModel))
                                    {
                                        pathsToRemove.Add(path);
                                        shouldBeRemoved = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (shouldBeRemoved) break;
                    }
                }
                else
                {
                    pathsToRemove.Add(path);
                }
            }

            foreach (var pathToRemove in pathsToRemove)
            {
                paths.Remove(pathToRemove);
            }

            Dictionary<string, List<IPositioned>> shortestPaths = new Dictionary<string, List<IPositioned>>();

            foreach (var path in paths)
            {
                string targetId = path.Last().Id;

                if (!shortestPaths.ContainsKey(targetId))
                {
                    shortestPaths.Add(targetId, null);
                }

                if (shortestPaths[targetId] == null || shortestPaths[targetId].Count > path.Count)
                {
                    shortestPaths[targetId] = path;
                }
            }

            return shortestPaths.Values.ToList();
        }

        private void MoveThroughPath()
        {
            PlayerContext = new PlayerContext_TransitionVM();

            if (Paths[PathIndex].Count > 0)
            {
                IPositioned next = Paths[PathIndex].First();
                ActiveContext.ActiveGraph.MoveTo(next, (customStatus) => { if (customStatus == CustomStatus.RanToCompletion) OnFinishMovement(next); }, PlayRate);
            }
        }

        protected ICommand pauseCommand;
        public ICommand PauseCommand => pauseCommand ?? (pauseCommand = new RelayCommand(() => { }, () => PlayerContext != null));

        protected ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(() => { Stop(); }, () => PlayerContext != null));

        private void Stop()
        {
            ActiveContext.SoundPlayerService?.Stop();
            ActiveContext.TaskService?.Stop();

            PathIndex = int.MinValue;
            Paths?.Clear();

            PlayerContext = null;
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

        public void Clear()
        { 
        
        }

        public IGraph StartGraph { get; set; }
        public INode StartNode { get; set; }
        public string ActiveDialogEntryId { get; set; }

        List<List<IPositioned>> Paths { get; set; }

        private int _pathIndex;
        public int PathIndex
        {
            get => _pathIndex;
            set
            {
                _pathIndex = value;
                if (_pathIndex >= 0)
                {
                    MoveThroughPath();
                }
            }
        }

        public override string Id => null;
    }
}