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
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Windows;
using System.ComponentModel;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class TreeVm : NonFolderVm, ICopyPaste, ITagged
    {
        public event Action<string> OnSetBackground = delegate { };

        public event Action<Node_BaseVm> OnFoundRoot = delegate { };

        public event Action<Node_BaseVm> OnNodeAdded = delegate { };
        public event Action<Node_BaseVm> OnNodeRemoved = delegate { };

        public event Action<NodePairVm> OnLinkAdded = delegate { };
        public event Action<NodePairVm> OnLinkRemoved = delegate { };

        public event Action OnRootNodesChanged = delegate { };

        public event Action<Node_BaseVm> OnNodePositionChanged = delegate { };

        public void NodePositionChanged(Node_BaseVm node) { OnNodePositionChanged(node); }

        public TreeVm(BaseVm<FullContextVm> Parent) : base(Parent)
        {
            Nodes = new ObservableCollection<Node_BaseVm>();
            Links = new ObservableCollection<NodePairVm>();
            Selection = new List<Node_BaseVm>();

            RootNodeIds = new List<string>();
            Participants = new List<string>();

            globalTagIds = new ObservableCollection<string>();
            globalTagIds.CollectionChanged += OnCollectionChanged;

            FullContextVm.OnSearchFilterChangedEvent += OnSearchFilterChanged;
        }

        public TreeVm() : this(null) { }

        ~TreeVm()
        {
            FullContextVm.OnSearchFilterChangedEvent -= OnSearchFilterChanged;
            
            globalTagIds.CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Notify(nameof(Tags));

        public string Stats
        {
            get
            {
                string result = "";
                result += id + Environment.NewLine;
                result += Environment.NewLine;
                result += "Вершины: " + Nodes.Count + Environment.NewLine;

                Dictionary<string, int> countByCharacter = new Dictionary<string, int>();
                Dictionary<string, int> countByTypeDescription = new Dictionary<string, int>();

                int namedCount = 0;

                foreach (var node in Nodes)
                {
                    if (node is IOwnered owneredNode)
                    {
                        namedCount++;
                        var character = owneredNode.Owner.Name;
                        if (!countByCharacter.ContainsKey(character)) countByCharacter.Add(character, 0);
                        countByCharacter[character]++;
                    }

                    var typeDescription = TypeToDescriptionConverter.GetTypeDescription(node.GetType());
                    if (!countByTypeDescription.ContainsKey(typeDescription)) countByTypeDescription.Add(typeDescription, 0);
                    countByTypeDescription[typeDescription]++;
                }

                countByCharacter.Add("N/A", Nodes.Count - namedCount);

                foreach (var entry in countByCharacter) result += "- " + entry.Key + ": " + entry.Value + Environment.NewLine;
                result += Environment.NewLine;
                foreach (var entry in countByTypeDescription) result += "- " + entry.Key + ": " + entry.Value + Environment.NewLine;

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
                    if (Parent is BaseTabVm<BaseNamedVm<BaseVm<FullContextVm>>, FullContextVm> tabParent)
                    {
                        CollectionViewSource.GetDefaultView(tabParent.Items).Refresh();
                    }

                    foreach (var node in Nodes)
                    {
                        if (node is DNode_CharacterVm dNode_Character)
                        {
                            dNode_Character.ParticipantStates.Add(new ParticipantStateVm(dNode_Character, character));
                        }
                    }
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
                    if (Parent is BaseTabVm<BaseNamedVm<BaseVm<FullContextVm>>, FullContextVm> tabParent)
                    {
                        CollectionViewSource.GetDefaultView(tabParent.Items).Refresh();
                    }

                    foreach (var node in Nodes)
                    {
                        if (node is DNode_CharacterVm dNode_Character)
                        {
                            dNode_Character.RemoveParticipantState(character);
                        }
                    }
                }
            }
        }

        public void AddRootNode(Node_BaseVm node)
        {
            if (node is DNode_VirtualVm) return;

            RootNodeIds.Add(node.Id);
            OnRootNodesChanged.Invoke();

            Notify(nameof(RootNodesInfo));
            Notify(nameof(RootNodeToView));
            Notify(nameof(RootNodes));
        }

        public void RemoveRootNode(Node_BaseVm node)
        {
            if (node is DNode_VirtualVm) return;

            RootNodeIds.Remove(node.Id);
            OnRootNodesChanged.Invoke();

            if (rootNodeIndex >= RootNodeIds.Count) rootNodeIndex--;

            Notify(nameof(RootNodesInfo));
            Notify(nameof(RootNodeToView));
            Notify(nameof(RootNodes));
        }

        int rootNodeIndex = -1;

        public string RootNodesInfo => rootNodeIndex >= 0 ? string.Format("{0}/{1}", rootNodeIndex + 1, RootNodeIds.Count) : "";


        [XmlIgnore]
        public Node_BaseVm RootNodeToView
        {
            get => Nodes.FirstOrDefault((node) => node.Id == (rootNodeIndex >= 0 && RootNodeIds.Count > rootNodeIndex ? RootNodeIds[rootNodeIndex] : null));
            set
            {
                if (RootNodeIds.Contains(value?.Id) && RootNodeIds.IndexOf(value?.Id) != rootNodeIndex)
                {
                    rootNodeIndex = RootNodeIds.IndexOf(value?.Id);
                    OnFoundRoot(value);
                    Notify(nameof(RootNodesInfo));
                    Notify(nameof(RootNodeToView));
                }
            }
        }

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

                ////// Clear root infos ////// TODO Remove after migration all quests to new format
                rootNodeIndex = -1;
                Notify(nameof(RootNodesInfo));
                Notify(nameof(RootNodeToView));
            }
        }

        protected void RemoveFromSelection(Node_BaseVm node)
        {
            if (Selection.Contains(node))
            {
                Selection.Remove(node);
                node.NotifyIsSelectedChanged();

                Notify(nameof(Selected));

                ////// Clear root infos ////// TODO Remove after migration all quests to new format
                rootNodeIndex = -1;
                Notify(nameof(RootNodesInfo));
                Notify(nameof(RootNodeToView));
            }
        }

        [XmlIgnore]
        public Node_BaseVm Selected => Selection.Count == 1 ? Selection[0] : null;

        ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand
        {
            get
            {
                return toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand<Node_BaseVm>((node) =>
                {
                    node.ToggleGender();
                }, (node) => node != null));
            }
        }

        ICommand removeCommand;
        public ICommand RemoveCommand
        {
            get
            {
                return removeCommand ?? (removeCommand = new RelayCommand<object>((argument) =>
                {
                    if (argument is NodePairVm link) RemoveLink_Internal(link);

                    if (argument is Node_BaseVm node) RemoveNode_Internal(node);

                }, (argument) => argument != null));
            }
        }

        ICommand findRootCommand;
        public ICommand FindRootCommand => findRootCommand ?? (findRootCommand = new RelayCommand(() =>
        {
            rootNodeIndex = rootNodeIndex >= 0 ? rootNodeIndex : 0;
            OnFoundRoot(Nodes.FirstOrDefault((node) => node.Id == RootNodeIds[rootNodeIndex]));
            Notify(nameof(RootNodesInfo));
            Notify(nameof(RootNodeToView));
        }, () => RootNodeIds.Count > 0));

        ICommand prevRootCommand;
        public ICommand PrevRootCommand => prevRootCommand ?? (prevRootCommand = new RelayCommand(() =>
        {
            rootNodeIndex = ((rootNodeIndex >= 0 ? rootNodeIndex - 1 : rootNodeIndex) + RootNodeIds.Count) % RootNodeIds.Count;
            OnFoundRoot(Nodes.FirstOrDefault((node) => node.Id == RootNodeIds[rootNodeIndex]));
            Notify(nameof(RootNodesInfo));
            Notify(nameof(RootNodeToView));
        }, () => RootNodeIds.Count > 0));

        ICommand nextRootCommand;
        public ICommand NextRootCommand => nextRootCommand ?? (nextRootCommand = new RelayCommand(() =>
        {
            rootNodeIndex = (rootNodeIndex + 1) % RootNodeIds.Count;
            OnFoundRoot(Nodes.FirstOrDefault((node) => node.Id == RootNodeIds[rootNodeIndex]));
            Notify(nameof(RootNodesInfo));
            Notify(nameof(RootNodeToView));
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
            OnLinkRemoved(link);

            if (Links.All(remainingLink => remainingLink.ToId != link.ToId))
            {
                AddRootNode(Nodes.FirstOrDefault((node) => node.Id == link.ToId));
            }

            NotifyIsValidChanged();
        }

        private void RemoveNode_Internal(Node_BaseVm node)
        {
            RemoveFromSelection(node);

            List<NodePairVm> brokenLinks = new List<NodePairVm>();
            foreach (var link in Links) { if (link.FromId == node.Id || link.ToId == node.Id) brokenLinks.Add(link); }
            foreach (var link in brokenLinks) { RemoveLink_Internal(link); }

            Nodes.Remove(node);
            OnNodeRemoved(node);

            RemoveRootNode(node);

            if (node is IOwnered ownered) RemoveParticipant(ownered.Owner);

            NotifyIsValidChanged();
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
                        if (characterNode.ParticipantStates.All(state => state.CharacterId != characterId)) characterNode.ParticipantStates.Add(new ParticipantStateVm(characterNode, character));
                    }
                }

                AddParticipant(ownered.Owner);
            }
            AddToSelection(node, true);
            NotifyIsValidChanged();
        }

        public bool CanLink(Node_BaseVm from, Node_BaseVm to)
        {
            if (from is DNode_VirtualVm || to is DNode_VirtualVm) return false;

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
                            to.Gender == Node_BaseVm.UNISEX ||
                            link.To.Gender == Node_BaseVm.UNISEX)
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

        public bool IsLeafNode(Node_BaseVm node) => Links.All(link => link?.FromId != node.Id);

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is TreeVm casted)
            {
                ////// TODO MORE COMPLEX
            }
        }

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

        [XmlArray]
        public ObservableCollection<string> globalTagIds { get; set; }



        [XmlIgnore]
        public List<JournalTagVm> Tags => Parent?.Parent?.GlobalTagsTab.Items.Where((tag) => globalTagIds.Contains(tag.Id)).ToList();

        protected CollectionViewSource tagsToAdd;
        [XmlIgnore]
        public CollectionViewSource TagsToAdd
        {
            get
            {
                if (Parent != null && tagsToAdd == null)
                {
                    tagsToAdd = new CollectionViewSource() { Source = Parent.Parent.GlobalTagsTab.Items };
                    tagsToAdd.View.Filter = (object obj) => obj is JournalTagVm tag && !globalTagIds.Contains(tag.Id);
                    tagsToAdd.View.MoveCurrentTo(null);
                }

                return tagsToAdd;
            }
        }

        [XmlIgnore]
        public JournalTagVm TagToAdd
        {
            get => null;
            set
            {
                if (value != null && !globalTagIds.Contains(value.Id))
                {
                    globalTagIds.Add(value.Id);
                    TagsToAdd.View.Refresh();
                }
            }
        }

        ICommand removeTagCommand;
        public ICommand RemoveTagCommand => removeTagCommand ?? (removeTagCommand = new RelayCommand<JournalTagVm>(tag => RemoveTag(tag.Id), tag => tag != null));

        public void RemoveTag(string tagId) { globalTagIds.Remove(tagId); TagsToAdd.View.Refresh(); }



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

            TreeVm copiedTree = new TreeVm(Parent);

            Dictionary<string, string> nodesMapping = new Dictionary<string, string>();

            foreach (var node in Selection)
            {
                var copiedNode = node.Clone<Node_BaseVm>(copiedTree);
                copiedNode.PositionX += App.Offset.X;
                copiedNode.PositionY += App.Offset.Y;
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
        }

        public void Paste()
        {
            string xmlString = Clipboard.GetText();
            if (!string.IsNullOrEmpty(xmlString))
            {
                TreeVm copiedTree = App.DeserializeXmlFromString<TreeVm>(xmlString);

                Dictionary<string, string> nodesMapping = new Dictionary<string, string>();

                foreach (var node in copiedTree.Nodes)
                {
                    var copiedNode = node.Clone<Node_BaseVm>(this);
                    copiedNode.PositionX -= App.Offset.X;
                    copiedNode.PositionY -= App.Offset.Y;
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
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Nodes.Any(node => node.PassFilter(filter)) || 
            Links.Any(link => link.PassFilter(filter));
    }
}