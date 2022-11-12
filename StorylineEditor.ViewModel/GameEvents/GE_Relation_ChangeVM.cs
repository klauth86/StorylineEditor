using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Common;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.GameEvents
{
    public class GE_Relation_ChangeVM : GE_BaseVM<GE_Relation_ChangeM>
    {
        public CollectionViewSource CharactersCVS { get; }

        public GE_Relation_ChangeVM(GE_Relation_ChangeM model, ICallbackContext callbackContext) : base(model, callbackContext)
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
    }
}