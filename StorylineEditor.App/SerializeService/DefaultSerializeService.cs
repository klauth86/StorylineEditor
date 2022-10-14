/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace StorylineEditor.App.SerializeService
{
    public class DefaultSerializeService : ISerializeService
    {
        private static Type[] addonTypes = 
            AppDomain.CurrentDomain.GetAssemblies().First((assembly) => assembly.GetName().Name.Contains("StorylineEditor.Model"))
            .GetTypes().Where((type) => !type.IsSealed).ToArray();
        
        ////// We dont mark any of our class as sealed, so the only classes with this mark are static classes (marked automatically)

        // Universal

        public void Serialize<T>(Stream stream, T obj)
        {
            XmlSerializer xmlSerializerInstance = new XmlSerializer(typeof(T), addonTypes);
            xmlSerializerInstance.Serialize(stream, obj);
        }

        public T Deserialize<T>(Stream stream)
        {
            XmlSerializer xmlSerializerInstance = new XmlSerializer(typeof(T), addonTypes);
            return (T)xmlSerializerInstance.Deserialize(stream);
        }

        // Clipboard

        public string Serialize<T>(T obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                Serialize(memoryStream, obj);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public T Deserialize<T>(string str)
        {
            T result = default;

            try
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
                {
                    result = Deserialize<T>(memoryStream);
                }
            }
            catch (Exception) { } ////// TODO

            return result;
        }
    }
}