/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using StorylineEditor.Common;
using StorylineEditor.CopyPasteService;
using StorylineEditor.FileDialog;
using StorylineEditor.ViewModels;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Predicates;
using StorylineEditor.ViewModels.Tabs;

namespace StorylineEditor.Views.Controls
{
    public class TreeCanvasProxy { public TreeCanvas TreeCanvas { get; set; } }

    public class TreeCanvas : Canvas, ICopyPaste
    {
        const double TransitionDuration = 0.75;

        #region TreeVm DP

        public Vector Snapping
        {
            get => (Vector)this.GetValue(SnappingProperty);
            set { this.SetValue(SnappingProperty, value); }
        }

        public static readonly DependencyProperty SnappingProperty = DependencyProperty.Register(
            "Snapping", typeof(Vector), typeof(TreeCanvas));

        public TreeVm Tree
        {
            get => (TreeVm)this.GetValue(TreeProperty);
            set { this.SetValue(TreeProperty, value); }
        }

        public static readonly DependencyProperty TreeProperty = DependencyProperty.Register(
            "Tree", typeof(TreeVm), typeof(TreeCanvas), new PropertyMetadata(null, OnTreeChanged));

        public Node_BaseVm SelectedValue
        {
            get => (Node_BaseVm)this.GetValue(SelectedValueProperty);
            set { this.SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            "SelectedValue", typeof(Node_BaseVm), typeof(TreeCanvas), new PropertyMetadata(null));

        public double Scale
        {
            get => (double)this.GetValue(ScaleProperty);
            set { this.SetValue(ScaleProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(TreeCanvas), new PropertyMetadata(1.0));

        public double AnimTranslationX
        {
            get => (double)this.GetValue(AnimTranslationXProperty);
            set { this.SetValue(AnimTranslationXProperty, value); }
        }

        public static readonly DependencyProperty AnimTranslationXProperty = DependencyProperty.Register(
            "AnimTranslationX", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0, OnAnimTranslationChanged));

        public double AnimTranslationY
        {
            get => (double)this.GetValue(AnimTranslationYProperty);
            set { this.SetValue(AnimTranslationYProperty, value); }
        }

        public static readonly DependencyProperty AnimTranslationYProperty = DependencyProperty.Register(
            "AnimTranslationY", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0, OnAnimTranslationChanged));

        public double TranslationX
        {
            get => (double)this.GetValue(TranslationXProperty);
            set { this.SetValue(TranslationXProperty, value); }
        }

        public static readonly DependencyProperty TranslationXProperty = DependencyProperty.Register(
            "TranslationX", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0));

        public double TranslationY
        {
            get => (double)this.GetValue(TranslationYProperty);
            set { this.SetValue(TranslationYProperty, value); }
        }

        public static readonly DependencyProperty TranslationYProperty = DependencyProperty.Register(
            "TranslationY", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0));

        public int StateDuration
        {
            get => (int)this.GetValue(StateDurationProperty);
            set { this.SetValue(StateDurationProperty, value); }
        }

        public static readonly DependencyProperty StateDurationProperty = DependencyProperty.Register(
            "StateDuration", typeof(int), typeof(TreeCanvas), new PropertyMetadata(4));

        public double StateTimeLeft
        {
            get => (double)this.GetValue(StateTimeLeftProperty);
            set { this.SetValue(StateTimeLeftProperty, value); }
        }

