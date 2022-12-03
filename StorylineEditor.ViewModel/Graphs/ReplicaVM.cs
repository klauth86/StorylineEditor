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
using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Graphs
{
    public class ReplicaVM : BaseVM<ReplicaM>
    {
        public BaseM ReplicaLocation => ActiveContextService.GetLocation(Model.locationId);

        public ReplicaVM(ReplicaM model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        protected ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((viewModel) =>
        {
            CallbackContext?.Callback(this, nameof(ICallbackContext));
        }));

        public override string Stats
        {
            get
            {
                string result = "";

                Dictionary<string, Dictionary<string, int>> countByCharacter = new Dictionary<string, Dictionary<string, int>>();
                Dictionary<string, int> countByTypeDescription = new Dictionary<string, int>();

                int characterNameMaxLength = 0;

                foreach (var node in Model.nodes)
                {
                    string characterName = "N/A";

                    if (node is Node_RegularM regularNode)
                    {
                        characterName = ActiveContextService.GetCharacter(regularNode.characterId)?.name;
                        characterNameMaxLength = Math.Max(characterName?.Length ?? 0, characterNameMaxLength);
                    }

                    string gender = " ";

                    if (node.gender == GENDER.MALE) gender = "👨";
                    if (node.gender == GENDER.FEMALE) gender = "👩";

                    if (!countByCharacter.ContainsKey(characterName))
                    {
                        countByCharacter.Add(characterName, new Dictionary<string, int>() { { " ", 0 }, { "👨", 0 }, { "👩", 0 } });
                    }

                    countByCharacter[characterName][gender]++;

                    var typeDescription = node.GetType().Name;
                    if (!countByTypeDescription.ContainsKey(typeDescription)) countByTypeDescription.Add(typeDescription, 0);
                    countByTypeDescription[typeDescription]++;
                }

                if (countByCharacter.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ПЕРСОНАЖАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByCharacter.OrderBy(pair => pair.Key))
                    {
                        result += string.Format("{0, -" + (characterNameMaxLength + 6) + "}{1}", "- " + entry.Key + ":", string.Join("\t", entry.Value.Select(pair => string.Format("{0}{1, -6}", pair.Key, pair.Value)))) + Environment.NewLine;
                    }
                }

                if (countByTypeDescription.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ТИПАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByTypeDescription.OrderBy(pair => pair.Key)) result += "- " + entry.Key + ": " + entry.Value + Environment.NewLine;
                }

                return result;
            }
        }
    }

    public class ReplicaEditorVM : Graph_BaseVM<ReplicaM>
    {
        public CollectionViewSource FilteredReplicaLocationCVS { get; }

        public ReplicaEditorVM(ReplicaVM viewModel, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
           Func<Notifier, ICallbackContext, Notifier> editorCreator, Type defaultNodeType) : base(viewModel.Model, callbackContext,
                modelCreator, viewModelCreator, editorCreator, defaultNodeType)
        {
            FilteredReplicaLocationCVS = new CollectionViewSource() { Source = ActiveContextService.Locations };

            if (FilteredReplicaLocationCVS.View != null)
            {
                FilteredReplicaLocationCVS.View.Filter = OnLocationFilter;
                FilteredReplicaLocationCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                FilteredReplicaLocationCVS.View.MoveCurrentTo(ReplicaLocation);
            }
        }

        protected override string CanLinkNodes(INodeVM from, INodeVM to) { return string.Empty; }

        private bool OnLocationFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(replicaLocationFilter) || model.PassFilter(replicaLocationFilter);
            }
            return false;
        }

        protected string replicaLocationFilter;
        public string ReplicaLocationFilter
        {
            set
            {
                if (value != replicaLocationFilter)
                {
                    replicaLocationFilter = value;
                    FilteredReplicaLocationCVS.View?.Refresh();
                }
            }
        }

        public BaseM ReplicaLocation
        {
            get => ActiveContextService.GetLocation(Model.locationId);
            set
            {
                if (value?.id != Model.locationId)
                {
                    Model.locationId = value?.id;
                    OnModelChanged(Model, nameof(ReplicaVM.ReplicaLocation));
                    Notify(nameof(ReplicaLocation));
                }
            }
        }

        public override string Title => null;

        public override string Stats => null;
    }
}