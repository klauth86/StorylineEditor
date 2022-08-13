/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public abstract class Node_BaseVm : BaseNamedVm<TreeVm>
    {
        protected override void RefreshSubscribtions(TreeVm oldValue, TreeVm newValue)
        {
            if (oldValue != null)
            {
                oldValue.OnRootNodesChanged -= OnRootNodesChanged;
                oldValue.OnLinkRemoved -= OnLinkRemoved;
            }
            if (newValue != null)
            {
                newValue.OnRootNodesChanged += OnRootNodesChanged;
                newValue.OnLinkRemoved += OnLinkRemoved;
            }
        }

        public Node_BaseVm(TreeVm Parent) : base(Parent) {
            Position = new Point(0, 0);
            Gender = UNISEX;

            FullContextVm.OnSearchFilterChangedEvent += OnSearchFilterChanged;
        }

        public Node_BaseVm() : this(null) { }

        ~Node_BaseVm() { FullContextVm.OnSearchFilterChangedEvent -= OnSearchFilterChanged; }

        private void OnRootNodesChanged() {
            Notify(nameof(IsRoot));
            Notify(nameof(Labels));
        }

        private void OnLinkRemoved(NodePairVm nodePair)
        {
            if (nodePair.ToId == this?.Id) {
                Notify(nameof(Labels));
            }
        }

        public bool IsRoot => Parent != null && Parent.RootNodeIds.Contains(Id);

        public bool IsLeaf => Parent != null && Parent.IsLeafNode(this);

        [XmlIgnore]
        public List<Node_BaseVm> ChildNodes => Parent.NodesTraversal(this, true);

        public virtual bool AllowsManyChildren => true;


        protected Point position;
        public Point Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    PositionX = value.X;
                    PositionY = value.Y;
                }
            }
        }

        [XmlIgnore]
        public bool IsSelected => Parent?.Selection.Contains(this) ?? false;
        public void NotifyIsSelectedChanged() { Notify(nameof(IsSelected)); }

        [XmlIgnore]
        public double PositionX
        {
            get => position.X;
            set
            {
                if (value != position.X)
                {
                    position.X = value;
                    NotifyWithCallerPropName();
                    Notify(nameof(position));
                    Parent?.NodePositionChanged(this);
                }
            }
        }

        [XmlIgnore]
        public double PositionY
        {
            get => position.Y;
            set
            {
                if (value != position.Y)
                {
                    position.Y = value;
                    NotifyWithCallerPropName();
                    Notify(nameof(position));
                    Parent?.NodePositionChanged(this);
                }
            }
        }


        protected string label;
        public string Label {
            get => label;
            set {
                if (value != label) {
                    label = value;
                    NotifyWithCallerPropName();

                    if (Parent != null) {
                        foreach (var childNode in Parent.NodesTraversal(this, false))
                        {
                            childNode.Notify(nameof(Labels));
                        }

                        Notify(nameof(Labels));
                    }
                }
            }
        }

        public string Labels
        {
            get
            {
                if (IsRoot) return Label;

                var labels = Parent.GetRootNodes(this).Where((node) => !string.IsNullOrEmpty(node.Label)).ToList();
                if (labels.Count > 0) return string.Join(", ", labels.Select((node) => node.Label));

                return null;
            }
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is Node_BaseVm casted)
            {
                casted.Position = Position;
                casted.label = label;
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

                    Parent?.RefreshJournalLinks_To(this);
                }
            }
        }

        public const int UNISEX = 0;
        public const int MALE = 1;
        public const int FEMALE = 2;

        public void ToggleGender() => Gender = (Gender + 1) % 3;

        public string GetGenderEnum()
        {
            if (gender == MALE) return "EGender::Male";

            if (gender == FEMALE) return "EGender::Female";

            return "GENDER ENUM BUG";
        }

        public string GetSafeString(string str) { return str?.Replace("\"", "\\\"")?.Replace("\n", "...")?.Replace("\r", "..."); }
    }
}
