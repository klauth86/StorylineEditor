﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;
using System;
using System.Windows;
using System.Windows.Media;

namespace StorylineEditor.ViewModel.Nodes
{
    public interface ILinkVM
    {
        // Absoulute
        double FromX { get; set; }
        double FromY { get; set; }
        double ToX { get; set; }
        double ToY { get; set; }

        // Local
        double Left { get; set; }
        double Top { get; set; }
        double HandleX { get; set; }
        double HandleY { get; set; }

        void RefreshStepPoints();

        string Description { get; set; }
    }

    public class LinkVM : BaseVM<LinkM>, ILinkVM
    {
        const int N = 12;

        public LinkVM(LinkM model, ICallbackContext callbackContext, double step = 64, double cap = 8, double remove = 12) : base(model, callbackContext)
        {
            Step = step;
            Cap = cap;
            Remove = remove;

            _coss = new double[N];
            _sins = new double[N];
            
            for (int i = 0; i < N; i++)
            {
                _coss[i] = Math.Cos(2 * i * Math.PI / N);
                _sins[i] = Math.Sin(2 * i * Math.PI / N);
            }

            zIndex = 0;
        }

        public readonly double Step;        
        public readonly double Cap;
        public readonly double Remove;
        private readonly double _cos30;
        private readonly double _sin30;
        private readonly double[] _coss;
        private readonly double[] _sins;

        public double FromX { get; set; }
        public double FromY { get; set; }
        public double ToX { get; set; }
        public double ToY { get; set; }

        private double _left;
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

        private double _top;
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

        private double _handleX;
        public double HandleX
        {
            get => _handleX;
            set
            {
                if (_handleX != value)
                {
                    _handleX = value;
                    Notify(nameof(HandleX));
                }
            }
        }

        private double _handleY;
        public double HandleY
        {
            get => _handleY;
            set
            {
                if (_handleY != value)
                {
                    _handleY = value;
                    Notify(nameof(HandleY));
                }
            }
        }

        public void RefreshStepPoints()
        {
            double norm2 = HandleX * HandleX + HandleY * HandleY;

            if (norm2 > 0)
            {
                double remainingLength = Math.Sqrt(norm2);

                double dirX = HandleX / remainingLength;
                double dirY = HandleY / remainingLength;

                double dxFwd = dirX * _cos30 + dirY * _sin30;
                double dyFwd = -dirX * _sin30 + dirY * _cos30;

                double dxBwd = dirX * _cos30 - dirY * _sin30;
                double dyBwd = dirX * _sin30 + dirY * _cos30;

                int stepCount = 1;
                
                double actualStep = remainingLength;
                while (actualStep > Step)
                {
                    actualStep /= 2;
                    stepCount *= 2;
                }

                _stepPoints = new PointCollection(4 * stepCount);

                while (remainingLength >= actualStep)
                {
                    _stepPoints.Add(new Point(remainingLength * dirX, remainingLength * dirY));
                    _stepPoints.Add(new Point(remainingLength * dirX - dxFwd, remainingLength * dirY - dyFwd));
                    _stepPoints.Add(new Point(remainingLength * dirX - dxBwd, remainingLength * dirY - dyBwd));
                    _stepPoints.Add(new Point(remainingLength * dirX, remainingLength * dirY));

                    remainingLength -= actualStep;
                }

                _stepPoints.Add(new Point(0, 0));

                double centerX = HandleX / 2;
                double centerY = HandleY / 2;

                _removePoints = new PointCollection(N);

                for (int i = 0; i < N; i++)
                {
                    _removePoints.Add(new Point(centerX + Remove*(_coss[i] * dirX + _sins[i] * dirY), centerY + Remove * (- _sins[i] * dirX + _coss[i] * dirY)));
                }

                _removePoints.Add(_removePoints[0]);

                Notify(nameof(StepPoints));
                Notify(nameof(RemovePoints));
                Notify(nameof(CrossPoints));
            }
        }

        private PointCollection _stepPoints;
        public PointCollection StepPoints => _stepPoints;

        private PointCollection _removePoints;
        public PointCollection RemovePoints => _removePoints;

        private PointCollection _crossPoints;
        public PointCollection CrossPoints => _crossPoints;

        protected int zIndex;
        public int ZIndex => zIndex;
    }

    public class PreviewLinkVM : LinkVM {
        public PreviewLinkVM(LinkM model, ICallbackContext callbackContext, double step = 64, double cap = 8) : base(model, callbackContext, step, cap)
        {
            zIndex = 10000;
        }
    }
}