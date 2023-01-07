/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.GameEvents
{
    public class GE_Relation_ChangeVM : GE_BaseVM<GE_Relation_ChangeM, object>
    {
        public CollectionViewSource CharactersCVS { get; }

        public GE_Relation_ChangeVM(GE_Relation_ChangeM model, object parent) : base(model, parent)
        {
            CharactersCVS = new CollectionViewSource() { Source = ActiveContextService.Characters };

            if (CharactersCVS.View != null)
            {
                CharactersCVS.View.Filter = OnFilter;
                CharactersCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                CharactersCVS.View.MoveCurrentTo(Character);
            }
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return model.id != CharacterM.PLAYER_ID && (string.IsNullOrEmpty(charactersFilter) || model.PassFilter(charactersFilter));
            }
            return false;
        }

        protected string charactersFilter;
        public string CharactersFilter
        {
            set
            {
                if (value != charactersFilter)
                {
                    charactersFilter = value;
                    CharactersCVS.View?.Refresh();
                }
            }
        }

        public BaseM Character
        {
            get => ActiveContextService.GetCharacter(Model.npcId);
            set
            {
                if (value?.id != Model.npcId)
                {
                    Model.npcId = value?.id;
                    Notify(nameof(Character));
                }
            }
        }

        public float Value
        {
            get => Model.value;
            set
            {
                if (value != Model.value)
                {
                    Model.value = value;
                    Notify(nameof(Value));
                }
            }
        }

        public override void Execute()
        {
            if (Character != null)
            {
                CharacterEntryVM characterEntryVm = ActiveContextService.History.CharacterEntries.FirstOrDefault((ceVm) => ceVm.Model.id == Character.id);

                if (characterEntryVm == null)
                {
                    ActiveContextService.History.AddCharacter(Character);
                }

                characterEntryVm = ActiveContextService.History.CharacterEntries.FirstOrDefault((ceVm) => ceVm.Model.id == Character.id);

                characterEntryVm.DeltaRelation += Value;
            }
        }
    }
}