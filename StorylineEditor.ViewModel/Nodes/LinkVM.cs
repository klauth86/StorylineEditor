/*
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

    public class LinkVM : BaseVM<LinkM>
    {
        public LinkVM(LinkM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public sealed class PreviewLinkVM : SimpleVM<object>, ILinkVM
    {
        public PreviewLinkVM(ICallbackContext callbackContext) : base(new object(), callbackContext) { }

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
                System.Diagnostics.Trace.WriteLine("!!! " + HandleX);

                double norm2root = Math.Sqrt(norm2);

                double dirX = HandleX / norm2root;
                double dirY = HandleY / norm2root;

                double cos30 = Math.Cos(Math.PI/6);
                double sin30 = Math.Sin(Math.PI / 6);

                double dxFwd = dirX * cos30 + dirY * sin30;
                double dyFwd = -dirX * sin30 + dirY * cos30;

                double dxBwd = dirX * cos30 - dirY * sin30;
                double dyBwd = dirX * sin30 + dirY * cos30;

                double step = norm2root;
                while (step > 64) step /= 2;

                _stepPoints = new PointCollection();

                double start = norm2root;
                while (start >= step)
                {
                    _stepPoints.Add(new Point(start * dirX, start * dirY));
                    _stepPoints.Add(new Point(start * dirX - 8 * dxFwd, start * dirY - 8 * dyFwd));
                    _stepPoints.Add(new Point(start * dirX - 8 * dxBwd, start * dirY - 8 * dyBwd));
                    _stepPoints.Add(new Point(start * dirX, start * dirY));

                    start -= step;
                }

                Notify(nameof(StepPoints));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    Notify(nameof(Description));
                }
            }
        }

        protected PointCollection _stepPoints;
        public PointCollection StepPoints => _stepPoints;
    }
}