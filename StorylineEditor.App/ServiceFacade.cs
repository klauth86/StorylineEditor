using StorylineEditor.App.FileService;
using StorylineEditor.App.SerializeService;

namespace StorylineEditor.App
{
    public static class ServiceFacade
    {
        public static readonly IFileService FileService;
        public static readonly ISerializeService SerializeService;

        static ServiceFacade()
        {
            FileService = new DefaultFileService();
            SerializeService = new DefaultSerializeService();
        }
    }
}
