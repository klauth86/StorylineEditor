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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Predicates
{
    [Description("Статус тэгов")]
    [XmlRoot]
    public class P_HasJournalTagsVm : P_BaseVm, ITagged
    {
        public P_HasJournalTagsVm(Node_BaseVm inParent) : base(inParent)
        {
            TagStates = new ObservableCollection<JournalTagStateVm>();
        }

        public P_HasJournalTagsVm() : this(null) { }

        private bool PredicateFilter(object obj) => obj != null && TagStates.All((tagState) => tagState.Tag != obj);

        public override bool IsValid => base.IsValid && TagStates.Count > 0;

        public ObservableCollection<JournalTagStateVm> TagStates { get; set; }

        protected CollectionViewSource journalTagsToAdd;
        public CollectionViewSource JournalTagsToAdd
        {
            get
            {
                if (journalTagsToAdd == null)
                {
                    journalTagsToAdd = new CollectionViewSource() { Source = Parent.Parent.Parent.Parent.GlobalTagsTab.Items };
                    journalTagsToAdd.View.Filter = PredicateFilter;
                    journalTagsToAdd.View.MoveCurrentTo(null);
                }
                return journalTagsToAdd;
            }
        }

        [XmlIgnore]
        public JournalTagVm JournalTagToAdd
        {
            get => null;
            set
            {
                if (value != null && TagStates.All((tagState) => tagState.Tag != value))
                {
                    TagStates.Add(new JournalTagStateVm(this, value));
                    JournalTagsToAdd.View.Refresh();
                    NotifyIsValidChanged();
                }
            }
        }

        public void RemoveStateFor(JournalTagVm journalTag)
        {
            List<JournalTagStateVm> statesToRemove = new List<JournalTagStateVm>();
            foreach (var tagState in TagStates) if (tagState.Tag == journalTag) statesToRemove.Add(tagState);
            foreach (var stateToRemove in statesToRemove) TagStates.Remove(stateToRemove);

            JournalTagsToAdd.View.Refresh();
            NotifyIsValidChanged();
        }

        ICommand removeTagStateCommand;
        public ICommand RemoveTagStateCommand => removeTagStateCommand ?? (removeTagStateCommand = new RelayCommand<JournalTagStateVm>(stateToRemove => RemoveTag(stateToRemove.TagId)));

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            TagStates.Clear();
            JournalTagsToAdd.View.Refresh();
            NotifyIsValidChanged();
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is P_HasJournalTagsVm casted)
            {
                foreach (var tagState in TagStates)
                {
                    var newTagState = new JournalTagStateVm(this, tagState.Tag)
                    {
                        HasTag = tagState.HasTag
                    };
                    casted.TagStates.Add(newTagState);
                }
            }
        }

        public override string GenerateCode(string outerName)
        {
           return string.Join("&& ", TagStates.Select((tagState) => 
            string.Format("{1}journalContext.Tags.Contains(\"{0}\")", tagState.Tag.Id, tagState.HasTag ? "" : "!")));
        }

        public override void SetupParenthood()
        {
            foreach (var tagState in TagStates)
            {
                tagState.Parent = this;
                tagState.SetupParenthood();
            }
        }

        public void RemoveTag(string tagId)
        {
            var stateToRemove = TagStates.FirstOrDefault(state => state.Id == tagId);
            if (stateToRemove != null)
            {
                TagStates.Remove(stateToRemove);
                JournalTagsToAdd.View.Refresh();
                NotifyIsValidChanged();
            }
        }
    }
}