/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.ViewModels;
using StorylineEditor.ViewModels.GameEvents;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Predicates;
using StorylineEditor.ViewModels.Tabs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace StorylineEditor
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected static List<Type> AddOnTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
            type == typeof(CharacterVm) ||
            type == typeof(ItemVm) ||
            type == typeof(LocationObjectVm) ||
            type == typeof(TreeVm) ||
            type == typeof(TreeFolderVm) ||
            type == typeof(CharactersTabVm) ||
            type == typeof(ItemsTabVm) ||
            type == typeof(LocationObjectsTabVm) ||
            type == typeof(JournalRecordsTabVm) ||
            type == typeof(PlayerDialogsTabVm) ||
            type == typeof(ReplicasTabVm) ||
            type.IsSubclassOf(typeof(P_BaseVm)) ||
            type.IsSubclassOf(typeof(GE_BaseVm)) ||
            type.IsSubclassOf(typeof(Node_BaseVm)) ||
            type.IsSubclassOf(typeof(BaseTreesTabVm)) ||
            type.IsSubclassOf(typeof(BaseM))).ToList();

        public static T DeserializeXml<T>(Stream stream) where T : class
        {
            XmlSerializer s = new XmlSerializer(typeof(T), AddOnTypes.ToArray<Type>());
            return s.Deserialize(stream) as T;
        }

        public static T DeserializeXmlFromString<T>(string xmlString) where T : class
        {
            using (MemoryStream ms  = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
            {
                return DeserializeXml<T>(ms);
            }
        }

        public static void SerializeXml<T>(Stream stream, T obj) where T : class
        {
            List<Type> addOnTypes = new List<Type>(AddOnTypes);

            var modelAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((ass) => ass.GetName().Name.Contains("StorylineEditor.Model"));
            addOnTypes.AddRange(modelAssembly.GetTypes().Where((type) => !type.IsSealed)); ////// We dont mark any of our class as sealed, so the only classes with this mark are static classes (marked automatically)

            using (var streamWriter = new StreamWriter(stream))
            {
                XmlSerializer s = new XmlSerializer(typeof(T), addOnTypes.ToArray<Type>());
                s.Serialize(streamWriter, obj);
            }
        }

        public static string SerializeXmlToString<T>(T obj) where T : class
        {
            MemoryStream memoryStream = new MemoryStream();
            App.SerializeXml<T>(memoryStream, obj);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    };
}