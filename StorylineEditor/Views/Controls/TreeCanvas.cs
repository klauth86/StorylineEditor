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

        long prevTicks = 0;

        public byte Tick
        {
            get => (byte)this.GetValue(TickProperty);
            set { this.SetValue(TickProperty, value); }
        }

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

        public double TranslationX
        {
            get => (double)this.GetValue(TranslationXProperty);
            set { this.SetValue(TranslationXProperty, value); }
        }

        public double TranslationY
        {
            get => (double)this.GetValue(TranslationYProperty);
            set { this.SetValue(TranslationYProperty, value); }
        }

        public static readonly DependencyProperty TickProperty = DependencyProperty.Register(
            "Tick", typeof(byte), typeof(TreeCanvas), new PropertyMetadata((byte)0, OnTickChanged));

        public static readonly DependencyProperty TreeProperty = DependencyProperty.Register(
            "Tree", typeof(TreeVm), typeof(TreeCanvas), new PropertyMetadata(null, OnTreeChanged));

        public static readonly DependencyProperty SnappingProperty = DependencyProperty.Register(
            "Snapping", typeof(Vector), typeof(TreeCanvas));

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(TreeCanvas), new PropertyMetadata(1.0));

        public static readonly DependencyProperty TranslationXProperty = DependencyProperty.Register(
            "TranslationX", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0));

        public static readonly DependencyProperty TranslationYProperty = DependencyProperty.Register(
            "TranslationY", typeof(double), typeof(TreeCanvas), new PropertyMetadata(0.0));

        private static void OnTickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeCanvas treeCanvas)
            {
                long ticks = DateTime.Now.Ticks;

                long deltaTicks = ticks - treeCanvas.prevTicks;

                if (treeCanvas.translationDurationLeft > 0)
                {
                    treeCanvas.translationDurationLeft -= deltaTicks;

                    if (treeCanvas.translationDurationLeft < 0)
                    {
                        treeCanvas.TranslationX = treeCanvas.translationTarget.X;
                        treeCanvas.TranslationY = treeCanvas.translationTarget.Y;

                        treeCanvas.OnTransformChanged();

                        treeCanvas.onTransitionComplete?.Invoke();
                    }
                    else
                    {
                        double alpha = 1.0 * treeCanvas.translationDurationLeft / treeCanvas.translationDuration;
                        double betta = 1 - alpha;

                        treeCanvas.TranslationX = treeCanvas.translationTarget.X * betta + treeCanvas.TranslationX * alpha;
                        treeCanvas.TranslationY = treeCanvas.translationTarget.Y * betta + treeCanvas.TranslationY * alpha;

                        treeCanvas.OnTransformChanged();
                    }
                }

                if (treeCanvas.waitDurationLeft > 0)
                {
                    treeCanvas.waitDurationLeft -= deltaTicks;

                    if (treeCanvas.waitDurationLeft < 0)
                    {
                        treeCanvas.Tree?.OnDurationAlphaChanged(1);

                        treeCanvas.onWaitComplete?.Invoke();
                    }
                    else
                    {
                        treeCanvas.Tree?.OnDurationAlphaChanged(1.0 - 1.0 * treeCanvas.waitDurationLeft / treeCanvas.waitDuration);
                    }
                }

                treeCanvas.PlayingAdorner?.Tick(deltaTicks);

                treeCanvas.prevTicks = ticks;
            }
        }

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

        #endregion

        const double AnimAlphaDuration = 1;

        const double StateAlphaDuration = 0.2;

        private void OnSetBackground(string path)
        {
            ////// TODO
        }

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
                    node.Position.X,
                    node.Position.Y,
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

        private bool IsNearlyCentered(Node_BaseVm node)
        {
            if (GraphNodes.ContainsKey(node))
            {
                return
                    Math.Abs(Canvas.GetLeft(GraphNodes[node]) + GraphNodes[node].ActualWidth / 2 - ActualWidth / 2) < 5 &&
                    Math.Abs(Canvas.GetTop(GraphNodes[node]) + GraphNodes[node].ActualHeight / 2 - ActualHeight / 2) < 5;
            }

            return true;
        }

        private void OnFoundRoot(Node_BaseVm node)
        {
            if (GraphNodes.ContainsKey(node))
            {
                translationTarget.X = node.PositionX + GraphNodes[node].ActualWidth / 2 - ActualWidth / 2 / Scale;
                translationTarget.Y = node.PositionY + GraphNodes[node].ActualHeight / 2 - ActualHeight / 2 / Scale;

                translationDuration = TimeSpan.FromSeconds(AnimAlphaDuration).Ticks;
                translationDurationLeft = TimeSpan.FromSeconds(AnimAlphaDuration).Ticks;
            }
        }

        private void OnNodeAdded(Node_BaseVm node) { AddGraphNode(node); }

        private void OnNodeCopied(Node_BaseVm node)
        {
            node.PositionX = (node.PositionX - TranslationX) * Scale;
            node.PositionY = (node.PositionY - TranslationY) * Scale;
        }

        private void OnNodePasted(Node_BaseVm node)
        {
            node.PositionX = node.PositionX / Scale + TranslationX;
            node.PositionY = node.PositionY / Scale + TranslationY;
        }

        private void OnLinkAdded(NodePairVm link) { AddGraphLink(link); }

        private void OnNodePositionChanged(Node_BaseVm node)
        {
            if (GraphNodes.ContainsKey(node))
            {
                RefreshPosition(GraphNodes[node], (node.PositionX - TranslationX) * Scale, (node.PositionY - TranslationY) * Scale);
                UpdateLinksLayout(GraphNodes[node]);
            }
        }

        private void StartActiveNode(Node_BaseVm node, double duration)
        {
            if (node != null)
            {
                if (PlayingAdorner == null)
                {
                    PlayingAdorner = new PlayingAdorner(TimeSpan.FromSeconds(StateAlphaDuration).Ticks);
                    PlayingAdorner.RenderTransform = new ScaleTransform() { ScaleX = Scale, ScaleY = Scale };

                    Canvas.SetZIndex(PlayingAdorner, -ActiveZIndex);

                    if (!IsNearlyCentered(node))
                    {
                        translationTarget.X = node.PositionX + GraphNodes[node].ActualWidth / 2 - ActualWidth / 2 / Scale;
                        translationTarget.Y = node.PositionY + GraphNodes[node].ActualHeight / 2 - ActualHeight / 2 / Scale;

                        translationDuration = TimeSpan.FromSeconds(AnimAlphaDuration).Ticks;
                        translationDurationLeft = TimeSpan.FromSeconds(AnimAlphaDuration).Ticks;

                        onTransitionComplete = () => StartActiveNode(node, duration);
                        
                        return;
                    }
                }

                if (!Children.Contains(PlayingAdorner))
                {
                    PlayingAdorner.PositionX = node.PositionX + GraphNodes[node].ActualWidth / 2 - PlayingAdorner.Width / 2;
                    PlayingAdorner.PositionY = node.PositionY + GraphNodes[node].ActualHeight / 2 - PlayingAdorner.Height / 2;

                    RefreshPosition(PlayingAdorner, (PlayingAdorner.PositionX - TranslationX) / Scale, (PlayingAdorner.PositionY - TranslationY) / Scale);

                    Children.Add(PlayingAdorner);
                }

                PlayingAdorner.ToActiveNodeState(GraphNodes[node]);

                waitDuration = TimeSpan.FromSeconds(duration).Ticks;
                waitDurationLeft = TimeSpan.FromSeconds(duration).Ticks;

                onWaitComplete = () => { Tree?.OnEndActiveNode(node); };
            }
        }

        private void StartTransition(Node_BaseVm node)
        {
            PlayingAdorner.ToTransitionState();

            translationTarget.X = node.PositionX + GraphNodes[node].ActualWidth / 2 - ActualWidth / 2 / Scale;
            translationTarget.Y = node.PositionY + GraphNodes[node].ActualHeight / 2 - ActualHeight / 2 / Scale;

            translationDuration = TimeSpan.FromSeconds(2 * AnimAlphaDuration).Ticks;
            translationDurationLeft = TimeSpan.FromSeconds(2 * AnimAlphaDuration).Ticks;

            onTransitionComplete = () => Tree?.OnEndTransition(node);
        }

        private void PauseUnpause() { if (Tree != null) { Tree.IsPlaying = !Tree.IsPlaying; } }

        private void Stop()
        {
            Tree.IsPlaying = false;

            if (PlayingAdorner != null)
            {
                Children.Remove(PlayingAdorner);
                PlayingAdorner = null;
            }
        }

        private void AddGraphNode(Node_BaseVm node)
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
            Focusable = true;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            prevTicks = DateTime.Now.Ticks;

            var animation = new ByteAnimation()
            {
                From = 0,
                To = 63,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            BeginAnimation(TickProperty, animation);
            
            MainWindow.FacadeKeyEvent += OnFacadeKeyEvent;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.FacadeKeyEvent -= OnFacadeKeyEvent;

            BeginAnimation(TickProperty, null);
        }

        private void OnFacadeKeyEvent(bool isDown, Key key)
        {
            shiftMode = isDown && (key == Key.LeftShift || key == Key.RightShift);
            ctrlMode = isDown && (key == Key.LeftCtrl || key == Key.RightCtrl);
        }

        protected long translationDuration = 0;
        protected long translationDurationLeft = 0;
        protected Vector translationTarget = new Vector();
        protected Action onTransitionComplete = null;

        protected long waitDuration = 0;
        protected long waitDurationLeft = 0;
        protected Action onWaitComplete = null;

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
            translationDuration = 0;
            translationDurationLeft = 0;
            translationTarget.X = 0;
            translationTarget.Y = 0;
            onTransitionComplete = null;

            waitDuration = 0;
            waitDurationLeft = 0;
            onWaitComplete = null;

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

        protected Dictionary<Node_BaseVm, GraphNode> GraphNodes = new Dictionary<Node_BaseVm, GraphNode>();

        protected Dictionary<NodePairVm, GraphLink> GraphLinks = new Dictionary<NodePairVm, GraphLink>();

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

            bool isLockedByTreePlayer = (TreePlayerVm.TreePlayerInstance?.IsLocked ?? false) || ((TreePlayerVm.TreePlayerInstance?.ActiveContext ?? null) != null);

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

                    double absolutePositionX = TranslationX + absoluteMousePosition.X;
                    double absolutePositionY = TranslationY + absoluteMousePosition.Y;

                    var node = (ActiveGraphNode?.DataContext as Node_BaseVm);
                    if (node != null)
                    {
                        if (node.IsSelected)
                        {
                            foreach (var selected in node.Parent.Selection)
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
                else if (dragAllMode)
                {
                    var currentPosition = e.GetPosition(this);

                    TranslationX -= (currentPosition.X - prevMousePosition.X) / Scale;
                    TranslationY -= (currentPosition.Y - prevMousePosition.Y) / Scale;

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

            TranslationX += ActualWidth / 2 * (1 / oldScale - 1 / Scale);
            TranslationY += ActualHeight / 2 * (1 / oldScale - 1 / Scale);

            OnTransformChanged();
        }));
    }
}
