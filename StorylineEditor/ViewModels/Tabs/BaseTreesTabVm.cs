/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    public class PlayerErrorVm : BaseVm { }

    public class PlayerTransitionVm : BaseVm { }

    public class PlayerChoiceVm : BaseVm<PlayerVm>
    {
        public PlayerChoiceVm(PlayerVm parent, Node_BaseVm activeNode, List<Node_BaseVm> nodesToSelect) : base(parent, 0)
        {
            ActiveNode = activeNode;
            NodesToSelect = new List<Node_BaseVm>();
            NodesToSelect.AddRange(nodesToSelect); 
        }

        public Node_BaseVm ActiveNode { get; set; }
        public List<Node_BaseVm> NodesToSelect { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<Node_BaseVm>((node) => { Parent?.StartTransition(ActiveNode, node); }, (node) => node != null && NodesToSelect.Contains(node)));
    }

    public class PlayerVm : BaseVm<BaseTreesTabVm>, IDialogContext
    {
        public void OnClosing()
        {
            Stop();

            TreeToPlay.DurationAlphaChangedEvent -= OnDurationAlphaChanged;
            TreeToPlay.EndActiveNodeEvent -= OnEndActiveNode;
            TreeToPlay.EndTransitionEvent -= OnEndTransition;
        }

        public PlayerVm(BaseTreesTabVm parent, long additionalTicks, TreeVm treeToPlay) : base(parent, additionalTicks)
        {
            Random = new Random();
            TreeToPlay = treeToPlay;

            activeTime = 4;

            activeContext = null;

            gender = 1;

            TreeToPlay.EndTransitionEvent += OnEndTransition;
            TreeToPlay.EndActiveNodeEvent += OnEndActiveNode;
            TreeToPlay.DurationAlphaChangedEvent += OnDurationAlphaChanged;
        }

        private void OnEndTransition(object nodeObject)
        {
            var node = nodeObject as Node_BaseVm;
            
            TreeToPlay.OnStartActiveNode(node, activeTime);

            ActiveContext = node;
        }

        private void OnEndActiveNode(object nodeObject)
        {
            var activeNode = nodeObject as Node_BaseVm;

            List<Node_BaseVm> childNodes = TreeToPlay.GetChildNodes(activeNode);

            childNodes.RemoveAll((node) => node.Gender > 0 && node.Gender != Gender);

            ////// TODO Execute other predicates

            if (childNodes.Count == 1)
            {
                StartTransition(activeNode, childNodes[0]);
            }
            else if (childNodes.Count > 0)
            {
                if (activeNode is DNode_RandomVm randomNode)
                {
                    StartTransition(activeNode, childNodes[Random.Next(childNodes.Count)]);
                }
                else if (childNodes.TrueForAll((childNode) => (childNode is IOwnered owneredNode) && owneredNode.Owner != null && owneredNode.Owner.Id == CharacterVm.PlayerId))
                {
                    ActiveContext = new PlayerChoiceVm(this, activeNode, childNodes);
                }
                else
                {
                    ActiveContext = new PlayerErrorVm(); ////// TODO error description
                }
            }
            else
            {
                Stop();
            }
        }

        private void OnDurationAlphaChanged(double alpha)
        {
            ActiveTimeLeft = ActiveTime * (1 - alpha);
        }

        public void StartTransition(Node_BaseVm fromNode, Node_BaseVm toNode)
        {
            TreeToPlay.OnStartTransition(fromNode, toNode);

            ActiveContext = new PlayerTransitionVm();
        }

        private void Stop()
        {
            TreeToPlay.OnStop();

            ActiveContext = null;
        }

        public Random Random { get; private set; }
        public TreeVm TreeToPlay { get; private set; }


        private double activeTime;
        public double ActiveTime
        {
            get => activeTime;
            set
            {
                if (value != activeTime)
                {
                    activeTime = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        private double activeTimeLeft;
        public double ActiveTimeLeft
        {
            get => activeTimeLeft;
            set
            {
                if (value != activeTimeLeft)
                {
                    activeTimeLeft = value;
                    NotifyWithCallerPropName();
                }
            }
        }


        private BaseVm activeContext;
        public BaseVm ActiveContext
        {
            get => activeContext;
            set
            {
                if (value != activeContext)
                {
                    activeContext = value;
                    NotifyWithCallerPropName();
                }
            }
        }


        protected int gender;
        public int Gender
        {
            get => gender;
            set
            {
                if (value != gender)
                {
                    gender = value;
                    NotifyWithCallerPropName();
                }
            }
        }
        
        
        protected ICommand togglePlayCommand;
        public ICommand TogglePlayCommand => togglePlayCommand ?? (togglePlayCommand = new RelayCommand
                    (() =>
                    {
                        if (ActiveContext == null)
                        {
                            OnEndTransition(TreeToPlay.Selected);
                        }
                        else
                        {
                            TreeToPlay.OnPauseUnpause();
                        }

                    }, () => TreeToPlay?.Selected != null));


        protected ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(() => Stop()));


        ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand => toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand(() => Gender = 3 - gender));
    }

    [XmlRoot]
    public abstract class BaseTreesTabVm : FolderedTabVm
    {
        public BaseTreesTabVm(FullContextVm Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public BaseTreesTabVm() : this(null, 0) { }

        protected abstract string GetItemDefaultName();

        public override FolderedVm CreateItem(object parameter)
        {
            if (parameter == FolderedVm.FolderFlag) return new TreeFolderVm(this, 0) { Name = "Новая папка" };

            return new TreeVm(this, 0) { Name = GetItemDefaultName() };
        }

        public Node_BaseVm CreateNode(TreeVm tree)
        {
            return selectedNodeType != null
                ? CustomByteConverter.CreateByName(selectedNodeType.Name, tree, 0) as Node_BaseVm
                : null;
        }

        protected Type selectedNodeType;
        [XmlIgnore]
        public Type SelectedNodeType
        {
            get => selectedNodeType;
            set
            {
                if (selectedNodeType != value)
                {
                    selectedNodeType = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ??
            (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type, type => type != null));

        public override bool EditItemInPlace => true;

        protected ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<FolderedVm>
            (
            (item) => new InfoWindow("Статистика " + item.Name, "DT_" + item.GetType().Name + "_Info", item) { Owner = App.Current.MainWindow }.Show(),
            (item) => item != null && !item.IsFolder
            ));

        protected ICommand playCommand;
        public ICommand PlayCommand => playCommand ?? (playCommand = new RelayCommand
            (
            () => new InfoWindow("Воспроизведение " + SelectedItem.Name, "DT_" + SelectedItem.GetType().Name + "_Player", new PlayerVm(this, 0, SelectedItem as TreeVm)) { Owner = App.Current.MainWindow }.ShowDialog()
            ));
    }
}