        public static readonly DependencyProperty StateTimeLeftProperty = DependencyProperty.Register(
            "StateTimeLeft", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0));

        public BaseVm ActiveContext
        {
            get => (BaseVm)this.GetValue(ActiveContextProperty);
            set { this.SetValue(ActiveContextProperty, value); }
        }

        public static readonly DependencyProperty ActiveContextProperty = DependencyProperty.Register(
            "ActiveContext", typeof(BaseVm), typeof(TreeCanvas), new PropertyMetadata(null));

        public int GenderToPlay
        {
            get => (int)this.GetValue(GenderToPlayProperty);
            set { this.SetValue(GenderToPlayProperty, value); }
        }

        public static readonly DependencyProperty GenderToPlayProperty = DependencyProperty.Register(
            "GenderToPlay", typeof(int), typeof(TreeCanvas), new PropertyMetadata(1));

        public bool IsPlaying
        {
            get => (bool)this.GetValue(IsPlayingProperty);
            set { this.SetValue(IsPlayingProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            "IsPlaying", typeof(bool), typeof(TreeCanvas), new PropertyMetadata(false));

        public bool FullMode
        {
            get => (bool)this.GetValue(FullModeProperty);
            set { this.SetValue(FullModeProperty, value); }
        }

        public static readonly DependencyProperty FullModeProperty = DependencyProperty.Register(
            "FullMode", typeof(bool), typeof(TreeCanvas), new PropertyMetadata(false));

        private static void OnTreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeCanvas treeCanvas)
            {
                if (e.OldValue is TreeVm oldTree)
                {
                    oldTree.OnNodeRemoved -= treeCanvas.OnNodeRemoved;
                    oldTree.OnLinkRemoved -= treeCanvas.OnLinkRemoved;
                    oldTree.OnNodePositionChanged -= treeCanvas.OnNodePositionChanged;

                    foreach (var link in oldTree.Links) treeCanvas.OnLinkRemoved(link);
                    foreach (var item in oldTree.Nodes) treeCanvas.OnNodeRemoved(item);
                }

                treeCanvas.Reset();

                if (e.NewValue is TreeVm newTree)
                {
                    foreach (var node in newTree.Nodes) treeCanvas.AddGraphNode(node);
                    foreach (var link in newTree.Links) treeCanvas.AddGraphLink(link);

                    newTree.OnNodeRemoved += treeCanvas.OnNodeRemoved;
                    newTree.OnLinkRemoved += treeCanvas.OnLinkRemoved;
                    newTree.OnNodePositionChanged += treeCanvas.OnNodePositionChanged;
                }
            }
        }

        private static void OnAnimTranslationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeCanvas treeCanvas)
            {
                if (treeCanvas.Storyboard.GetCurrentState(treeCanvas) != ClockState.Stopped)
                {
                    treeCanvas.TranslationX = treeCanvas.AnimTranslationX - treeCanvas.ActualWidth / 2 / treeCanvas.Scale;
                    treeCanvas.TranslationY = treeCanvas.AnimTranslationY - treeCanvas.ActualHeight / 2 / treeCanvas.Scale;

                    if (treeCanvas.StateTimeLeft > 0)
                    {
                        treeCanvas.StateTimeLeft = treeCanvas.StateDuration * (1.0 - treeCanvas.Storyboard.GetCurrentProgress(treeCanvas) ?? 1);
                    }

                    if (treeCanvas.PlayingAdorner != null)
                    {
                        treeCanvas.PlayingAdorner.PositionX = treeCanvas.AnimTranslationX - treeCanvas.PlayingAdorner.Width / 2;
                        treeCanvas.PlayingAdorner.PositionY = treeCanvas.AnimTranslationY - treeCanvas.PlayingAdorner.Height / 2;
                    }

                    treeCanvas.OnTransformChanged();
                }
            }
        }

        #endregion

        private void OnTransformChanged()
        {
            Rect canvasRect = new Rect(
                TranslationX,
                TranslationY,
                ActualWidth / Scale,
                ActualHeight / Scale);

            foreach (var graphNodesEntry in GraphNodes)
            {
                Node_BaseVm node = graphNodesEntry.Key;
                GraphNode graphNode = graphNodesEntry.Value;

                RefreshPosition(graphNode, (node.PositionX - TranslationX) * Scale, (node.PositionY - TranslationY) * Scale);

                Rect graphNodeRect = new Rect(
                    node.PositionX,
                    node.PositionY,
                    graphNode.ActualWidth,
                    graphNode.ActualHeight);

                Rect canvasRectCopy = canvasRect;
                canvasRectCopy.Intersect(graphNodeRect);

                graphNode.Visibility = canvasRectCopy.IsEmpty ? Visibility.Hidden : Visibility.Visible;

                (graphNode.RenderTransform as ScaleTransform).ScaleX = Scale;
                (graphNode.RenderTransform as ScaleTransform).ScaleY = Scale;
            }

            UpdateLinksLayout(null);

            if (PlayingAdorner != null)
            {
                RefreshPosition(PlayingAdorner, (PlayingAdorner.PositionX - TranslationX) * Scale, (PlayingAdorner.PositionY - TranslationY) * Scale);

                (PlayingAdorner.RenderTransform as ScaleTransform).ScaleX = Scale;
                (PlayingAdorner.RenderTransform as ScaleTransform).ScaleY = Scale;
            }
        }

        private void RefreshPosition(FrameworkElement frameworkElement, double positionX, double positionY)
        {
            Canvas.SetLeft(frameworkElement, positionX);
            Canvas.SetTop(frameworkElement, positionY);
        }

        private void OnFoundRoot(Node_BaseVm node)
        {
            if (node != null && GraphNodes.ContainsKey(node))
            {
                StopStoryboard();

                double duration = 0.25;

                var xAnimation = new DoubleAnimation
                {
                    From = TranslationX + ActualWidth / 2 / Scale,
                    To = node.PositionX + GraphNodes[node].ActualWidth / 2,
                    Duration = TimeSpan.FromSeconds(duration),
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTargetProperty(xAnimation, new PropertyPath("AnimTranslationX"));

                var yAnimation = new DoubleAnimation
                {
                    From = TranslationY + ActualHeight / 2 / Scale,
                    To = node.PositionY + GraphNodes[node].ActualHeight / 2,
                    Duration = TimeSpan.FromSeconds(duration),
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTargetProperty(yAnimation, new PropertyPath("AnimTranslationY"));

                Storyboard.Children.Add(xAnimation);
                Storyboard.Children.Add(yAnimation);

                Storyboard.Begin(this, true);

                AddToSelection(node, true);
            }
        }



        public void Copy()
        {
            // Only need to copy nodes and links
            // Other stuff will be generated in Paste

            if (Tree != null)
            {
                TreeVm copiedTree = new TreeVm(Tree.Parent, 0);

                Dictionary<string, string> nodesMapping = new Dictionary<string, string>();

                long counter = 0;

                foreach (var node in Selection)
                {
                    var copiedNode = node.Clone<Node_BaseVm>(copiedTree, counter++);

                    copiedNode.PositionX = (copiedNode.PositionX - TranslationX) * Scale;
                    copiedNode.PositionY = (copiedNode.PositionY - TranslationY) * Scale;

                    copiedTree.Nodes.Add(copiedNode);

                    nodesMapping.Add(node.Id, copiedNode.Id);

                    System.Diagnostics.Trace.WriteLine(string.Format("{0}|{1}", node.Id, copiedNode.Id));
                }

                foreach (var link in Tree.Links)
                {
                    if (nodesMapping.ContainsKey(link.FromId) && nodesMapping.ContainsKey(link.ToId))
                    {
                        copiedTree.Links.Add(new NodePairVm() { FromId = nodesMapping[link.FromId], ToId = nodesMapping[link.ToId] });

                        System.Diagnostics.Trace.WriteLine(string.Format("{0}|{1} => {2}|{3}", link.FromId, link.ToId, nodesMapping[link.FromId], nodesMapping[link.ToId]));
                    }
                }

                Clipboard.SetText(App.SerializeXmlToString<TreeVm>(copiedTree));
            }
        }

        public void Paste()
        {
            string xmlString = Clipboard.GetText();
            if (!string.IsNullOrEmpty(xmlString))
            {
                try
                {
                    TreeVm copiedTree = App.DeserializeXmlFromString<TreeVm>(xmlString);

                    Dictionary<string, string> nodesMapping = new Dictionary<string, string>();

                    long counter = 0;

                    foreach (var node in copiedTree.Nodes)
                    {
                        var copiedNode = node.Clone<Node_BaseVm>(Tree, counter++);

                        copiedNode.PositionX = copiedNode.PositionX / Scale + TranslationX;
                        copiedNode.PositionY = copiedNode.PositionY / Scale + TranslationY;

                        Tree.AddNode(copiedNode);
                        AddGraphNode(copiedNode);

                        nodesMapping.Add(node.Id, copiedNode.Id);

                        copiedNode.SetupParenthood();
                    }

                    foreach (var link in copiedTree.Links)
                    {
                        if (nodesMapping.ContainsKey(link.FromId) && nodesMapping.ContainsKey(link.ToId))
                        {
                            NodePairVm copiedLink = Tree.AddLink(
                            Tree.Nodes.First(node => node.Id == nodesMapping[link.FromId]),
                            Tree.Nodes.First(node => node.Id == nodesMapping[link.ToId])
                            );
                            AddGraphLink(copiedLink);
                        }
                    }

                    foreach (var nodeId in nodesMapping.Values)
                    {
                        AddToSelection(Tree.Nodes.First(node => node.Id == nodeId), false);
                    }
                }
                catch (Exception)
                {

                }
            }
        }



        private void StartState(Node_BaseVm node, double duration)
        {
            if (node != null)
            {
                ActiveContext = node;

                if (PlayingAdorner == null)
                {
                    PlayingAdorner = new PlayingAdorner();
                    PlayingAdorner.RenderTransform = new ScaleTransform() { ScaleX = Scale, ScaleY = Scale };

                    Canvas.SetZIndex(PlayingAdorner, -ActiveZIndex);

                    PlayingAdorner.PositionX = node.PositionX + GraphNodes[node].ActualWidth / 2 - PlayingAdorner.Width / 2;
                    PlayingAdorner.PositionY = node.PositionY + GraphNodes[node].ActualHeight / 2 - PlayingAdorner.Height / 2;

                    RefreshPosition(PlayingAdorner, (PlayingAdorner.PositionX - TranslationX) * Scale, (PlayingAdorner.PositionY - TranslationY) * Scale);

                    Children.Add(PlayingAdorner);
                }

                PlayingAdorner.ToStateView(GraphNodes[node], TransitionDuration / 4);

                StopStoryboard();

                var xAnimation = new DoubleAnimation
                {
                    From = TranslationX + ActualWidth / 2 / Scale,
                    To = node.PositionX + GraphNodes[node].ActualWidth / 2,
                    Duration = TimeSpan.FromSeconds(duration),
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTargetProperty(xAnimation, new PropertyPath("AnimTranslationX"));

                var yAnimation = new DoubleAnimation
                {
                    From = TranslationY + ActualHeight / 2 / Scale,
                    To = node.PositionY + GraphNodes[node].ActualHeight / 2,
                    Duration = TimeSpan.FromSeconds(duration),
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTargetProperty(yAnimation, new PropertyPath("AnimTranslationY"));

                Storyboard.Children.Add(xAnimation);
                Storyboard.Children.Add(yAnimation);

                Storyboard.Completed += OnCompletedState;

                StateTimeLeft = duration;
                nextNode = node;

                Storyboard.Begin(this, true);
            }
            else
            {
                Stop();
            }
        }

        public void StartTransition(Node_BaseVm node)
        {
            if (node != null)
            {
                ActiveContext = new TreePlayerContext_TransitionVm();

                PlayingAdorner?.ToTransitionView(TransitionDuration / 4);

                StopStoryboard();

                var xAnimation = new DoubleAnimation
                {
                    From = TranslationX + ActualWidth / 2 / Scale,
                    To = node.PositionX + GraphNodes[node].ActualWidth / 2,
                    Duration = TimeSpan.FromSeconds(TransitionDuration),
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTargetProperty(xAnimation, new PropertyPath("AnimTranslationX"));

                var yAnimation = new DoubleAnimation
                {
                    From = TranslationY + ActualHeight / 2 / Scale,
                    To = node.PositionY + GraphNodes[node].ActualHeight / 2,
                    Duration = TimeSpan.FromSeconds(TransitionDuration),
                    FillBehavior = FillBehavior.HoldEnd
                };

                Storyboard.SetTargetProperty(yAnimation, new PropertyPath("AnimTranslationY"));

                Storyboard.Children.Add(xAnimation);
                Storyboard.Children.Add(yAnimation);

                Storyboard.Completed += OnCompletedTransition;

                StateTimeLeft = 0;
                nextNode = node;

                Storyboard.Begin(this, true);
            }
            else
            {
                Stop();
            }
        }

        private void GoToNextStep(Node_BaseVm node)
        {
            if (node != null)
            {
                List<Node_BaseVm> childNodes = Tree.GetChildNodes(node);

                childNodes.RemoveAll((childNode) => childNode.Gender > 0 && childNode.Gender != GenderToPlay);

                List<Node_BaseVm> nodesToRemove = new List<Node_BaseVm>();

                foreach (var childNode in childNodes)
                {
                    if (childNode is Node_InteractiveVm interactiveNode)
                    {
                        foreach (var predicate in interactiveNode.Predicates)
                        {
                            if (!predicate.IsOk) { nodesToRemove.Add(childNode); }
                        }
                    }
                }

                childNodes.RemoveAll((childNode) => nodesToRemove.Contains(childNode));

                if (childNodes.Count == 1)
                {
                    StartTransition(childNodes[0]);
                }
                else if (childNodes.Count > 0)
                {
                    if (node is DNode_RandomVm randomNode)
                    {
                        StartTransition(childNodes[Random.Next(childNodes.Count)]);
                    }
                    else if (childNodes.TrueForAll((childNode) => (childNode is IOwnered owneredNode) && owneredNode.Owner != null && owneredNode.Owner.Id == CharacterVm.PlayerId))
                    {
                        ActiveContext = new TreePlayerContext_ChoiceVm(this, childNodes);
                    }
                    else
                    {
                        string description = "Дочерние вершины не подходят ни под одну из ситуаций:" + Environment.NewLine;
                        description += Environment.NewLine;

                        description += "- " + "После Случайной вершины (⇝) возможен любой состав дочерних вершин..." + Environment.NewLine;
                        description += Environment.NewLine;

                        description += "- " + "Если НЕ Случайная вершина (💬, ⇴) имеет одну актуальную (удовлетворяющую полу и своим предикатам) дочернюю вершину, то этой вершиной может быть любая вершина кроме вершины Транзит (⇴) с несколькими актуальными (удовлетворяющие полу и своим предикатам) дочерними вершинами..." + Environment.NewLine;
                        description += Environment.NewLine;

                        description += "- " + "Если НЕ Случайная вершина (💬, ⇴) имеет более одной актуальной (удовлетворяющей полу и своим предикатам) дочерней вершины, то эти вершины должны быть либо вершинами Основного персонажа (💬), либо Транзитом (⇴) на вершины Основного персонажа (💬) (ситуация ВЫБОР ИГРОКА)..." + Environment.NewLine;
                        description += Environment.NewLine;

                        ActiveContext = new TreePlayerContext_ErrorVm() { Description = description };
                    }
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                Stop();
            }
        }

        private void Stop()
        {
            StopStoryboard();

            if (PlayingAdorner != null)
            {
                Children.Remove(PlayingAdorner);
                PlayingAdorner = null;
            }

            IsPlaying = false;

            ActiveContext = null;
        }

        private void PauseUnpause()
        {
            if (Storyboard.GetIsPaused(this))
            {
                Storyboard.Resume(this);
                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
                Storyboard.Pause(this);
            }
        }

        private void OnCompletedTransition(object sender, EventArgs e) { StartState(nextNode, StateDuration); }

        private void OnCompletedState(object sender, EventArgs e) { GoToNextStep(nextNode); }

        void StopStoryboard()
        {
            Storyboard.Completed -= OnCompletedTransition;
            Storyboard.Completed -= OnCompletedState;

            Storyboard.Stop(this);

            Storyboard.Children.Clear();
        }

        private void AddGraphNode(Node_BaseVm node)
        {
            if (node != null)
            {
                GraphNode graphNode = new GraphNode();
                RefreshPosition(graphNode, (node.PositionX - TranslationX) * Scale, (node.PositionY - TranslationY) * Scale);
                graphNode.RenderTransform = new ScaleTransform(Scale, Scale);

                graphNode.DataContext = node;
                graphNode.SizeChanged += OnNodeSizeChanged;

                Canvas.SetZIndex(graphNode, ActiveZIndex);
                Children.Add(graphNode);

                GraphNodes.Add(node, graphNode);
            }
        }

        private void OnNodeSizeChanged(object sender, SizeChangedEventArgs e) { UpdateLinksLayout(sender as GraphNode); }

        private void OnNodeRemoved(Node_BaseVm node)
        {
            RemoveFromSelection(node);

            if (node != null && GraphNodes.ContainsKey(node))
            {
                GraphNodes[node].SizeChanged -= OnNodeSizeChanged;
                GraphNodes[node].DataContext = null;

                Children.Remove(GraphNodes[node]);

                GraphNodes.Remove(node);
            }
        }



        private void AddGraphLink(NodePairVm link)
        {
            if (link != null)
            {
                var fromPair = GraphNodes.FirstOrDefault((gnodePair) => gnodePair.Key.Id == link.FromId);
                var fromVertice = fromPair.Value;

                var toPair = GraphNodes.FirstOrDefault((gnodePair) => gnodePair.Key.Id == link.ToId);
                var toVertice = toPair.Value;

                if (fromVertice != null && toVertice != null)
                {
                    var newGraphLink = fromVertice.DataContext is JNode_StepVm
                        ? new SequenceLink(fromVertice, toVertice)
                        : new GraphLink(fromVertice, toVertice);

                    GraphLinks.Add(link, newGraphLink);

                    newGraphLink.LineAndArrow.DataContext = link;
                    newGraphLink.LinkContent.DataContext = link;

                    Children.Add(newGraphLink.LineAndArrow);

                    Canvas.SetZIndex(newGraphLink.LinkContent, ActiveZIndex + 1);
                    Children.Add(newGraphLink.LinkContent);

                    // Update layout after Loaded
                    Dispatcher.BeginInvoke(new Action(() => { newGraphLink.UpdateLayout(); }), DispatcherPriority.Loaded);
                }
            }
        }

        private void OnLinkRemoved(NodePairVm link)
        {
            if (link != null && GraphLinks.ContainsKey(link))
            {
                GraphLinks[link].LinkContent.DataContext = null;
                GraphLinks[link].LineAndArrow.DataContext = null;

                Children.Remove(GraphLinks[link].LineAndArrow);
                Children.Remove(GraphLinks[link].LinkContent);

                GraphLinks.Remove(link);
            }
        }



        private void OnNodePositionChanged(Node_BaseVm node)
        {
            if (node != null && GraphNodes.ContainsKey(node))
            {
                RefreshPosition(GraphNodes[node], (node.PositionX - TranslationX) * Scale, (node.PositionY - TranslationY) * Scale);
                UpdateLinksLayout(GraphNodes[node]);
            }
        }



        private void UpdateLinksLayout(GraphNode graphNode)
        {
            if (graphNode != null)
            {
                foreach (var graphLink in GraphLinks.Values.Where(link => link.From == graphNode || link.To == graphNode))
                {
                    graphLink.UpdateLayout();
                }
            }
            else
            {
                foreach (var graphLink in GraphLinks.Values)
                {
                    graphLink.UpdateLayout();
                }
            }
        }



        public TreeCanvas() : base()
        {
            Focusable = true;

            Selection = new ObservableCollection<Node_BaseVm>();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (ICopyPasteService.Context != this)
            {
                ICopyPasteService.Context = this;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (ICopyPasteService.Context == this)
            {
                ICopyPasteService.Context = null;
            }

            base.OnMouseLeave(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e) { MainWindow.FacadeKeyEvent += OnFacadeKeyEvent; }

        private void OnUnloaded(object sender, RoutedEventArgs e) { Stop(); MainWindow.FacadeKeyEvent -= OnFacadeKeyEvent; }

        private void OnFacadeKeyEvent(bool isDown, Key key)
        {
            shiftMode = isDown && (key == Key.LeftShift || key == Key.RightShift);
            ctrlMode = isDown && (key == Key.LeftCtrl || key == Key.RightCtrl);
        }

        Node_BaseVm nextNode = null;

        Storyboard Storyboard = new Storyboard();
        Random Random = new Random();
        protected Dictionary<Node_BaseVm, GraphNode> GraphNodes = new Dictionary<Node_BaseVm, GraphNode>();
        protected Dictionary<NodePairVm, GraphLink> GraphLinks = new Dictionary<NodePairVm, GraphLink>();

        protected int rootNodeIndex = -1;
        public ObservableCollection<Node_BaseVm> Selection { get; }

        protected bool shiftMode = false;
        protected bool ctrlMode = false;

        protected bool linkMode = false;
        protected bool dragMode = false;
        protected bool dragAllMode = false;
        protected Point prevMousePosition = new Point();
        protected GraphNode ActiveGraphNode;
        protected Rectangle SelectionRectangle;

        protected PlayingAdorner PlayingAdorner;

        protected IndicatorLink indicatorLink;
        public IndicatorLink IndicatorLink
        {
            get => indicatorLink;
            set
            {
                if (value != indicatorLink)
                {
                    if (indicatorLink != null)
                    {
                        Children.Remove(indicatorLink.LineAndArrow);
                        Children.Remove(indicatorLink.InfoImage);
                    }

                    indicatorLink = value;

                    if (indicatorLink != null)
                    {
                        Canvas.SetZIndex(indicatorLink.LineAndArrow, ActiveZIndex + 10);
                        Children.Add(indicatorLink.LineAndArrow);

                        Canvas.SetZIndex(indicatorLink.InfoImage, ActiveZIndex + 10);
                        Children.Add(indicatorLink.InfoImage);
                    }
                }
            }
        }

        const int ActiveZIndex = 20;



        protected void Reset()
        {
            Stop();

            rootNodeIndex = -1;
            Selection.Clear();

            shiftMode = false;
            ctrlMode = false;
            linkMode = false;
            dragMode = false;
            dragAllMode = false;
            prevMousePosition.X = 0;
            prevMousePosition.Y = 0;
            ActiveGraphNode = null;

            IndicatorLink = null;

            Children.Clear();

            Scale = 1;

            TranslationX = 0;
            TranslationY = 0;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            double oldScale = Scale;

            Scale = Math.Max(Math.Min(Scale + e.Delta * 0.0002, 4), 1.0 / 64);

            Point mousePosition = e.GetPosition(this);

            Point oldScaleMousePosition = new Point(mousePosition.X / oldScale, mousePosition.Y / oldScale);

            Point newScaleMousePosition = new Point(mousePosition.X / Scale, mousePosition.Y / Scale);

            TranslationX += oldScaleMousePosition.X - newScaleMousePosition.X;

            TranslationY += oldScaleMousePosition.Y - newScaleMousePosition.Y;

            OnTransformChanged();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Focus();

            bool isLockedByTreePlayer = false;
            // bool isLockedByTreePlayer = (TreePlayerVm.TreePlayerInstance?.IsLocked ?? false) || ((TreePlayerVm.TreePlayerInstance?.ActiveContext ?? null) != null);

            if (Tree != null)
            {
                if (e.LeftButton == MouseButtonState.Released &&
                    e.RightButton == MouseButtonState.Pressed)
                {
                    if (e.Source is GraphNode graphNode && !isLockedByTreePlayer)
                    {
                        if (shiftMode)
                        {
                            if (graphNode.DataContext is Node_BaseVm node)
                            {
                                var newNode = node.Clone<Node_BaseVm>(node.Parent, 0);
                                Tree.AddNode(newNode);
                                AddGraphNode(newNode);

                                AddToSelection(newNode, true);

                                GraphNodes[newNode].RelativePosition = graphNode.RelativePosition;
                                ActiveGraphNode = GraphNodes[newNode];
                                dragMode = true;
                                e.Handled = true;
                            }
                        }
                        else if (ctrlMode)
                        {
                            ActiveGraphNode = graphNode;
                            prevMousePosition = e.GetPosition(this);
                            dragMode = true;

                            e.Handled = true;
                        }
                        else
                        {
                            ActiveGraphNode = e.Source as GraphNode;
                            linkMode = true;
                            IndicatorLink = new IndicatorLink(ActiveGraphNode);

                            e.Handled = true;
                        }
                    }
                    else if (ctrlMode)
                    {
                        prevMousePosition = e.GetPosition(this);
                        dragAllMode = true;

                        e.Handled = true;
                    }
                }
                else if (e.LeftButton == MouseButtonState.Pressed &&
                    e.RightButton == MouseButtonState.Released)
                {
                    if (e.OriginalSource is Canvas && !isLockedByTreePlayer)
                    {
                        if (shiftMode)
                        {
                            prevMousePosition = e.GetPosition(this);

                            SelectionRectangle = new Rectangle
                            {
                                StrokeDashArray = new DoubleCollection() { 4.0, 4.0 },
                                Stroke = Brushes.DarkBlue,
                                StrokeThickness = 1
                            };

                            Canvas.SetLeft(SelectionRectangle, prevMousePosition.X);
                            Canvas.SetTop(SelectionRectangle, prevMousePosition.Y);

                            SelectionRectangle.Width = 0;
                            SelectionRectangle.Height = 0;

                            Canvas.SetZIndex(SelectionRectangle, ActiveZIndex + 10);
                            Children.Add(SelectionRectangle);

                            foreach (var child in Children) (child as UIElement).IsHitTestVisible = false;

                            e.Handled = true;
                        }
                        else
                        {
                            Point mousePosition = e.GetPosition(this);
                            var absoluteMousePositionX = mousePosition.X / Scale + TranslationX;
                            var absoluteMousePositionY = mousePosition.Y / Scale + TranslationY;

                            if (Snapping.X * Snapping.Y >= 1)
                            {
                                absoluteMousePositionX = Math.Round(absoluteMousePositionX / Snapping.X) * Snapping.X;
                                absoluteMousePositionY = Math.Round(absoluteMousePositionY / Snapping.Y) * Snapping.Y;
                            }

                            var tab = Tree.Parent as BaseTreesTabVm;
                            var newNode = tab.CreateNode(Tree);
                            if (newNode != null)
                            {
                                newNode.PositionX = absoluteMousePositionX;
                                newNode.PositionY = absoluteMousePositionY;
                                Tree.AddNode(newNode);
                                AddGraphNode(newNode);

                                AddToSelection(newNode, true);
                            }
                            e.Handled = true;
                        }
                    }
                    else if (e.Source is GraphNode)
                    {
                        if (e.Source is GraphNode graphNode)
                        {
                            var node = graphNode.DataContext as Node_BaseVm;
                            AddToSelection(node, !shiftMode);
                        }
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (Tree != null)
            {
                if (e.LeftButton == MouseButtonState.Released &&
                    e.RightButton == MouseButtonState.Released)
                {
                    if (dragMode)
                    {
                        ActiveGraphNode = null;
                        dragMode = false;
                    }

                    if (linkMode)
                    {
                        if (e.Source is GraphNode graphNodeTo && IndicatorLink != null && indicatorLink.CanLink == true)
                        {
                            var link = Tree.AddLink(ActiveGraphNode.DataContext as Node_BaseVm, graphNodeTo.DataContext as Node_BaseVm);
                            AddGraphLink(link);
                        }

                        IndicatorLink = null;
                        linkMode = false;
                        ActiveGraphNode = null;
                    }

                    dragAllMode = false;

                    if (SelectionRectangle != null)
                    {
                        Rect selectionRect = new Rect(
                            Canvas.GetLeft(SelectionRectangle) / Scale + TranslationX,
                            Canvas.GetTop(SelectionRectangle) / Scale + TranslationY,
                            SelectionRectangle.ActualWidth / Scale,
                            SelectionRectangle.ActualHeight / Scale);

                        Queue<Node_BaseVm> nodesToSelect = new Queue<Node_BaseVm>();

                        foreach (var graphNodesEntry in GraphNodes)
                        {
                            Node_BaseVm node = graphNodesEntry.Key;
                            GraphNode graphNode = graphNodesEntry.Value;

                            Rect graphNodeRect = new Rect(
                                node.PositionX,
                                node.PositionY,
                                graphNode.ActualWidth * Scale,
                                graphNode.ActualHeight * Scale);

                            Rect selectionRectCopy = selectionRect;
                            selectionRectCopy.Intersect(graphNodeRect);

                            if (!selectionRectCopy.IsEmpty) nodesToSelect.Enqueue(graphNodesEntry.Key);
                        }

                        bool isFirst = true;

                        while (nodesToSelect.Count > 0)
                        {
                            var node = nodesToSelect.Dequeue();
                            AddToSelection(node, isFirst);
                            isFirst = false;
                        }

                        Children.Remove(SelectionRectangle);
                        SelectionRectangle = null;

                        foreach (var child in Children) (child as UIElement).IsHitTestVisible = true;
                    }
                }
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Tree != null)
            {
                if (dragMode)
                {
                    StopStoryboard();

                    var node = (ActiveGraphNode?.DataContext as Node_BaseVm);
                    if (node != null)
                    {
                        var mousePosition = e.GetPosition(this);

                        double absolutePositionX = TranslationX + mousePosition.X / Scale - ActiveGraphNode.RelativePosition.X;
                        double absolutePositionY = TranslationY + mousePosition.Y / Scale - ActiveGraphNode.RelativePosition.Y;

                        if (Snapping.X * Snapping.Y >= 1)
                        {
                            absolutePositionX = Math.Round(absolutePositionX / Snapping.X) * Snapping.X;
                            absolutePositionY = Math.Round(absolutePositionY / Snapping.Y) * Snapping.Y;
                        }

                        if (absolutePositionX != 0 || absolutePositionY != 0)
                        {
                            if (Selection.Contains(node))
                            {
                                foreach (var selected in Selection)
                                {
                                    if (selected == node) continue;

                                    selected.PositionX = absolutePositionX + selected.PositionX - node.PositionX;
                                    selected.PositionY = absolutePositionY + selected.PositionY - node.PositionY;
                                }
                            }

                            node.PositionX = absolutePositionX;
                            node.PositionY = absolutePositionY;
                        }
                    }
                }
                else if (dragAllMode)
                {
                    StopStoryboard();

                    var currentPosition = e.GetPosition(this);

                    TranslationX -= (currentPosition.X - prevMousePosition.X) / Scale;
                    TranslationY -= (currentPosition.Y - prevMousePosition.Y) / Scale;

                    prevMousePosition = currentPosition;

                    OnTransformChanged();
                }
                else if (SelectionRectangle != null)
                {
                    StopStoryboard();

                    var currentPosition = (Vector)e.GetPosition(this);

                    Canvas.SetLeft(SelectionRectangle, Math.Min(prevMousePosition.X, currentPosition.X));
                    Canvas.SetTop(SelectionRectangle, Math.Min(prevMousePosition.Y, currentPosition.Y));

                    double width = prevMousePosition.X - currentPosition.X;
                    if (width < 0) width = -width;
                    double height = prevMousePosition.Y - currentPosition.Y;
                    if (height < 0) height = -height;

                    SelectionRectangle.Width = width;
                    SelectionRectangle.Height = height;

                    e.Handled = true;
                }
                else if (linkMode)
                {
                    StopStoryboard();

                    if (e.Source is GraphNode graphNodeTo)
                    {
                        IndicatorLink.CanLink = Tree.CanLink(ActiveGraphNode.DataContext as Node_BaseVm, graphNodeTo.DataContext as Node_BaseVm);
                        IndicatorLink.UpdateLayout(graphNodeTo);
                    }
                    else
                    {
                        IndicatorLink.CanLink = null;
                        IndicatorLink.UpdateLayout(e.GetPosition(this));
                    }
                }
            }

            base.OnMouseMove(e);
        }

        public void AddToSelection(Node_BaseVm node, bool withClean)
        {
            if (withClean)
            {
                foreach (var selectedNode in Selection)
                {
                    if (selectedNode != null && GraphNodes.ContainsKey(selectedNode))
                    {
                        GraphNodes[selectedNode].IsSelected = false;
                    }
                }

                Selection.Clear();
            }

            if (!Selection.Contains(node))
            {
                Selection.Add(node);

                if (node != null && GraphNodes.ContainsKey(node))
                {
                    GraphNodes[node].IsSelected = true;
                }

                SelectedValue = Selection.Count == 1 ? Selection[0] : null;
            }
        }

        protected void RemoveFromSelection(Node_BaseVm node)
        {
            if (Selection.Contains(node))
            {
                if (node != null && GraphNodes.ContainsKey(node))
                {
                    GraphNodes[node].IsSelected = false;
                }

                Selection.Remove(node);

                SelectedValue = Selection.Count == 1 ? Selection[0] : null;
            }
        }



        ICommand prevRootCommand;
        public ICommand PrevRootCommand => prevRootCommand ?? (prevRootCommand = new RelayCommand(() =>
        {
            rootNodeIndex = (rootNodeIndex - 1 + 2 * Tree.RootNodeIds.Count) % Tree.RootNodeIds.Count;
            OnFoundRoot(Tree.Nodes.FirstOrDefault((node) => node.Id == Tree.RootNodeIds[rootNodeIndex]));
        }, () => Tree != null && Tree.RootNodeIds.Count > 0));


        ICommand nextRootCommand;
        public ICommand NextRootCommand => nextRootCommand ?? (nextRootCommand = new RelayCommand(() =>
        {
            rootNodeIndex = (rootNodeIndex + 1 + 2 * Tree.RootNodeIds.Count) % Tree.RootNodeIds.Count;
            OnFoundRoot(Tree.Nodes.FirstOrDefault((node) => node.Id == Tree.RootNodeIds[rootNodeIndex]));
        }, () => Tree != null && Tree.RootNodeIds.Count > 0));


        const string imageFilter = "Image files (*.png;*.jpg;*.jpeg;*.tiff;*.bmp)|*.png;*.jpg;*.jpeg;*.tiff;*.bmp";

        ICommand setBackgroundCommand;
        public ICommand SetBackgroundCommand => setBackgroundCommand ?? (setBackgroundCommand = new RelayCommand(() =>
        {
            string path = IDialogService.DialogService.OpenFileDialog(imageFilter, false);
            if (!string.IsNullOrEmpty(path)) { } ////// TODO
        }));


        protected ICommand resetScaleCommand;
        public ICommand ResetScaleCommand => resetScaleCommand ?? (resetScaleCommand = new RelayCommand(() =>
        {
            double oldScale = Scale;

            Scale = 1.0;

            TranslationX += ActualWidth / 2 * (1 / oldScale - 1 / Scale);
            TranslationY += ActualHeight / 2 * (1 / oldScale - 1 / Scale);

            OnTransformChanged();
        }));


        protected ICommand openPlayerCommand;
        public ICommand OpenPlayerCommand => openPlayerCommand ?? (openPlayerCommand = new RelayCommand(() =>
        {
            new InfoWindow("▶ Воспроизведение", "DT_ContentControl_TreePlayer", new TreeCanvasProxy { TreeCanvas = this }) { Owner = App.Current.MainWindow }.ShowDialog();
            Stop();
        }));


        ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand =>
            toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand<Node_BaseVm>((node) => node.ToggleGender(), (node) => node != null));


        ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand<object>((obj) =>
        {
            if (obj is NodePairVm link) Tree.RemoveLink(link);
            if (obj is Node_BaseVm node) Tree.RemoveNode(node);
        }, (obj) => obj != null));


        ICommand togglePlayCommand;
        public ICommand TogglePlayCommand =>
            togglePlayCommand ?? (togglePlayCommand = new RelayCommand(() => { if (ActiveContext != null) PauseUnpause(); else { StartTransition(SelectedValue); IsPlaying = ActiveContext != null; } }, () => SelectedValue != null));


        ICommand stopCommand;
        public ICommand StopCommand =>
            stopCommand ?? (stopCommand = new RelayCommand(() => Stop(), () => ActiveContext != null));


        ICommand toggleGenderToPlayCommand;
        public ICommand ToggleGenderToPlayCommand =>
            toggleGenderToPlayCommand ?? (toggleGenderToPlayCommand = new RelayCommand(() => { GenderToPlay = 3 - GenderToPlay; }, () => ActiveContext == null));
    }
}