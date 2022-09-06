using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Tabs;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModels
{
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

        public void StartTransition(Node_BaseVm nextNode)
        {
            TreeToPlay.OnStartTransition(nextNode);

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
}
