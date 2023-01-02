﻿using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System.Windows;

namespace StorylineEditor.ViewModel.Nodes
{
    public class PlayerIndicatorVM : Notifier
    {
        public PlayerIndicatorVM(double paddingMultiplier, double sizeAlpha, double pulseAlpha) : base()
        {
            IsFilterPassed = true;

            _zIndex = -20000;

            _paddingMultiplier = paddingMultiplier;
            
            _sizeAlpha = sizeAlpha;

            _pulseAlpha = pulseAlpha;

            Hide();
        }

        public override string Id => null;

        protected double _width;
        public double Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    Notify(nameof(Width));
                }
            }
        }

        protected double _height;
        public double Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    Notify(nameof(Height));
                }
            }
        }

        protected double _left;
        public double Left
        {
            get => _left;
            set
            {
                if (_left != value)
                {
                    _left = value;
                    Notify(nameof(Left));
                }
            }
        }

        protected double _top;
        public double Top
        {
            get => _top;
            set
            {
                if (_top != value)
                {
                    _top = value;
                    Notify(nameof(Top));
                }
            }
        }

        protected int _zIndex;
        public int ZIndex => _zIndex;

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

        protected double _pulseAlpha;

        protected double _scale { get; set; }

        public bool IsVisible => _scale > 0;
        public void Show(double scale) => _scale = scale;
        public void Hide() => _scale = -1;

        public void Tick(double alpha)
        {
            if (PlayerContext != null)
            {
                double tmp = (alpha - _sizeAlpha);
                while (tmp > _sizeAlpha) tmp -= _sizeAlpha;
                tmp /= _sizeAlpha;

                double betta = tmp > 0.5 ? (2 - 2 * tmp) : (2 * tmp);

                double targetWidth = ((1 - betta) + 1.25 * betta) * TargetWidth;
                double targetHeight = ((1 - betta) + 1.25 * betta) * TargetHeight;

                if (alpha < CurrentSizeAlpha)
                {
                    betta = alpha / CurrentSizeAlpha;
                }
                else
                {
                    CurrentSizeAlpha = 0;
                    betta = 1;
                }

                Width = (1 - betta) * Width + betta * targetWidth;
                Height = (1 - betta) * Height + betta * targetHeight;

                Left = (StorylineVM.ViewWidth - Width * _scale) / 2;
                Top = (StorylineVM.ViewHeight - Height * _scale) / 2;
            }
        }
    }
}