using StorylineEditor.App.FileService;

namespace StorylineEditor.App
{
    public static class ServiceFacade
    {
        public static readonly IFileService FileService;

        static ServiceFacade()
        {
            FileService = new DefaultFileService();
        }
    }
}
