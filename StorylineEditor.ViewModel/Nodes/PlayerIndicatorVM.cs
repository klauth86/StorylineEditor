using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System.Windows;

namespace StorylineEditor.ViewModel.Nodes
{
    public class PlayerIndicatorVM : Notifier
    {
        public PlayerIndicatorVM(double paddingMultiplier, double sizeAlpha) : base()
        {
            IsFilterPassed = true;

            zIndex = -20000;

            _paddingMultiplier = paddingMultiplier;
            _sizeAlpha = sizeAlpha;
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

        protected readonly double _paddingMultiplier;

        protected object _playerContext;
        public object PlayerContext
        {
            get => _playerContext;
            set
            {
                if (_playerContext != value)
                {
                    _playerContext = value;

                    if (_playerContext is INode node)
                    {
                        _targetWidth = node.Width * _paddingMultiplier;
                        _targetHeight = node.Height * _paddingMultiplier;
                        CurrentSizeAlpha = _sizeAlpha;
                    }
                    else
                    {
                        _targetWidth = (double)Application.Current.FindResource("Double_Size_16x");
                        _targetHeight = (double)Application.Current.FindResource("Double_Size_16x");
                        CurrentSizeAlpha = _sizeAlpha;
                    }
                }
            }
        }

        protected double _targetWidth;
        public double TargetWidth => _targetWidth;

        protected double _targetHeight;
        public double TargetHeight => _targetHeight;

        protected double _sizeAlpha;
        public double CurrentSizeAlpha { get; set; }

        public double Scale { get; set; }

        public void Tick(double alpha)
        {
            if (PlayerContext != null)
            {
                if (alpha < CurrentSizeAlpha)
                {
                    double betta = alpha / CurrentSizeAlpha;

                    Width = (1 - betta) * Width + betta * TargetWidth;
                    Height = (1 - betta) * Height + betta * TargetHeight;

                    Left = (StorylineVM.ViewWidth - Width * Scale) / 2;
                    Top = (StorylineVM.ViewHeight - Height * Scale) / 2;
                }
                else
                {
                    CurrentSizeAlpha = 0;

                    double tmp = (alpha - _sizeAlpha);
                    while (tmp > _sizeAlpha) tmp -= _sizeAlpha;
                    tmp /= _sizeAlpha;

                    double betta = tmp > 0.5 ? (2 - 2 * tmp) : (2 * tmp);

                    Width = ((1 - betta) + 1.25 * betta) * TargetWidth;
                    Height = ((1 - betta) + 1.25 * betta) * TargetHeight;

                    Left = (StorylineVM.ViewWidth - Width * Scale) / 2;
                    Top = (StorylineVM.ViewHeight - Height * Scale) / 2;
                }
            }
        }
    }
}