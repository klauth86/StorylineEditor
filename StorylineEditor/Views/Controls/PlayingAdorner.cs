/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StorylineEditor.Views.Controls
{
    public class PlayingAdorner: ContentControl
    {
        Vector ActiveElementSize = new Vector(0, 0);

        public double PositionX { get; set; }
        public double PositionY { get; set; }

        public double StateAlpha
        {
            get => (double)GetValue(StateAlphaProperty);
            set { SetValue(StateAlphaProperty, value); }
        }

        public static readonly DependencyProperty StateAlphaProperty = DependencyProperty.Register(
            "StateAlpha", typeof(double), typeof(PlayingAdorner), new PropertyMetadata(0.0, OnStateAlphaChanged));

        private static void OnStateAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlayingAdorner playingAdorner)
            {
                double mAlpha = playingAdorner.StateAlpha * 1.25;
                double mBetta = (1 - playingAdorner.StateAlpha) * 1.25;

                ScaleTransform scaleTransform = (ScaleTransform)playingAdorner.ellipse.RenderTransform;
                scaleTransform.ScaleX = playingAdorner.ActiveElementSize.X / playingAdorner.Width * mBetta + playingAdorner.StateAlpha * mAlpha;
                scaleTransform.ScaleY = playingAdorner.ActiveElementSize.Y / playingAdorner.Height * mBetta + playingAdorner.StateAlpha * mAlpha;
            }
        }

        System.Windows.Shapes.Ellipse ellipse;

        public PlayingAdorner(double inScale)
        {
            RenderTransform = new ScaleTransform() { ScaleX = inScale, ScaleY = inScale };

            Width = 32;
            Height = 32;

            ContentControl internalContent = new ContentControl() { Width = 32, Height = 32 };
            internalContent.RenderTransform = new ScaleTransform() { CenterX = 16, CenterY = 16 };
            Content = internalContent;

            ellipse = new System.Windows.Shapes.Ellipse();
            ellipse.Fill = Brushes.Gold;
            ellipse.RenderTransform = new ScaleTransform() { CenterX = 16, CenterY = 16 };

            internalContent.Content = ellipse;
            {
                var scaleXAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 1.5,
                    Duration = TimeSpan.FromSeconds(0.5),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };

                Storyboard.SetTarget(scaleXAnimation, internalContent);
                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

                var storyboard = new Storyboard();
                storyboard.Children.Add(scaleXAnimation);
                Dispatcher.BeginInvoke(new Action(() => { storyboard.Begin(); }));
            }
            {
                var scaleyAnimation = new DoubleAnimation
                {
                    From = 1.5,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };

                Storyboard.SetTarget(scaleyAnimation, internalContent);
                Storyboard.SetTargetProperty(scaleyAnimation, new PropertyPath("RenderTransform.ScaleY"));

                var storyboard = new Storyboard();
                storyboard.Children.Add(scaleyAnimation);
                Dispatcher.BeginInvoke(new Action(() => { storyboard.Begin(); }));
            }
        }

        public void ToActiveNodeState(FrameworkElement activeElement, double duration)
        {
            ActiveElementSize.X = activeElement.ActualWidth;
            ActiveElementSize.Y = activeElement.ActualHeight;
            ToState(1, 0, duration);
        }

        public void ToTransitionState(double duration) { ToState(0, 1, duration); }

        private void ToState(double from, double to, double duration)
        {
            var stateTransitionAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(duration)
            };

            Storyboard.SetTarget(stateTransitionAnimation, this);
            Storyboard.SetTargetProperty(stateTransitionAnimation, new PropertyPath("StateAlpha"));

            var storyboard = new Storyboard();
            storyboard.Children.Add(stateTransitionAnimation);
            Dispatcher.BeginInvoke(new Action(() => { storyboard.Begin(); }));
        }
    }
}