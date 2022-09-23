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
    public class PlayingAdorner : ContentControl
    {
        Vector ActiveElementSize = new Vector(0, 0);

        Storyboard StateStoryboard = new Storyboard();

        Storyboard IndicateStoryboard = new Storyboard();

        bool IsGrowing = true;

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
                scaleTransform.ScaleX = playingAdorner.ActiveElementSize.X / playingAdorner.Width * mAlpha + mBetta;
                scaleTransform.ScaleY = playingAdorner.ActiveElementSize.Y / playingAdorner.Height * mAlpha + mBetta;

                if (playingAdorner.StateAlpha < 0.05 && !playingAdorner.IsGrowing ||
                    playingAdorner.StateAlpha > 0.95 && playingAdorner.IsGrowing)
                {
                    playingAdorner.StateStoryboard.Pause(playingAdorner);
                }
            }
        }

        System.Windows.Shapes.Ellipse ellipse;

        public PlayingAdorner()
        {
            Width = 32;
            Height = 32;

            ContentControl internalContent = new ContentControl() { Width = 32, Height = 32 };
            internalContent.RenderTransform = new ScaleTransform() { CenterX = 16, CenterY = 16 };
            Content = internalContent;

            ellipse = new System.Windows.Shapes.Ellipse();
            ellipse.Fill = Brushes.Gold;
            ellipse.RenderTransform = new ScaleTransform() { CenterX = 16, CenterY = 16 };

            internalContent.Content = ellipse;

            // Indicate anim

            var scaleXAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.5,
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

            var scaleYAnimation = new DoubleAnimation
            {
                From = 1.5,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

            IndicateStoryboard.Children.Add(scaleXAnimation);
            IndicateStoryboard.Children.Add(scaleYAnimation);
            
            IndicateStoryboard.Begin(internalContent, true);

            // State anim

            var stateTransitionAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true,
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTargetProperty(stateTransitionAnimation, new PropertyPath("StateAlpha"));

            StateStoryboard.Children.Add(stateTransitionAnimation);

            StateStoryboard.Begin(this, true);
            StateStoryboard.Pause(this);

            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;

            StateStoryboard.Stop(this);
            StateStoryboard.Remove(this);

            if (Content is FrameworkElement frameworkElement)
            {
                IndicateStoryboard.Stop(frameworkElement);
                IndicateStoryboard.Remove(frameworkElement);
            }
        }

        public void ToStateView(FrameworkElement activeElement, double duration)
        {
            ActiveElementSize.X = activeElement.ActualWidth;
            ActiveElementSize.Y = activeElement.ActualHeight;

            IsGrowing = true;

            StateStoryboard.SetSpeedRatio(this, 1 / duration);
            StateStoryboard.Resume(this);
        }

        public void ToTransitionView(double duration)
        {
            IsGrowing = false;

            StateStoryboard.SetSpeedRatio(this, 1 / duration);
            StateStoryboard.Resume(this);
        }
    }
}