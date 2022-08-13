/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.Views.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Шаг квеста")]
    [XmlRoot]
    public class JNode_StepVm : Node_BaseVm, ITagged
    {
        public JNode_StepVm(TreeVm Parent) : base(Parent)
        {
            Outs = new ObservableCollection<NodePairVm>();
            if (Parent != null) Parent.Links.CollectionChanged += OnLinksChanged;

            JournalTagIds = new ObservableCollection<string>();
            JournalTagIds.CollectionChanged += OnCollectionChanged;
        }

        public JNode_StepVm() : this(null) { }

        ~JNode_StepVm()
        {
            JournalTagIds.CollectionChanged -= OnCollectionChanged;

            if (Parent != null) Parent.Links.CollectionChanged -= OnLinksChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Notify(nameof(Tags));

        [XmlArray]
        public ObservableCollection<string> JournalTagIds { get; set; }

        private void OnLinksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (NodePairVm nodePair in e.NewItems)
                {
                    if (nodePair.FromId == id)
                    {
                        Outs.Add(nodePair);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (NodePairVm nodePair in e.OldItems)
                {
                    if (nodePair.FromId == id)
                    {
                        Outs.Remove(nodePair);
                    }
                }
            }
        }

        [XmlIgnore]
        public ObservableCollection<NodePairVm> Outs { get; }



        [XmlIgnore]
        public List<JournalTagVm> Tags => Parent?.Parent?.Parent?.GlobalTagsTab.Items.Where((tag) => JournalTagIds.Contains(tag.Id)).ToList();
        
        protected CollectionViewSource tagsToAdd;
        [XmlIgnore]
        public CollectionViewSource TagsToAdd
        {
            get
            {
                if (Parent != null && tagsToAdd == null)
                {
                    tagsToAdd = new CollectionViewSource() { Source = Parent.Parent.Parent.GlobalTagsTab.Items };
                    tagsToAdd.View.Filter = (object obj) => obj is JournalTagVm tag && !JournalTagIds.Contains(tag.Id);
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
                if (value != null && !JournalTagIds.Contains(value.Id))
                {
                    JournalTagIds.Add(value.Id);
                    TagsToAdd.View.Refresh();
                }
            }
        }

        ICommand removeTagCommand;
        public ICommand RemoveTagCommand => removeTagCommand ?? (removeTagCommand = new RelayCommand<JournalTagVm>(tag => RemoveTag(tag.Id), tag => tag != null));

        public void RemoveTag(string tagId) { JournalTagIds.Remove(tagId); TagsToAdd.View.Refresh(); }



        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is JNode_StepVm casted)
            {
                foreach (var journalTag in JournalTagIds)
                {
                    casted.JournalTagIds.Add(journalTag);
                }
            }
        }

        public override void SetupParenthood()
        {
            Parent.Links.CollectionChanged += OnLinksChanged;

            foreach (var nodePair in Parent.Links.Where(link => link.FromId == id)) Outs.Add(nodePair);
        }
    }
}