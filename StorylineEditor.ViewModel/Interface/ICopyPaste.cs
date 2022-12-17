namespace StorylineEditor.ViewModel.Interface
{
    public interface ICopyPaste
    {
        string Copy();
        void Paste(string clipboard);
        void Delete();
    }
}