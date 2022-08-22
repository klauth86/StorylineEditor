/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

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

        public Vector Scale
        {
            get => (Vector)this.GetValue(ScaleProperty);
            set { this.SetValue(ScaleProperty, value); }
        }

        public Vector Offset
        {
            get => (Vector)this.GetValue(OffsetProperty);
            set { this.SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty TreeProperty = DependencyProperty.Register(
            "Tree", typeof(TreeVm), typeof(TreeCanvas), new PropertyMetadata(null, OnTreeChanged));

        public static readonly DependencyProperty SnappingProperty = DependencyProperty.Register(
            "Snapping", typeof(Vector), typeof(TreeCanvas));

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(Vector), typeof(TreeCanvas), new PropertyMetadata(new Vector(1, 1)));

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset", typeof(Vector), typeof(TreeCanvas), new PropertyMetadata(new Vector(0, 0)));

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
                }
            }
        }

        #endregion

        private void OnSetBackground(string path)
        {
            ////// TODO
        }

        private void OnTransformChanged(Vector oldScale)
        {
            Rect canvasRect = new Rect(Offset.X, Offset.Y, ActualWidth / Scale.X, ActualHeight / Scale.Y);

            List<GraphNode> removedNodes = new List<GraphNode>();
            List<GraphNode> addedNodes = new List<GraphNode>();

            foreach (var graphNodesEntry in GraphNodes)
            {
                Node_BaseVm node = graphNodesEntry.Key;
                GraphNode graphNode = graphNodesEntry.Value;

                Rect graphNodeRect = new Rect(node.Position.X, node.Position.Y,
                    graphNode.ActualWidth * Scale.X / oldScale.X,
                    graphNode.ActualHeight * Scale.Y / oldScale.Y);

                Rect canvasRectCopy = canvasRect;
                canvasRectCopy.Intersect(graphNodeRect);

                if (canvasRectCopy.IsEmpty)
                {
                    if (Children.Contains(graphNode))
                    {
                        Children.Remove(graphNode);
                        removedNodes.Add(graphNode);
                    }
                }
                else
                {
                    if (!Children.Contains(graphNode))
                    {
                        Children.Add(graphNode);
                        addedNodes.Add(graphNode);
                    }

                    RefreshNodePosition(node, graphNode);
                    graphNode.RenderTransform = new ScaleTransform(Scale.X, Scale.Y);
                }
            }

            ////// TODO Update links
        }

        private void RefreshNodePosition(Node_BaseVm node, GraphNode graphNode)
        {
            Canvas.SetLeft(graphNode, (node.Position.X - Offset.X) * Scale.X);
            Canvas.SetTop(graphNode, (node.Position.Y - Offset.Y) * Scale.Y);
        }

        private void OnFoundRoot(Node_BaseVm node)
        {
            if (GraphNodes.ContainsKey(node))
            {
                ////// TODO
                //////var left = Canvas.GetLeft(GraphNodes[node]) + GraphNodes[node].ActualWidth / 2;
                //////var top = Canvas.GetTop(GraphNodes[node]) + GraphNodes[node].ActualHeight / 2;
                //////UpdateChildrenLayout(new Vector(ActualWidth / 2 - left, ActualHeight / 2 - top));
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

        private void AddGraphNode(Node_BaseVm node)
        {
            GraphNode graphNode = new GraphNode();
            RefreshNodePosition(node, graphNode);
            graphNode.RenderTransform = new ScaleTransform(Scale.X, Scale.Y);
            
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

            Scale = new Vector(1, 1);
            Offset = new Vector(0, 0);
        }

        protected Dictionary<Node_BaseVm, GraphNode> GraphNodes = new Dictionary<Node_BaseVm, GraphNode>();

        protected Dictionary<NodePairVm, GraphLink> GraphLinks = new Dictionary<NodePairVm, GraphLink>();

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            Vector oldScale = Scale;

            Scale = new Vector(
                e.Delta > 0 ? Scale.X * 2 : Scale.X / 2,
                e.Delta > 0 ? Scale.Y * 2 : Scale.Y / 2
                );

            Point mousePosition = e.GetPosition(this);

            Point oldScaleMousePosition = new Point(mousePosition.X / oldScale.X, mousePosition.Y / oldScale.Y);

            Point newScaleMousePosition = new Point(mousePosition.X / Scale.X, mousePosition.Y / Scale.Y);

            Offset += oldScaleMousePosition - newScaleMousePosition;

            OnTransformChanged(oldScale);
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
                            var absolutePosition = new Point(mousePosition.X / Scale.X, mousePosition.Y / Scale.Y) + Offset;
                            var tab = Tree.Parent as BaseTreesTabVm;
                            var newNode = tab.CreateNode(Tree);
                            if (newNode != null)
                            {
                                newNode.Position = absolutePosition;
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
                        var left = Canvas.GetLeft(SelectionRectangle);
                        var top = Canvas.GetTop(SelectionRectangle);
                        var right = left + SelectionRectangle.ActualWidth;
                        var bottom = top + SelectionRectangle.ActualHeight;

                        Queue<Node_BaseVm> nodesToSelect = new Queue<Node_BaseVm>();

                        foreach (var graphNodesEntry in GraphNodes)
                        {
                            var graphNodeLeft = Canvas.GetLeft(graphNodesEntry.Value);
                            var graphNodeTop = Canvas.GetTop(graphNodesEntry.Value);
                            var graphNodeRight = graphNodeLeft + graphNodesEntry.Value.ActualWidth;
                            var graphNodeBottom = graphNodeTop + graphNodesEntry.Value.ActualHeight;

                            if ((graphNodeLeft < right && graphNodeLeft > left ||
                                graphNodeRight < right && graphNodeRight > left) &&
                                (graphNodeTop < bottom && graphNodeTop > top ||
                                graphNodeBottom < bottom && graphNodeBottom > top)) nodesToSelect.Enqueue(graphNodesEntry.Key);
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
                    var currentPosition = e.GetPosition(this);
                    Vector offset = new Vector((currentPosition.X - prevMousePosition.X) / Scale.X, (currentPosition.Y - prevMousePosition.Y) / Scale.Y);
                    prevMousePosition = currentPosition;

                    var node = (ActiveGraphNode?.DataContext as Node_BaseVm);
                    if (node != null)
                    {
                        if (node.IsSelected)
                        {
                            foreach (var selected in node.Parent.Selection)
                            {
                                selected.PositionX += offset.X;
                                selected.PositionY += offset.Y;
                            }
                        }
                        else
                        {
                            node.PositionX += offset.X;
                            node.PositionY += offset.Y;
                        }
                    }
                }
                else if (dragAllMode)
                {
                    var currentPosition = e.GetPosition(this);
                    Offset -= new Vector((currentPosition.X - prevMousePosition.X) / Scale.X, (currentPosition.Y - prevMousePosition.Y) / Scale.Y);
                    prevMousePosition = currentPosition;
                    
                    OnTransformChanged(Scale);
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
    }
}
