using StorylineEditor.ViewModel.Interface;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace StorylineEditor.App.Service
{
    public class SerializationService : ISerializationService
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