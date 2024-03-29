﻿/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System.Windows;

namespace StorylineEditor.ViewModel.Nodes
{
    public class PlayerIndicatorVM : Notifier
    {
        public PlayerIndicatorVM(double paddingMultiplier, double sizeChangeDurationMsec, double pulseDurationMsec) : base()
        {
            IsFilterPassed = true;

            _zIndex = -20000;

            _paddingMultiplier = paddingMultiplier;

            _sizeChangeDurationMsec = sizeChangeDurationMsec;

            _pulseDurationMsec = pulseDurationMsec;

            IsVisible = false;
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
                        _sizeChangedDurationMsecLeft = _sizeChangeDurationMsec;
                    }
                    else
                    {
                        _targetWidth = (double)Application.Current.FindResource("Double_Size_16x");
                        _targetHeight = (double)Application.Current.FindResource("Double_Size_16x");
                        _sizeChangedDurationMsecLeft = _sizeChangeDurationMsec;
                    }
                }
            }
        }

        protected double _targetWidth;
        public double TargetWidth => _targetWidth;

        protected double _targetHeight;
        public double TargetHeight => _targetHeight;

        protected double _sizeChangeDurationMsec;

        protected double _sizeChangedDurationMsecLeft;

        protected double _pulseDurationMsec;

        protected double _pulseTimeMsec;

        public bool IsVisible { get; set; }

        public void Tick(double deltaTimeMsec)
        {
            if (PlayerContext != null)
            {
                double sizeAlpha = 1;

                if (_sizeChangedDurationMsecLeft > 0)
                {
                    _sizeChangedDurationMsecLeft -= deltaTimeMsec;

                    if (_sizeChangedDurationMsecLeft > 0)
                    {
                        sizeAlpha = 1 - _sizeChangedDurationMsecLeft / _sizeChangeDurationMsec;
                    }
                }

                _pulseTimeMsec += deltaTimeMsec;

                if (_pulseTimeMsec > _pulseDurationMsec)
                {
                    _pulseTimeMsec -= _pulseDurationMsec;
                }

                double pulseX = _pulseTimeMsec / _pulseDurationMsec;
                pulseX = pulseX - 0.5;
                double pulseAlpha = 1 - 4 * pulseX * pulseX;
                double targetMultiplier = pulseAlpha * 0.25 + 1;

                double targetWidth = TargetWidth * targetMultiplier;
                double targetHeight = TargetHeight * targetMultiplier;

                Width = (1 - sizeAlpha) * Width + sizeAlpha * targetWidth;
                Height = (1 - sizeAlpha) * Height + sizeAlpha * targetHeight;

                Left = (ActiveContext.ViewWidth - Width) / 2;
                Top = (ActiveContext.ViewHeight - Height) / 2;
            }
        }
    }
}