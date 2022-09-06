/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StorylineEditor.Views.Controls
{
    class TreeCanvas : Canvas
    {
        #region TreeVm DP

        public Vector Snapping
        {
            get => (Vector)this.GetValue(SnappingProperty);
            set { this.SetValue(SnappingProperty, value); }
        }

        public TreeVm Tree
        {
            get => (TreeVm)this.GetValue(TreeProperty);
            set { this.SetValue(TreeProperty, value); }
        }

        public double Scale
        {
            get => (double)this.GetValue(ScaleProperty);
            set { this.SetValue(ScaleProperty, value); }
        }

        public Vector Offset
        {
            get => (Vector)this.GetValue(OffsetProperty);
            set { this.SetValue(OffsetProperty, value); }
        }

        public double AnimAlpha
        {
            get => (double)this.GetValue(AnimAlphaProperty);
            set { this.SetValue(AnimAlphaProperty, value); }
        }

        public static readonly DependencyProperty TreeProperty = DependencyProperty.Register(
            "Tree", typeof(TreeVm), typeof(TreeCanvas), new PropertyMetadata(null, OnTreeChanged));

        public static readonly DependencyProperty SnappingProperty = DependencyProperty.Register(
            "Snapping", typeof(Vector), typeof(TreeCanvas));

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(TreeCanvas), new PropertyMetadata(1.0));

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset", typeof(Vector), typeof(TreeCanvas), new PropertyMetadata(new Vector(0, 0)));

        public static readonly DependencyProperty AnimAlphaProperty = DependencyProperty.Register(
            "AnimAlpha", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0, OnAnimAlphaChanged));

        private static void OnTreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeCanvas treeCanvas)
            {
                if (e.OldValue is TreeVm oldTree)
                {
                    oldTree.OnSetBackground -= treeCanvas.OnSetBackground;
                    oldTree.OnFoundRoot -= treeCanvas.OnFoundRoot;
                    oldTree.OnNodeRemoved -= treeCanvas.OnNodeRemoved;
                    oldTree.OnNodeAdded -= treeCanvas.OnNodeAdded;
                    oldTree.OnNodeCopied -= treeCanvas.OnNodeCopied;
                    oldTree.OnNodePasted -= treeCanvas.OnNodePasted;
                    oldTree.OnLinkRemoved -= treeCanvas.OnLinkRemoved;
                    oldTree.OnLinkAdded -= treeCanvas.OnLinkAdded;
                    oldTree.OnNodePositionChanged -= treeCanvas.OnNodePositionChanged;

                    oldTree.StartActiveNodeEvent -= treeCanvas.StartActiveNode;
                    oldTree.StartTransitionEvent -= treeCanvas.StartTransition;
                    oldTree.PauseUnpauseEvent -= treeCanvas.PauseUnpause;
                    oldTree.StopEvent -= treeCanvas.Stop;

                    foreach (var link in oldTree.Links) treeCanvas.OnLinkRemoved(link);
                    foreach (var item in oldTree.Nodes) treeCanvas.OnNodeRemoved(item);
                }

                treeCanvas.Reset();

                if (e.NewValue is TreeVm newTree)
                {
                    foreach (var node in newTree.Nodes) treeCanvas.AddGraphNode(node);
                    foreach (var link in newTree.Links) treeCanvas.AddGraphLink(link);

                    newTree.OnSetBackground += treeCanvas.OnSetBackground;
                    newTree.OnFoundRoot += treeCanvas.OnFoundRoot;
                    newTree.OnNodeRemoved += treeCanvas.OnNodeRemoved;
                    newTree.OnNodeAdded += treeCanvas.OnNodeAdded;
                    newTree.OnNodeCopied += treeCanvas.OnNodeCopied;
                    newTree.OnNodePasted += treeCanvas.OnNodePasted;
                    newTree.OnLinkRemoved += treeCanvas.OnLinkRemoved;
                    newTree.OnLinkAdded += treeCanvas.OnLinkAdded;
                    newTree.OnNodePositionChanged += treeCanvas.OnNodePositionChanged;

                    newTree.StartActiveNodeEvent += treeCanvas.StartActiveNode;
                    newTree.StartTransitionEvent += treeCanvas.StartTransition;
                    newTree.PauseUnpauseEvent += treeCanvas.PauseUnpause;
                    newTree.StopEvent += treeCanvas.Stop;
                }
            }
        }

        private static void OnAnimAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeCanvas treeCanvas)
            {
                if (treeCanvas.FocusNode != null)
                {
                    if (treeCanvas.PrevFocusNode != null)
                    {
                        Vector leftTopPosA = treeCanvas.PrevFocusNode.Position - new Point(treeCanvas.ActualWidth / 2 / treeCanvas.Scale, treeCanvas.ActualHeight / 2 / treeCanvas.Scale);
                        Vector offsetA = leftTopPosA + new Vector(treeCanvas.GraphNodes[treeCanvas.PrevFocusNode].ActualWidth / 2, treeCanvas.GraphNodes[treeCanvas.PrevFocusNode].ActualHeight / 2);

                        Vector leftTopPosB = treeCanvas.FocusNode.Position - new Point(treeCanvas.ActualWidth / 2 / treeCanvas.Scale, treeCanvas.ActualHeight / 2 / treeCanvas.Scale);
                        Vector offsetB = leftTopPosB + new Vector(treeCanvas.GraphNodes[treeCanvas.FocusNode].ActualWidth / 2, treeCanvas.GraphNodes[treeCanvas.FocusNode].ActualHeight / 2);

                        treeCanvas.Offset = offsetA * (1 - treeCanvas.AnimAlpha) + offsetB * treeCanvas.AnimAlpha;
                        treeCanvas.OnTransformChanged();
                    }
                    else
                    {
                        Vector leftTopPos = treeCanvas.FocusNode.Position - new Point(treeCanvas.ActualWidth / 2 / treeCanvas.Scale, treeCanvas.ActualHeight / 2 / treeCanvas.Scale);
                        treeCanvas.Offset = treeCanvas.Offset * (1 - treeCanvas.AnimAlpha) + (leftTopPos + new Vector(treeCanvas.GraphNodes[treeCanvas.FocusNode].ActualWidth / 2, treeCanvas.GraphNodes[treeCanvas.FocusNode].ActualHeight / 2)) * treeCanvas.AnimAlpha;
                        treeCanvas.OnTransformChanged();
                    }
                }
            }
        }

        #endregion

        const double AnimAlphaDuration = 0.5;
        
        const double StateAlphaDuration = 0.05;

        private void OnSetBackground(string path)
        {
            ////// TODO
        }

        private void OnTransformChanged()
        {
            Rect canvasRect = new Rect(
                Offset.X,
                Offset.Y,
                ActualWidth / Scale,
                ActualHeight / Scale);

            foreach (var graphNodesEntry in GraphNodes)
            {
                Node_BaseVm node = graphNodesEntry.Key;
                GraphNode graphNode = graphNodesEntry.Value;

                RefreshNodePosition(node, graphNode);

                Rect graphNodeRect = new Rect(
                    node.Position.X,
                    node.Position.Y,
                    graphNode.ActualWidth * Scale,
                    graphNode.ActualHeight * Scale);

                Rect canvasRectCopy = canvasRect;
                canvasRectCopy.Intersect(graphNodeRect);

                graphNode.IsOutOfView = canvasRectCopy.IsEmpty;

                (graphNode.RenderTransform as ScaleTransform).ScaleX = Scale;
                (graphNode.RenderTransform as ScaleTransform).ScaleY = Scale;
            }

            UpdateLinksLayout(null);
        }

        private void RefreshNodePosition(Node_BaseVm node, GraphNode graphNode)
        {
            Canvas.SetLeft(graphNode, (node.Position.X - Offset.X) * Scale);
            Canvas.SetTop(graphNode, (node.Position.Y - Offset.Y) * Scale);
        }

        private void OnFoundRoot(Node_BaseVm node)
        {
            if (GraphNodes.ContainsKey(node))
            {
                FocusNode = null;
                SetAnimAlphaStoryboard(node, AnimAlphaDuration);
                Dispatcher.BeginInvoke(new Action(() => { AnimAlphaStoryboard?.Begin(); }));
            }
        }

        private void OnNodeAdded(Node_BaseVm node) { AddGraphNode(node); }

        private void OnNodeCopied(Node_BaseVm node) { node.PositionX -= Offset.X; node.PositionY -= Offset.Y; }

        private void OnNodePasted(Node_BaseVm node) { node.PositionX += Offset.X; node.PositionY += Offset.Y; }

        private void OnLinkAdded(NodePairVm link) { AddGraphLink(link); }
        
        private void OnNodePositionChanged(Node_BaseVm node)
        {
            if (GraphNodes.ContainsKey(node)) {
                RefreshNodePosition(node, GraphNodes[node]);
                UpdateLinksLayout(GraphNodes[node]);
            }
        }

        private void OnCompleted_EndActiveNode(object sender, EventArgs e)
        {
            AnimAlphaStoryboard.Completed -= OnCompleted_EndActiveNode;
            Tree?.OnEndActiveNode(FocusNode);
        }

        private void OnCompleted_EndTransition(object sender, EventArgs e)
        {
            AnimAlphaStoryboard.Completed -= OnCompleted_EndTransition;
            Tree?.OnEndTransition(FocusNode);
        }

        private void SetAnimAlphaStoryboard(Node_BaseVm activeNode, double activeTime)
        {
            PrevFocusNode = FocusNode;
            FocusNode = activeNode;

            var focusAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(activeTime)
            };

            Storyboard.SetTarget(focusAnimation, this);
            Storyboard.SetTargetProperty(focusAnimation, new PropertyPath("AnimAlpha"));

            StopAnimAlphaStoryboard();

            AnimAlphaStoryboard = new Storyboard();
            AnimAlphaStoryboard.Children.Add(focusAnimation);
        }

        private void StopAnimAlphaStoryboard()
        {
            if (AnimAlphaStoryboard != null)
            {
                AnimAlphaStoryboard.Stop();
                AnimAlphaStoryboard.Completed -= OnCompleted_EndActiveNode;
                AnimAlphaStoryboard.Completed -= OnCompleted_EndTransition;
            }
        }

        private void StartActiveNode(Node_BaseVm activeNode, double duration)
        {
            if (activeNode != null)
            {
                bool wasCreated = false;

                if (PlayingAdorner == null)
                {
                    PlayingAdorner = new PlayingAdorner(Scale);
                    Canvas.SetZIndex(PlayingAdorner, -ActiveZIndex);

                    double fromX = Canvas.GetLeft(GraphNodes[activeNode]) + GraphNodes[activeNode].ActualWidth * Scale / 2;
                    double toX = Canvas.GetTop(GraphNodes[activeNode]) + GraphNodes[activeNode].ActualHeight * Scale / 2;

                    Canvas.SetLeft(this, fromX - PlayingAdorner.Width * Scale / 2);
                    Canvas.SetTop(this, toX - PlayingAdorner.Height * Scale / 2);

                    Children.Add(PlayingAdorner);

                    wasCreated = true;
                }

                if (wasCreated)
                {
                    PlayingAdorner.ToActiveNodeState(GraphNodes[activeNode], 0.1);

                    SetAnimAlphaStoryboard(activeNode, AnimAlphaDuration);
                    
                    EventHandler onCompleted_Callback = null;

                    onCompleted_Callback = (o, e) =>
                    {
                        AnimAlphaStoryboard.Completed -= onCompleted_Callback;
                        StartActiveNode(activeNode, duration);
                    };

                    AnimAlphaStoryboard.Completed += onCompleted_Callback;
                    
                    Dispatcher.BeginInvoke(new Action(() => AnimAlphaStoryboard?.Begin()));
                }
                else
                {
                    PlayingAdorner.ToActiveNodeState(GraphNodes[activeNode], StateAlphaDuration);

                    SetAnimAlphaStoryboard(activeNode, duration);
                    AnimAlphaStoryboard.Completed += OnCompleted_EndActiveNode;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AnimAlphaStoryboard?.Begin();
                        Tree.IsPlaying = true;
                    }));
                }
            }
        }

        private void StartTransition(Node_BaseVm nodeA, Node_BaseVm nodeB)
        {
            PlayingAdorner?.ToTransitionState(StateAlphaDuration);

            FocusNode = nodeA;
            SetAnimAlphaStoryboard(nodeB, AnimAlphaDuration);
            AnimAlphaStoryboard.Completed += OnCompleted_EndTransition;

            Dispatcher.BeginInvoke(new Action(() => AnimAlphaStoryboard?.Begin()));
        }

        private void PauseUnpause()
        {
            if (AnimAlphaStoryboard != null)
            {
                if (!AnimAlphaStoryboard.GetIsPaused())
                {
                    AnimAlphaStoryboard.Pause();
                    Tree.IsPlaying = false;
                }
                else
                {
                    AnimAlphaStoryboard.Resume();
                    Tree.IsPlaying = true;
                }
            }
        }

        private void Stop()
        {
            StopAnimAlphaStoryboard();

            PrevFocusNode = null;
            FocusNode = null;

            if (PlayingAdorner != null)
            {
                Children.Remove(PlayingAdorner);
                PlayingAdorner = null;
            }

            if (Tree.Selected != null)
            {
                FocusNode = null;
                SetAnimAlphaStoryboard(Tree.Selected, AnimAlphaDuration);
                Dispatcher.BeginInvoke(new Action(() => { AnimAlphaStoryboard?.Begin(); }));
            }
        }

        private void AddGraphNode(Node_BaseVm node)
        {
            GraphNode graphNode = new GraphNode();
            RefreshNodePosition(node, graphNode);
            graphNode.RenderTransform = new ScaleTransform(Scale, Scale);
            
            graphNode.DataContext = node;
            graphNode.SizeChanged += OnNodeSizeChanged;

            Canvas.SetZIndex(graphNode, ActiveZIndex);
            Children.Add(graphNode);

            GraphNodes.Add(node, graphNode);
        }

        private void OnNodeSizeChanged(object sender, SizeChangedEventArgs e) { UpdateLinksLayout(sender as GraphNode); }

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

        private void AddGraphLink(NodePairVm link)
        {
            if (Tree != null)
            {
                var fromPair = GraphNodes.FirstOrDefault((gnodePair) => gnodePair.Key.Id == link.FromId);
                var fromVertice = fromPair.Value;
                var toPair = GraphNodes.First((gnodePair) => gnodePair.Key.Id == link.ToId);
                var toVertice = toPair.Value;
                if (fromVertice != null && toVertice != null)
                {
                    var newGraphLink = CreateGraphLink(fromVertice, toVertice);
                    newGraphLink.LineAndArrow.DataContext = link;
                    newGraphLink.LinkContent.DataContext = link;

                    Children.Add(newGraphLink.LineAndArrow);

                    Canvas.SetZIndex(newGraphLink.LinkContent, ActiveZIndex + 1);
                    Children.Add(newGraphLink.LinkContent);

                    GraphLinks.Add(link, newGraphLink);

                    // Update layout after Loaded
                    Dispatcher.BeginInvoke(new Action(() => { newGraphLink.UpdateLayout(); }), DispatcherPriority.Loaded);
                }
            }
        }

        private GraphLink CreateGraphLink(GraphNode fromVertice, GraphNode toVertice)
        {
            var from = fromVertice?.DataContext;

            return from is JNode_StepVm
            ? new SequenceLink(fromVertice, toVertice)
            : new GraphLink(fromVertice, toVertice);
        }

        private void OnLinkRemoved(NodePairVm link)
        {
            if (GraphLinks.ContainsKey(link))
            {
                GraphLinks[link].LinkContent.DataContext = null;
                GraphLinks[link].LineAndArrow.DataContext = null;

                Children.Remove(GraphLinks[link].LineAndArrow);
                Children.Remove(GraphLinks[link].LinkContent);
                
                GraphLinks.Remove(link);
            }
        }

        private void OnNodeRemoved(Node_BaseVm node)
        {
            if (GraphNodes.ContainsKey(node))
            {
                GraphNodes[node].SizeChanged -= OnNodeSizeChanged;
                GraphNodes[node].DataContext = null;

                Children.Remove(GraphNodes[node]);
                
                GraphNodes.Remove(node);
            }
        }

        public TreeCanvas() : base()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) { MainWindow.FacadeKeyEvent += OnFacadeKeyEvent; }

        private void OnUnloaded(object sender, RoutedEventArgs e) { MainWindow.FacadeKeyEvent -= OnFacadeKeyEvent; }

        private void OnFacadeKeyEvent(bool isDown, Key key)
        {
            shiftMode = isDown && (key == Key.LeftShift || key == Key.RightShift);
            ctrlMode = isDown && (key == Key.LeftCtrl || key == Key.RightCtrl);
        }

        protected bool shiftMode = false;
        protected bool ctrlMode = false;

        protected bool linkMode = false;
        protected bool dragMode = false;
        protected bool dragAllMode = false;
        protected Point prevMousePosition = new Point();
        protected GraphNode ActiveGraphNode;
        protected Rectangle SelectionRectangle;
        
        protected PlayingAdorner PlayingAdorner;
        protected Node_BaseVm PrevFocusNode;
        protected Node_BaseVm FocusNode;
        Storyboard AnimAlphaStoryboard;

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
            Offset *= 0;
        }

        protected Dictionary<Node_BaseVm, GraphNode> GraphNodes = new Dictionary<Node_BaseVm, GraphNode>();

        protected Dictionary<NodePairVm, GraphLink> GraphLinks = new Dictionary<NodePairVm, GraphLink>();

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            double oldScale = Scale;

            double goal = e.Delta > 0 ? 2 : 1.0 / 8;

            Scale = Scale * 0.9 + goal * 0.1;

            Point mousePosition = e.GetPosition(this);

            Point oldScaleMousePosition = new Point(mousePosition.X / oldScale, mousePosition.Y / oldScale);

            Point newScaleMousePosition = new Point(mousePosition.X / Scale, mousePosition.Y / Scale);

            Offset += oldScaleMousePosition - newScaleMousePosition;

            OnTransformChanged();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Tree != null)
            {
                if (e.LeftButton == MouseButtonState.Released &&
                    e.RightButton == MouseButtonState.Pressed)
                {
                    if (e.Source is GraphNode graphNode)
                    {
                        if (shiftMode)
                        {
                            if (graphNode.DataContext is Node_BaseVm node)
                            {
                                var newNode = node.Clone<Node_BaseVm>(node.Parent, 0);
                                Tree.AddNode(newNode);

                                if (GraphNodes.ContainsKey(newNode))
                                {
                                    GraphNodes[newNode].RelativePosition = graphNode.RelativePosition;
                                    ActiveGraphNode = GraphNodes[newNode];
                                    dragMode = true;
                                    e.Handled = true;
                                }
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
                    if (e.OriginalSource is Canvas)
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
                            var absoluteMousePosition = new Point(mousePosition.X / Scale, mousePosition.Y / Scale) + Offset;

                            if (Snapping.X * Snapping.Y >= 1)
                            {
                                absoluteMousePosition.X = Math.Round(absoluteMousePosition.X / Snapping.X) * Snapping.X;
                                absoluteMousePosition.Y = Math.Round(absoluteMousePosition.Y / Snapping.Y) * Snapping.Y;
                            }

                            var tab = Tree.Parent as BaseTreesTabVm;
                            var newNode = tab.CreateNode(Tree);
                            if (newNode != null)
                            {
                                newNode.Position = absoluteMousePosition;
                                Tree.AddNode(newNode);
                            }
                            e.Handled = true;
                        }
                    }
                    else if (e.Source is GraphNode)
                    {
                        if (e.Source is GraphNode graphNode)
                        {
                            var node = graphNode.DataContext as Node_BaseVm;
                            node.Parent.AddToSelection(node, !shiftMode);
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
                            Tree.AddLink(ActiveGraphNode.DataContext as Node_BaseVm, graphNodeTo.DataContext as Node_BaseVm);
                        }

                        IndicatorLink = null;
                        linkMode = false;
                        ActiveGraphNode = null;
                    }

                    dragAllMode = false;

                    if (SelectionRectangle != null)
                    {
                        Rect selectionRect = new Rect(
                            Canvas.GetLeft(SelectionRectangle) / Scale + Offset.X,
                            Canvas.GetTop(SelectionRectangle) / Scale + Offset.Y,
                            SelectionRectangle.ActualWidth / Scale,
                            SelectionRectangle.ActualHeight / Scale);

                        Queue<Node_BaseVm> nodesToSelect = new Queue<Node_BaseVm>();

                        foreach (var graphNodesEntry in GraphNodes)
                        {
                            Node_BaseVm node = graphNodesEntry.Key;
                            GraphNode graphNode = graphNodesEntry.Value;

                            Rect graphNodeRect = new Rect(
                                node.Position.X, 
                                node.Position.Y,
                                graphNode.ActualWidth * Scale,
                                graphNode.ActualHeight * Scale);

                            Rect selectionRectCopy = selectionRect;
                            selectionRectCopy.Intersect(graphNodeRect);

                            if (!selectionRectCopy.IsEmpty) nodesToSelect.Enqueue(graphNodesEntry.Key);
                        }

                        if (nodesToSelect.Count > 0)
                        {
                            var firstNode = nodesToSelect.Dequeue();
                            firstNode.Parent.AddToSelection(firstNode, true);

                            while (nodesToSelect.Count > 0)
                            {
                                var node = nodesToSelect.Dequeue();
                                node.Parent.AddToSelection(node, false);
                            }
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
                    var mousePosition = e.GetPosition(this);
                    var absoluteMousePosition = new Point(mousePosition.X / Scale, mousePosition.Y / Scale) - ActiveGraphNode.RelativePosition;

                    if (Snapping.X * Snapping.Y >= 1)
                    {
                        absoluteMousePosition.X = Math.Round(absoluteMousePosition.X / Snapping.X) * Snapping.X;
                        absoluteMousePosition.Y = Math.Round(absoluteMousePosition.Y / Snapping.Y) * Snapping.Y;
                    }

                    var absolutePosition = Offset + absoluteMousePosition;

                    var node = (ActiveGraphNode?.DataContext as Node_BaseVm);
                    if (node != null)
                    {
                        if (node.IsSelected)
                        {
                            foreach (var selected in node.Parent.Selection)
                            {
                                if (selected == node) continue;

                                selected.PositionX = absolutePosition.X + selected.PositionX - node.PositionX;
                                selected.PositionY = absolutePosition.Y + selected.PositionY - node.PositionY;
                            }
                        }

                        node.PositionX = absolutePosition.X;
                        node.PositionY = absolutePosition.Y;
                    }
                }
                else if (dragAllMode)
                {
                    var currentPosition = e.GetPosition(this);
                    Offset -= new Vector((currentPosition.X - prevMousePosition.X) / Scale, (currentPosition.Y - prevMousePosition.Y) / Scale);
                    prevMousePosition = currentPosition;
                    
                    OnTransformChanged();
                }
                else if (SelectionRectangle != null)
                {
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

        protected ICommand resetScaleCommand;
        public ICommand ResetScaleCommand => resetScaleCommand ?? (resetScaleCommand = new RelayCommand(() =>
        {
            double oldScale = Scale;

            Scale = 1.0;

            Offset += new Vector(ActualWidth / 2, ActualHeight / 2) * (1 / oldScale - 1 / Scale);

            OnTransformChanged();
        }));
    }
}
