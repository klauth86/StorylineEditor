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
    public class PlayerVm : BaseVm<BaseTreesTabVm>
    {
        public PlayerVm(BaseTreesTabVm parent, long additionalTicks, TreeVm treeToPlay) : base(parent, additionalTicks)
        {
            TreeToPlay = treeToPlay;
            isPlaying = false;
            NodeTime = 4;

            MainWindow.TickEvent += OnTick;
        }

        ~PlayerVm()
        {
            MainWindow.TickEvent -= OnTick;
        }

        private void OnTick(double millisec)
        {
            if (IsPlaying)
            {
                NodeTimeLeft -= millisec;
                
                if (NodeTimeLeft < 0)
                {
                    List<Node_BaseVm> childNodes = TreeToPlay.GetPrimaryChildNodes(NowPlaying);

                    if (childNodes.Count == 1)
                    {
                        NowPlaying = childNodes[0];
                    }
                    if (childNodes.Count > 0)
                    {
                        if (NowPlaying is DNode_RandomVm randomNode)
                        { 
                        
                        }
                        else if (NowPlaying is DNode_TransitVm transitNode)
                        {

                        }
                        else if (NowPlaying is DNode_CharacterVm characterNode)
                        {

                        }
                    }
                    else
                    {
                        IsPlaying = false;
                        NowPlaying = null;
                    }
                }
            }
        }

        readonly TreeVm TreeToPlay;

        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (value != isPlaying)
                {
                    if (value) NowPlaying = TreeToPlay.Selected;

                    isPlaying = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        private double nodeTime;
        public double NodeTime
        {
            get => nodeTime;
            set
            {
                if (value != nodeTime)
                {
                    nodeTime = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        private double nodeTimeLeft;
        public double NodeTimeLeft
        {
            get => nodeTimeLeft;
            set
            {
                if (value != nodeTimeLeft)
                {
                    nodeTimeLeft = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        private Node_BaseVm nowPlaying;
        public Node_BaseVm NowPlaying
        { 
            get => nowPlaying;
            set 
            {
                if (value != nowPlaying)
                {
                    NodeTimeLeft = NodeTime;

                    nowPlaying = value;
                    NotifyWithCallerPropName();
                }
                

            }
        }

        protected ICommand togglePlayCommand;

        public ICommand TogglePlayCommand => togglePlayCommand ?? (togglePlayCommand = new RelayCommand
                    (() => IsPlaying = !IsPlaying, () => TreeToPlay != null && TreeToPlay.Selected != null));
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