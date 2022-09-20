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
using StorylineEditor.ViewModels.Tabs;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModels
{
    public class TreePlayerVm : BaseVm<BaseTreesTabVm>, IDialogContext
    {
        public void OnClosing()
        {
            TreePlayerInstance = null;
            
            Stop();

            TreeToPlay.DurationAlphaChangedEvent -= OnDurationAlphaChanged;
            TreeToPlay.EndActiveNodeEvent -= OnEndActiveNode;
            TreeToPlay.EndTransitionEvent -= OnEndTransition;
        }

        public TreePlayerVm(BaseTreesTabVm parent, long additionalTicks, TreeVm treeToPlay) : base(parent, additionalTicks)
        {
            Random = new Random();
            TreeToPlay = treeToPlay;

            activeTime = 4;

            activeContext = null;

            gender = 1;

            isLocked = false;

            TreeToPlay.EndTransitionEvent += OnEndTransition;
            TreeToPlay.EndActiveNodeEvent += OnEndActiveNode;
            TreeToPlay.DurationAlphaChangedEvent += OnDurationAlphaChanged;

            TreePlayerInstance = this;
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
                StartTransition(childNodes[0]);
            }
            else if (childNodes.Count > 0)
            {
                if (activeNode is DNode_RandomVm randomNode)
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

        private void OnDurationAlphaChanged(double alpha)
        {
            ActiveTimeLeft = ActiveTime * (1 - alpha);
        }

        public void StartTransition(Node_BaseVm nextNode)
        {
            TreeToPlay.OnStartTransition(nextNode);

            ActiveContext = new TreePlayerContext_TransitionVm();
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

        protected bool isLocked;
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (value != isLocked)
                {
                    isLocked = value;
                    NotifyWithCallerPropName();
                }
            }
        }


        public static TreePlayerVm TreePlayerInstance = null;


        protected ICommand togglePlayCommand;
        public ICommand TogglePlayCommand => togglePlayCommand ?? (togglePlayCommand = new RelayCommand
                    (() =>
                    {
                        if (ActiveContext == null)
                        {
                            OnEndTransition(TreeToPlay.Selected);
                            TreeToPlay.IsPlaying = true;
                        }
                        else
                        {
                            TreeToPlay.IsPlaying = !TreeToPlay.IsPlaying;
                        }
                    }, () => TreeToPlay?.Selected != null));


        protected ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(() => Stop()));


        ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand => toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand(() => Gender = 3 - gender));

        ICommand toggleLockCommand;
        public ICommand ToggleLockCommand => toggleLockCommand ?? (toggleLockCommand = new RelayCommand(() => IsLocked = !IsLocked));
    }
}
