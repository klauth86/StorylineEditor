/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Views.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StorylineEditor.Views.Controls
{
    public class IndicatorLink : BaseLink
    {
        public readonly Image InfoImage;

        protected bool? canLink;
        public bool? CanLink
        {
            get => canLink;
            set
            {
                if (canLink != value)
                {
                    canLink = value;

                    if (canLink == false)
                    {
                        InfoImage.Source = IconToImageSourceConverter.Convert(System.Drawing.SystemIcons.Error);
                        InfoImage.Visibility = Visibility.Visible;

                        LineAndArrow.Fill = Brushes.Red;
                        LineAndArrow.Stroke = Brushes.Red;
                    }
                    else if (canLink == true) ////// TODO
                    {
                        InfoImage.Source = IconToImageSourceConverter.Convert(System.Drawing.SystemIcons.Error);
                        InfoImage.Visibility = Visibility.Hidden;

                        LineAndArrow.Fill = Brushes.Green;
                        LineAndArrow.Stroke = Brushes.Green;
                    }
                    else if (canLink == null)
                    {
                        InfoImage.Visibility = Visibility.Hidden;

                        LineAndArrow.Fill = Brushes.Goldenrod;
                        LineAndArrow.Stroke = Brushes.Goldenrod;
                    }
                }
            }
        }

        public IndicatorLink(GraphNode from):base(from)
        {
            LineAndArrowThickness = 4;
            CanLink = null;

            InfoImage = new Image();
            InfoImage.Width = 16;
            InfoImage.Height = 16;
            InfoImage.Visibility = Visibility.Collapsed;

            InfoImage.RenderTransform = new MatrixTransform();
        }

        public void UpdateLayout(GraphNode to)
        {
            UpdateLayout(From.ActualWidth, From.ActualHeight, Canvas.GetLeft(From), Canvas.GetTop(From),
                to.ActualWidth, to.ActualHeight, Canvas.GetLeft(to), Canvas.GetTop(to));

            Matrix matrix = new Matrix();
            matrix.M11 = GetScaleX();
            matrix.M22 = GetScaleY();

            (InfoImage.RenderTransform as MatrixTransform).Matrix = matrix;
        }

        public void UpdateLayout(Point toPoint)
        {
            UpdateLayout(From.ActualWidth, From.ActualHeight, Canvas.GetLeft(From), Canvas.GetTop(From),
                0, 0, toPoint.X, toPoint.Y);

            Matrix matrix = new Matrix();
            matrix.M11 = GetScaleX();
            matrix.M22 = GetScaleY();

            (InfoImage.RenderTransform as MatrixTransform).Matrix = matrix;
        }

        protected override void PostUpdateLayout(Point fromPoint, Point toPoint)
        {
            var cx = (fromPoint.X + toPoint.X - InfoImage.ActualWidth * GetScaleX()) / 2;
            var cy = (fromPoint.Y + toPoint.Y - InfoImage.ActualHeight * GetScaleY()) / 2;
            Canvas.SetLeft(InfoImage, cx);
            Canvas.SetTop(InfoImage, cy);
        }
    }
}