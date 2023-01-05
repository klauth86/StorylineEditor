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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Graphs
{
    public class GraphVM<T> : BaseVM<T> where T : GraphM
    {
        public GraphVM(T model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        protected ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((viewModel) => ActiveContextService.DlgService?.ShowDialog(this)));

        public string Stats => GetStats(Model);

        protected string GetStats(GraphM graphModel)
        {
            string result = "";

            Dictionary<string, Dictionary<string, int>> countByCharacter = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, int> countByType = new Dictionary<string, int>();

            int characterKeyMaxLength = 0;
            int typeKeyMaxLength = 0;

            foreach (var node in graphModel.nodes)
            {
                // Characters

                string characterKey = node is Node_RegularM regularNode ? ActiveContextService.GetCharacter(regularNode.characterId)?.name : "N/A";

                string gender = " ";
                if (node.gender == GENDER.MALE) gender = Application.Current.FindResource("String_Icon_Gender_Male")?.ToString();
                if (node.gender == GENDER.FEMALE) gender = Application.Current.FindResource("String_Icon_Gender_Female")?.ToString();

                if (!countByCharacter.ContainsKey(characterKey))
                {
                    countByCharacter.Add(characterKey, new Dictionary<string, int>()
                        {
                            { " ", 0 },
                            { Application.Current.FindResource("String_Icon_Gender_Male")?.ToString(), 0 },
                            { Application.Current.FindResource("String_Icon_Gender_Female")?.ToString(), 0 },
                        }
                    );
                    characterKeyMaxLength = Math.Max(characterKey?.Length ?? 0, characterKeyMaxLength);
                }

                countByCharacter[characterKey][gender]++;

                // Types

                var typeName = node.GetType().Name;
                var typeKey = Application.Current.FindResource(string.Format("String_Stats_{0}_TmpDescription", typeName))?.ToString();

                if (!countByType.ContainsKey(typeKey))
                {
                    countByType.Add(typeKey, 0);
                    typeKeyMaxLength = Math.Max(typeKey?.Length ?? 0, typeKeyMaxLength);
                }

                countByType[typeKey]++;
            }

            if (countByCharacter.Count > 0)
            {
                // Delimiter
                result += Environment.NewLine;

                result += Application.Current.FindResource("String_Stats_Characters")?.ToString() + Environment.NewLine;
                result += Environment.NewLine;

                foreach (var entry in countByCharacter.OrderBy(pair => pair.Key))
                {
                    result += string.Format("{0, -" + (characterKeyMaxLength + 6) + "}{1}",
                        "- " + entry.Key + ":",
                        string.Join("\t", entry.Value.Select(pair => string.Format("{0}{1, -6}", pair.Key, pair.Value)))
                        ) + Environment.NewLine;
                }
            }

            if (countByType.Count > 0)
            {
                // Delimiter
                result += Environment.NewLine;

                result += Application.Current.FindResource("String_Stats_Types")?.ToString() + Environment.NewLine;
                result += Environment.NewLine;

                foreach (var entry in countByType.OrderBy(pair => pair.Key))
                {
                    result += string.Format("{0, -" + (typeKeyMaxLength + 6) + "}{1}",
                        "- " + entry.Key + ": ",
                        entry.Value
                        ) + Environment.NewLine;
                }
            }

            return result;
        }
    }
}