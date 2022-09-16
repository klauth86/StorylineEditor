/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StorylineEditor.Views.Controls
{
    public class PlayingAdorner: ContentControl
    {
        Vector ActiveElementSize = new Vector(0, 0);

        public double PositionX { get; set; }
        public double PositionY { get; set; }

        public void Tick(long deltaTicks)
        {
            if (multiplier != 0)
            {
                stateTicks += multiplier * deltaTicks;

                if (multiplier < 0 && stateTicks < 0)
                {
                    multiplier = 0;
                    stateTicks = 0;
                }
                else if (multiplier > 0 && stateTicks > durationTicks)
                {
                    multiplier = 0;
                    stateTicks = durationTicks;
                }

                double stateAlpha = 1 - 1.0 * stateTicks / durationTicks;
                double mAlpha = stateAlpha * 1.25;
                double mBetta = (1 - stateAlpha) * 1.25;

                ellipseScaleTransform.ScaleX = ActiveElementSize.X / Width * mBetta + mAlpha;
                ellipseScaleTransform.ScaleY = ActiveElementSize.Y / Height * mBetta + mAlpha;
            }
        }

        System.Windows.Shapes.Ellipse ellipse = null;

        ScaleTransform ellipseScaleTransform = null;

        public PlayingAdorner(long inDurationTicks)
        {
            durationTicks = inDurationTicks;

            Width = 32;
            Height = 32;

            ContentControl internalContent = new ContentControl() { Width = 32, Height = 32 };
            internalContent.RenderTransform = new ScaleTransform() { CenterX = 16, CenterY = 16 };
            Content = internalContent;

            ellipse = new System.Windows.Shapes.Ellipse();
            ellipse.Fill = Brushes.Gold;
            ellipse.RenderTransform = ellipseScaleTransform = new ScaleTransform() { CenterX = 16, CenterY = 16 };

            internalContent.Content = ellipse;
        }

        long durationTicks = 0;
        long stateTicks = 0;
        long multiplier = 0;

        public void ToActiveNodeState(FrameworkElement activeElement)
        {
            ActiveElementSize.X = activeElement.ActualWidth;
            ActiveElementSize.Y = activeElement.ActualHeight;
            multiplier = 1;
        }

        public void ToTransitionState() { multiplier = -1; }
    }
}