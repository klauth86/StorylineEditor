/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.CopyPasteService;
using StorylineEditor.Common;
using StorylineEditor.FileDialog;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Tabs;
using StorylineEditor.Views.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Windows;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class TreeVm : NonFolderVm, ICopyPaste
    {
        public event Action PauseUnpauseEvent = delegate { };
        public void OnPauseUnpause() { PauseUnpauseEvent(); }

        public event Action<Node_BaseVm> StartTransitionEvent = delegate { };
        public void OnStartTransition(Node_BaseVm nextNode) { StartTransitionEvent(nextNode); }

        public event Action<object> EndTransitionEvent = delegate { };
        public void OnEndTransition(object nodeObj) { EndTransitionEvent(nodeObj); }

        public event Action<Node_BaseVm, double> StartActiveNodeEvent = delegate { };
        public void OnStartActiveNode(Node_BaseVm node, double activeTime) { StartActiveNodeEvent(node, activeTime); }

        public event Action<object> EndActiveNodeEvent = delegate { };
        public void OnEndActiveNode(object nodeObj) { EndActiveNodeEvent(nodeObj); }

        public event Action StopEvent = delegate { };
        public void OnStop() { StopEvent(); }

        public event Action<double> DurationAlphaChangedEvent = delegate { };
        public void OnDurationAlphaChanged(double alpha) { DurationAlphaChangedEvent(alpha); }


        protected bool isPlaying;
        [XmlIgnore]
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    NotifyWithCallerPropName();
                }
            }
        }


        public event Action<string> OnSetBackground = delegate { };
        public event Action<Node_BaseVm> OnFoundRoot = delegate { };
        public event Action<Node_BaseVm> OnNodeAdded = delegate { };
        public event Action<Node_BaseVm> OnNodeRemoved = delegate { };
        public event Action<Node_BaseVm> OnNodeCopied = delegate { };
        public event Action<Node_BaseVm> OnNodePasted = delegate { };

        public event Action<NodePairVm> OnLinkAdded = delegate { };
        public event Action<NodePairVm> OnLinkRemoved = delegate { };

        public event Action OnRootNodesChanged = delegate { };

        public event Action<Node_BaseVm> OnNodePositionChanged = delegate { };

        public void NodePositionChanged(Node_BaseVm node) { OnNodePositionChanged(node); }

        public TreeVm(BaseVm<FullContextVm> Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            Nodes = new ObservableCollection<Node_BaseVm>();
            Links = new ObservableCollection<NodePairVm>();
            Selection = new List<Node_BaseVm>();

            RootNodeIds = new List<string>();
            Participants = new List<string>();

            FullContextVm.OnSearchFilterChangedEvent += OnSearchFilterChanged;
        }

        public TreeVm() : this(null, 0) { }

        public string Stats
        {
            get
            {
                string result = "";
                result += string.Format("ID: {0}", id) + Environment.NewLine;
                result += string.Format("NAME: {0}", Name) + Environment.NewLine;

                Dictionary<string, Dictionary<string, int>> countByCharacter = new Dictionary<string, Dictionary<string, int>>();
                Dictionary<string, int> countByTypeDescription = new Dictionary<string, int>();

                int characterNameMaxLength = 0;

                foreach (var node in Nodes)
                {
                    string characterName = "N/A";

                    if (node is IOwnered owneredNode)
                    {
                        characterName = owneredNode.Owner?.Name;
                        characterNameMaxLength = Math.Max(characterName?.Length ?? 0, characterNameMaxLength);
                    }

                    string gender = " ";
                    if (node.Gender == Node_BaseVm.MALE) gender = "👨";
                    if (node.Gender == Node_BaseVm.FEMALE) gender = "👩";

                    if (!countByCharacter.ContainsKey(characterName))
                    {
                        countByCharacter.Add(characterName, new Dictionary<string, int>() { { " ", 0 }, { "👨", 0 }, { "👩", 0 } });
                    }

                    countByCharacter[characterName][gender]++;

                    var typeDescription = TypeToDescriptionConverter.GetTypeDescription(node.GetType());
                    if (!countByTypeDescription.ContainsKey(typeDescription)) countByTypeDescription.Add(typeDescription, 0);
                    countByTypeDescription[typeDescription]++;
                }

                if (countByCharacter.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ПЕРСОНАЖАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByCharacter.OrderBy(pair => pair.Key))
                    {
                        result += string.Format("{0, -" + (characterNameMaxLength + 6) + "}{1}", "- " + entry.Key + ":", string.Join("\t", entry.Value.Select(pair => string.Format("{0}{1, -6}", pair.Key, pair.Value)))) + Environment.NewLine;
                    }
                }

                if (countByTypeDescription.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ТИПАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByTypeDescription.OrderBy(pair => pair.Key)) result += "- " + entry.Key + ": " + entry.Value + Environment.NewLine;
                }

                // Delimiter
                result += Environment.NewLine;
                result += Environment.NewLine;

                result += "ОШИБКИ:" + Environment.NewLine;
                result += Environment.NewLine;

                if (RootNodeIds.Count == 0)
                {
                    result += string.Format("- ⚠ В дереве отсутствует корневая вершина. У дерева должна быть только ОДНА корневая вершина!") + Environment.NewLine;
                }
                else if (RootNodeIds.Count > 1)
                {
                    result += string.Format("- ⚠ В дереве несколько корневых вершин. У дерева должна быть только ОДНА корневая вершина!") + Environment.NewLine;
                }

                return result;
            }
        }
        public override bool IsValid => base.IsValid && Nodes.All(node => node?.IsValid ?? false) &&
            Links.All(link => link != null && Nodes.Any((node) => node.Id == link.FromId) &&
            Nodes.Any((node) => node.Id == link.ToId) && link.FromId != link.ToId);

        public ObservableCollection<Node_BaseVm> Nodes { get; set; }

        public ObservableCollection<NodePairVm> Links { get; set; }

        public List<string> RootNodeIds { get; set; }

        [XmlIgnore]
        public List<Node_BaseVm> RootNodes => Nodes.Where((node) => RootNodeIds.Contains(node.Id)).ToList();

        public bool NodeFilter(object obj)
        {
            if (obj is DNode_CharacterVm ||
                obj is DNode_DialogVm ||
                obj is JNode_AlternativeVm ||
                obj is JNode_StepVm)
            {
                return true;           
            }

            return false;
        }

        public List<string> Participants { get; set; }

        public void AddParticipant(FolderedVm character)
        {
            if (character != null)
            {
                if (!Participants.Contains(character.Id))
                {
                    Participants.Add(character.Id);

                    ////// TODO Think if we should refresh view
                    if (Parent is FolderedTabVm tabParent)
                    {
                        CollectionViewSource.GetDefaultView(tabParent.Items).Refresh();
                    }

                    foreach (var node in Nodes)
                    {
                        if (node is DNode_CharacterVm dNode_Character)
                        {
                            ////// TODO Here were Participant States
                        }
                    }

                    Notify(nameof(Stats));
                }
            }
        }

        public void RemoveParticipant(FolderedVm character)
        {
            if (character != null && Participants.Contains(character.Id))
            {
                if (Nodes.All((node) => ((node as IOwnered)?.Owner ?? null) != character))
                {
                    Participants.Remove(character.Id);

                    ////// TODO Think if we should refresh view
                    if (Parent is FolderedTabVm tabParent)
                    {
                        CollectionViewSource.GetDefaultView(tabParent.Items).Refresh();
                    }

                    foreach (var node in Nodes)
                    {
                        if (node is DNode_CharacterVm dNode_Character)
                        {
                            ////// TODO Here were Participant States
                        }
                    }
                }

                Notify(nameof(Stats));
            }
        }

        public void AddRootNode(Node_BaseVm node)
        {
            RootNodeIds.Add(node.Id);
            OnRootNodesChanged.Invoke();

            Notify(nameof(RootNodes));
            Notify(nameof(Stats));
        }

        public void RemoveRootNode(Node_BaseVm node)
        {
            RootNodeIds.Remove(node.Id);
            OnRootNodesChanged.Invoke();

            if (rootNodeIndex >= RootNodeIds.Count) rootNodeIndex--;

            Notify(nameof(RootNodes));
            Notify(nameof(Stats));
        }

        int rootNodeIndex = -1;


        [XmlIgnore]
        public readonly List<Node_BaseVm> Selection;

        public void AddToSelection(Node_BaseVm node, bool withClean)
        {
            if (withClean)
            {
                List<Node_BaseVm> removedFromSelection = new List<Node_BaseVm>(Selection);

                Selection.Clear();

                foreach (var removed in removedFromSelection) removed.NotifyIsSelectedChanged();
            }

            if (!Selection.Contains(node))
            {
                Selection.Add(node);
                node.NotifyIsSelectedChanged();

                Notify(nameof(Selected));
            }
        }

        protected void RemoveFromSelection(Node_BaseVm node)
        {
            if (Selection.Contains(node))
            {
                Selection.Remove(node);
                node.NotifyIsSelectedChanged();

                Notify(nameof(Selected));
            }
        }

        [XmlIgnore]
        public Node_BaseVm Selected => Selection.Count == 1 ? Selection[0] : null;

        ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand => toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand<Node_BaseVm>((node) => 
        { 
            node.ToggleGender();
            Notify(nameof(Stats));
        }, (node) => node != null));

        ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand<object>((argument) =>
        {
            if (argument is NodePairVm link) RemoveLink_Internal(link);
            if (argument is Node_BaseVm node) RemoveNode_Internal(node);
        }, (argument) => argument != null));

        ICommand prevRootCommand;
        public ICommand PrevRootCommand => prevRootCommand ?? (prevRootCommand = new RelayCommand(() =>
        {
            rootNodeIndex = (rootNodeIndex - 1 + 2 * RootNodeIds.Count) % RootNodeIds.Count;
            Node_BaseVm rootNode = Nodes.FirstOrDefault((node) => node.Id == RootNodeIds[rootNodeIndex]);
            if (rootNode != null)
            {
                AddToSelection(rootNode, true);
                OnFoundRoot(rootNode);
            }
        }, () => RootNodeIds.Count > 0));

        ICommand nextRootCommand;
        public ICommand NextRootCommand => nextRootCommand ?? (nextRootCommand = new RelayCommand(() =>
        {
            rootNodeIndex = (rootNodeIndex + 1 + 2 * RootNodeIds.Count) % RootNodeIds.Count;
            Node_BaseVm rootNode = Nodes.FirstOrDefault((node) => node.Id == RootNodeIds[rootNodeIndex]);
            if (rootNode != null)
            {
                AddToSelection(rootNode, true);
                OnFoundRoot(rootNode);
            }
        }, () => RootNodeIds.Count > 0));

        const string imageFilter = "Image files (*.png;*.jpg;*.jpeg;*.tiff;*.bmp)|*.png;*.jpg;*.jpeg;*.tiff;*.bmp";

        ICommand setBackgroundCommand;
        public ICommand SetBackgroundCommand => setBackgroundCommand ?? (setBackgroundCommand = new RelayCommand(() =>
        {
            string path = IDialogService.DialogService.OpenFileDialog(imageFilter, false);
            if (!string.IsNullOrEmpty(path)) OnSetBackground(path);
        }));

        private void RemoveLink_Internal(NodePairVm link)
        {
            Links.Remove(link);
            link.OnRemoval();

            OnLinkRemoved(link);

            if (Links.All(remainingLink => remainingLink.ToId != link.ToId))
            {
                AddRootNode(Nodes.FirstOrDefault((node) => node.Id == link.ToId));
            }

            NotifyIsValidChanged();
            Notify(nameof(Stats));
        }

        private void RemoveNode_Internal(Node_BaseVm node)
        {
            RemoveFromSelection(node);

            List<NodePairVm> brokenLinks = new List<NodePairVm>();
            foreach (var link in Links) { if (link.FromId == node.Id || link.ToId == node.Id) brokenLinks.Add(link); }
            foreach (var link in brokenLinks) { RemoveLink_Internal(link); }

            Nodes.Remove(node);
            node.OnRemoval();

            OnNodeRemoved(node);

            RemoveRootNode(node);

            if (node is IOwnered ownered) RemoveParticipant(ownered.Owner);

            NotifyIsValidChanged();
            Notify(nameof(Stats));
        }

        public void AddNode(Node_BaseVm node) { if (node != null) AddNode_Internal(node); }

        private void AddNode_Internal(Node_BaseVm node)
        {
            Nodes.Add(node);
            OnNodeAdded(node);

            AddRootNode(node);

            if (node is IOwnered ownered)
            {
                if (ownered is DNode_CharacterVm characterNode)
                {
                    foreach (var characterId in Participants)
                    {
                        var character = Parent.Parent.CharactersTab.Items.FirstOrDefault((participant) => participant.Id == characterId);
                        ////// TODO Here were Participant States
                    }
                }

                AddParticipant(ownered.Owner);
            }
            AddToSelection(node, true);
            NotifyIsValidChanged();
            Notify(nameof(Stats));
        }

        public bool CanLink(Node_BaseVm from, Node_BaseVm to)
        {
            if (from == to) return false;

            if (from is JNode_AlternativeVm && !(to is JNode_StepVm)) return false;

            if (from is JNode_StepVm || from is JNode_AlternativeVm)
            {
                if (to.ChildNodes.Contains(from)) return false;
            }

            var parentNodes = GetPrimaryParentNodes(to);

            if (parentNodes.Contains(from)) return false;

            return true;
        }

        public void AddLink(Node_BaseVm from, Node_BaseVm to)
        {
            // Break existing links
            if (!from.AllowsManyChildren)
            {
                List<NodePairVm> brokenLinks = new List<NodePairVm>();
                foreach (var link in Links) { if (link.FromId == from.Id) brokenLinks.Add(link); }
                foreach (var link in brokenLinks) { RemoveLink_Internal(link); }
            }

            RefreshJournalLinks_From(from, to);
            
            var newLink = new NodePairVm() { FromId = from?.Id, ToId = to?.Id, Parent = this };
            Links.Add(newLink);
            OnLinkAdded(newLink);

            RemoveRootNode(to);

            NotifyIsValidChanged();
        }

        public void RefreshJournalLinks_To(Node_BaseVm to)
        {
            List<NodePairVm> links = new List<NodePairVm>(Links);
            foreach (var link in links) if (link.ToId == to.Id) RefreshJournalLinks_From(link.From, link.To);
        }

        /// <summary>
        /// Break some links depending on node Type (Step and Alternative) and node Gender
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void RefreshJournalLinks_From(Node_BaseVm from, Node_BaseVm to)
        {
            // If we add new link from Step node to Step node
            // - all existing links to Alternative nodes will be broken (node Type rule)
            // - all existing Step nodes of intersecting gender will be broken (node Gender rule)

            if (to is JNode_StepVm)
            {
                List<NodePairVm> brokenLinks = new List<NodePairVm>();
                foreach (var link in Links)
                {
                    if (link.FromId == from.Id)
                    {
                        // if it is called from AddLink, we have no such link
                        // if it is called from RefreshJournalLinks_To, we prefer last changed node to all others
                        if (link.ToId == to.Id) continue;

                        if (link.To is JNode_AlternativeVm ||
                            to.Gender == link.To.Gender ||
                            to.Gender == 0 ||
                            link.To.Gender == 0)
                            brokenLinks.Add(link);
                    }
                }
                foreach (var link in brokenLinks) { RemoveLink_Internal(link); }
            }

            // If we add new link from Step node to Alternative node:
            // - all existing links to Step nodes will be broken (node Type rule)

            if (from is JNode_StepVm && to is JNode_AlternativeVm)
            {
                List<NodePairVm> brokenLinks = new List<NodePairVm>();
                foreach (var link in Links) { if (link.FromId == from.Id && Nodes.First(node => node.Id == link.ToId) is JNode_StepVm) brokenLinks.Add(link); }
                foreach (var link in brokenLinks) { RemoveLink_Internal(link); }
            }
        }

        public List<Node_BaseVm> GetPrimaryParentNodes(Node_BaseVm node)
        {
            var nodeIds = Links.Where(link => link.ToId == node.Id).Select(link => link.FromId).ToList();
            return Nodes.Where((otherNode) => nodeIds.Contains(otherNode.Id)).ToList();
        }

        public List<Node_BaseVm> GetPrimaryChildNodes(Node_BaseVm node)
        {
            var nodeIds = Links.Where(link => link.FromId == node.Id).Select(link => link.ToId).ToList();
            return Nodes.Where((otherNode) => nodeIds.Contains(otherNode.Id)).ToList();
        }

        public List<Node_BaseVm> GetChildNodes(Node_BaseVm node)
        {
            var nodeIds = Links.Where(link => link.FromId == node.Id).Select(link => link.ToId).ToList();
            
            List<Node_BaseVm> nonTransitNodes = Nodes.Where((otherNode) => nodeIds.Contains(otherNode.Id) && !(otherNode is DNode_TransitVm)).ToList();
            
            foreach(var transitChildNodes in 
                Nodes.Where((otherNode) => nodeIds.Contains(otherNode.Id) && (otherNode is DNode_TransitVm)).Select((otherNode) => GetChildNodes(otherNode)))
            {
                nonTransitNodes.AddRange(transitChildNodes);
            }

            return nonTransitNodes;
        }

        public bool IsLeafNode(Node_BaseVm node) => Links.All(link => link?.FromId != node.Id);

        public List<Node_BaseVm> NodesTraversal() => NodesTraversal(Nodes.Where((node) => RootNodeIds.Contains(node.Id)).ToList());

        public List<Node_BaseVm> NodesTraversal(Node_BaseVm startNode, bool includeSelf) { var result = NodesTraversal(GetPrimaryChildNodes(startNode)); if (includeSelf) result.Insert(0, startNode); return result; }

        public List<Node_BaseVm> NodesTraversal(List<Node_BaseVm> layerNodes)
        {
            List<Node_BaseVm> processedNodes = new List<Node_BaseVm>();

            while (layerNodes.Count > 0)
            {
                processedNodes.AddRange(layerNodes);
                layerNodes.Clear();

                foreach (var processedNode in processedNodes)
                {
                    foreach (var childNode in GetPrimaryChildNodes(processedNode))
                    {
                        if (!processedNodes.Contains(childNode) && !layerNodes.Contains(childNode)) layerNodes.Add(childNode);
                    }
                }
            }

            return processedNodes;
        }

        public List<Node_BaseVm> GetRootNodes(Node_BaseVm node)
        {
            List<Node_BaseVm> layerNodes = new List<Node_BaseVm>() { node };

            if (node.IsRoot) return layerNodes;

            List<Node_BaseVm> result = new List<Node_BaseVm>();

            List<Node_BaseVm> processedNodes = new List<Node_BaseVm>();

            while (layerNodes.Count > 0)
            {

                processedNodes.AddRange(layerNodes);
                layerNodes.Clear();

                foreach (var processedNode in processedNodes)
                {
                    foreach (var parentNode in GetPrimaryParentNodes(processedNode))
                    {
                        if (!layerNodes.Contains(parentNode) && !processedNodes.Contains(parentNode))
                        {
                            layerNodes.Add(parentNode);
                            if (parentNode.IsRoot) result.Add(parentNode);
                        }
                    }
                }
            }

            return result;
        }

        public string NameOwnered
        {
            get
            {
                var characters = Parent.Parent.CharactersTab.Items;
                return (Participants.Count == 1 ? characters.FirstOrDefault((character) => character.Id == Participants[0])?.Name + ": " : "") + Name;
            }
        }

        public override void SetupParenthood()
        {
            foreach (var node in Nodes)
            {
                node.Parent = this;
                node.SetupParenthood();
            }

            foreach (var link in Links)
            {
                link.Parent = this;
            }
        }



        [XmlIgnore]
        public override bool IsSelected
        {
            get => base.IsSelected;
            set
            {
                if (value != isSelected)
                {
                    base.IsSelected = value;

                    if (value)
                    {
                        ICopyPasteService.Context = this;
                    }
                }
            }
        }

        public void Copy()
        {
            // Only need to copy nodes and links
            // Other stuff will be generated in Paste

            TreeVm copiedTree = new TreeVm(Parent, 0);

            Dictionary<string, string> nodesMapping = new Dictionary<string, string>();

            long counter = 0;

            foreach (var node in Selection)
            {
                var copiedNode = node.Clone<Node_BaseVm>(copiedTree, counter++);
                OnNodeCopied(copiedNode);
                copiedTree.Nodes.Add(copiedNode);

                nodesMapping.Add(node.Id, copiedNode.Id);

                System.Diagnostics.Trace.WriteLine(string.Format("{0}|{1}", node.Id, copiedNode.Id));
            }

            foreach (var link in Links)
            {
                if (nodesMapping.ContainsKey(link.FromId) && nodesMapping.ContainsKey(link.ToId))
                {
                    copiedTree.Links.Add(new NodePairVm() { FromId = nodesMapping[link.FromId], ToId = nodesMapping[link.ToId] });

                    System.Diagnostics.Trace.WriteLine(string.Format("{0}|{1} => {2}|{3}", link.FromId, link.ToId, nodesMapping[link.FromId], nodesMapping[link.ToId]));
                }
            }

            Clipboard.SetText(App.SerializeXmlToString<TreeVm>(copiedTree));

            copiedTree.OnRemoval();
        }

        public void Paste()
        {
            string xmlString = Clipboard.GetText();
            if (!string.IsNullOrEmpty(xmlString))
            {
                TreeVm copiedTree = App.DeserializeXmlFromString<TreeVm>(xmlString);

                Dictionary<string, string> nodesMapping = new Dictionary<string, string>();

                long counter = 0;

                foreach (var node in copiedTree.Nodes)
                {
                    var copiedNode = node.Clone<Node_BaseVm>(this, counter++);
                    OnNodePasted(copiedNode);
                    AddNode(copiedNode);

                    nodesMapping.Add(node.Id, copiedNode.Id);

                    copiedNode.SetupParenthood();
                }

                foreach (var link in copiedTree.Links)
                {
                    if (nodesMapping.ContainsKey(link.FromId) && nodesMapping.ContainsKey(link.ToId))
                    {
                        AddLink(
                        Nodes.First(node => node.Id == nodesMapping[link.FromId]),
                        Nodes.First(node => node.Id == nodesMapping[link.ToId])
                        );
                    }
                }

                foreach (var nodeId in nodesMapping.Values)
                {
                    AddToSelection(Nodes.First(node => node.Id == nodeId), false);
                }

                copiedTree.OnRemoval();
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Nodes.Any(node => node.PassFilter(filter)) || 
            Links.Any(link => link.PassFilter(filter));

        public override bool OnRemoval()
        {
            foreach (var link in Links) link.OnRemoval();

            foreach (var node in Nodes) node.OnRemoval();

            FullContextVm.OnSearchFilterChangedEvent -= OnSearchFilterChanged;

            return base.OnRemoval(); 
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is TreeVm casted)
            {
                ////// TODO MORE COMPLEX
            }
        }
    }
}