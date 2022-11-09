/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StorylineEditor.ViewModel
{
    public interface IActiveContext
    {
        string Copy();
        void Paste(string clipboard);
        void Delete();
    }

    public class ActiveContextService
    {
        public static IActiveContext ActiveContext { get; set; }
        public static StorylineVM ActiveStoryline { get; set; }
        public static IEnumerable<BaseM> GetEnumerator(params List<BaseM>[] collectionSet)
        {
            for (int i = 0; i < collectionSet.Length; i++)
            {
                foreach (var item in collectionSet[i])
                {
                    if (item is FolderM folder)
                    {
                        foreach (var contentItem in GetEnumerator(folder.content))
                        {
                            yield return contentItem;
                        }
                    }
                    else
                    {
                        yield return item;
                    }
                }
            }
        }

        public static IEnumerable<BaseM> Characters => GetEnumerator(ActiveStoryline?.Model.characters);
        public static BaseM GetCharacter(string id) => Characters?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> Items => GetEnumerator(ActiveStoryline?.Model.items);
        public static BaseM GetItem(string id) => Items?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> Actors => GetEnumerator(ActiveStoryline?.Model.actors);
        public static BaseM GetActor(string id) => Actors?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> DialogsAndReplicas => GetEnumerator(ActiveStoryline?.Model.dialogs, ActiveStoryline?.Model.replicas);
        public static BaseM GetDialogOrReplica(string id) => DialogsAndReplicas?.FirstOrDefault((model) => model.id == id);
    }
}