using StorylineEditor.ViewModel.Common;

namespace StorylineEditor.ViewModel.Nodes
{
    public class PlayerIndicatorVM : Notifier
    {
        public PlayerIndicatorVM() : base()
        {
            IsFilterPassed = true;

            zIndex = -20000;
        }

        public override string Id => null;

        protected double width;
        public double Width
        {
            get => width;
            set
            {
                if (width != value)
                {
                    width = value;
                    Notify(nameof(Width));
                }
            }
        }

        protected double height;
        public double Height
        {
            get => height;
            set
            {
                if (height != value)
                {
                    height = value;
                    Notify(nameof(Height));
                }
            }
        }

        protected double left;
        public double Left
        {
            get => left;
            set
            {
                if (left != value)
                {
                    left = value;
                    Notify(nameof(Left));
                }
            }
        }

        protected double top;
        public double Top
        {
            get => top;
            set
            {
                if (top != value)
                {
                    top = value;
                    Notify(nameof(Top));
                }
            }
        }

        protected int zIndex;
        public int ZIndex => zIndex;
    }
}
