/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Graphs
{
    public class DialogEditorVM : GraphEditorVM<DialogM, object>
    {
        public CollectionViewSource FilteredDialogCharacterCVS { get; }
        public CollectionViewSource FilteredDialogLocationCVS { get; }
        public DialogEditorVM(
            DialogVM viewModel
            , object parent
            , IEnumerable<Type> nodeTypes
            , Func<Type, Point, BaseM> mCreator
            , Func<BaseM, Notifier> vmCreator
            , Func<Notifier, Notifier> evmCreator
            )
            : base(
                  viewModel.Model
                  , parent
                  , nodeTypes
                  , mCreator
                  , vmCreator
                  , evmCreator
                  )
        {
            FilteredDialogCharacterCVS = new CollectionViewSource() { Source = ActiveContext.Characters };
            
            if (FilteredDialogCharacterCVS.View != null)
            {
                FilteredDialogCharacterCVS.View.Filter = OnCharacterFilter;
                FilteredDialogCharacterCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                FilteredDialogCharacterCVS.View.MoveCurrentTo(DialogCharacter);
            }

            FilteredDialogLocationCVS = new CollectionViewSource() { Source = ActiveContext.Locations };

            if (FilteredDialogLocationCVS.View != null)
            {
                FilteredDialogLocationCVS.View.Filter = OnLocationFilter;
                FilteredDialogLocationCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                FilteredDialogLocationCVS.View.MoveCurrentTo(DialogLocation);
            }
        }

        protected override string CanLinkNodes(INode from, INode to)
        {
            if (from == to) return nameof(ArgumentException);

            if (from is Node_GateVM) return nameof(ArgumentException);

            if (to is Node_ExitVM) return nameof(ArgumentException);

            if (FromNodesLinks.ContainsKey(from.Id) && ToNodesLinks.ContainsKey(to.Id))
            {
                foreach (var linkId in FromNodesLinks[from.Id])
                {
                    if (ToNodesLinks[to.Id].Contains(linkId)) return nameof(ArgumentException);
                }
            }

            return string.Empty;
        }

        private bool OnCharacterFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return model.id != CharacterM.PLAYER_ID && (string.IsNullOrEmpty(dialogCharacterFilter) || model.PassFilter(dialogCharacterFilter));
            }
            return false;
        }

        protected string dialogCharacterFilter;
        public string DialogCharacterFilter
        {
            set
            {
                if (value != dialogCharacterFilter)
                {
                    dialogCharacterFilter = value;
                    FilteredDialogCharacterCVS.View?.Refresh();
                }
            }
        }

        public BaseM DialogCharacter
        {
            get => ActiveContext.GetCharacter(Model.npcId);
            set
            {
                if (value?.id != Model.npcId)
                {
                    Model.npcId = value?.id;
                    OnModelChanged(Model, nameof(DialogVM.DialogCharacter));
                    Notify(nameof(DialogCharacter));
                }
            }
        }

        private bool OnLocationFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(dialogLocationFilter) || model.PassFilter(dialogLocationFilter);
            }
            return false;
        }

        protected string dialogLocationFilter;
        public string DialogLocationFilter
        {
            set
            {
                if (value != dialogLocationFilter)
                {
                    dialogLocationFilter = value;
                    FilteredDialogLocationCVS.View?.Refresh();
                }
            }
        }

        public BaseM DialogLocation
        {
            get => ActiveContext.GetLocation(Model.locationId);
            set
            {
                if (value?.id != Model.locationId)
                {
                    Model.locationId = value?.id;
                    OnModelChanged(Model, nameof(DialogVM.DialogLocation));
                    Notify(nameof(DialogLocation));
                }
            }
        }
    }
}