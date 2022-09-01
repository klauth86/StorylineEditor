/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StorylineEditor.Views.Controls
{
    public class PlayingAdorner: ContentControl
    {
        public TreeVm TreeToPlay { get; set; }

        Storyboard storyboard = null;

        FrameworkElement ActiveElement = null;

        FrameworkElement FromElement = null;
        FrameworkElement ToElement = null;

        const double transitionTime = 1;

        public double TransitionAlpha
        {
            get => (double)GetValue(TransitionAlphaProperty);
            set { SetValue(TransitionAlphaProperty, value); }
        }

        public static readonly DependencyProperty TransitionAlphaProperty = DependencyProperty.Register(
            "TransitionAlpha", typeof(double), typeof(PlayingAdorner), new PropertyMetadata(0.0, OnTransitionAlphaChanged));

        private static void OnTransitionAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlayingAdorner playingAdorner)
            {
                if (playingAdorner.FromElement != null && playingAdorner.ToElement != null)
                {
                    double fromX = Canvas.GetLeft(playingAdorner.FromElement) + playingAdorner.FromElement.ActualWidth * playingAdorner.scale / 2;
                    double toX = Canvas.GetLeft(playingAdorner.ToElement) + playingAdorner.ToElement.ActualWidth * playingAdorner.scale / 2;

                    double fromY = Canvas.GetTop(playingAdorner.FromElement) + playingAdorner.FromElement.ActualHeight * playingAdorner.scale / 2;
                    double toY = Canvas.GetTop(playingAdorner.ToElement) + playingAdorner.ToElement.ActualHeight * playingAdorner.scale / 2;

                    double alpha = (double)e.NewValue;
                    double x = fromX * (1 - alpha) + toX * alpha;
                    double y = fromY * (1 - alpha) + toY * alpha;

                    Canvas.SetLeft(playingAdorner, x - playingAdorner.Width * playingAdorner.scale / 2);
                    Canvas.SetTop(playingAdorner, y - playingAdorner.Height * playingAdorner.scale / 2);

                    playingAdorner.TreeToPlay.ActiveTimeLeft = (1 - alpha) * transitionTime;
                }
            }
        }

        public double ActiveTime
        {
            get => (double)GetValue(ActiveTimeProperty);
            set { SetValue(ActiveTimeProperty, value); }
        }

        public static readonly DependencyProperty ActiveTimeProperty = DependencyProperty.Register(
            "ActiveTime", typeof(double), typeof(PlayingAdorner), new PropertyMetadata(0.0, OnActiveTimeChanged));

        private static void OnActiveTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlayingAdorner playingAdorner)
            {
                playingAdorner.TreeToPlay.ActiveTimeLeft = (double)e.NewValue;
            }
        }

        System.Windows.Shapes.Ellipse ellipse;

        double scale;

        public PlayingAdorner(double inScale)
        {
            scale = inScale;
            RenderTransform = new ScaleTransform() { ScaleX = inScale, ScaleY = inScale };

            Width = 32;
            Height = 32;

            ellipse = new System.Windows.Shapes.Ellipse();
            ellipse.Fill = Brushes.Gold;
            ellipse.RenderTransform = new ScaleTransform() { CenterX = 16, CenterY = 16 };

            Content = ellipse;

            ////var activeAnimation = new DoubleAnimation
            ////{
            ////    From = 1,
            ////    To = 1.25,
            ////    Duration = TimeSpan.FromSeconds(0.25),
            ////    RepeatBehavior = RepeatBehavior.Forever,
            ////    AutoReverse = true
            ////};

            ////Storyboard.SetTarget(activeAnimation, ellipse);
            ////Storyboard.SetTargetProperty(activeAnimation, new PropertyPath("RenderTransform.ScaleX"));

            ////var scaleStoryboard = new Storyboard();
            ////scaleStoryboard.Children.Add(activeAnimation);
            ////Dispatcher.BeginInvoke(new Action(() => scaleStoryboard.Begin()));
        }

        public void ToTransitionForm()
        {
            AnimateWidthTo(64);
            AnimateHeightTo(64);
        }

        public void StartActiveNode(FrameworkElement activeElement, double activeTime)
        {
            ActiveElement = activeElement;

            double fromX = Canvas.GetLeft(ActiveElement) + ActiveElement.ActualWidth * scale / 2;
            double toX = Canvas.GetTop(ActiveElement) + ActiveElement.ActualHeight * scale / 2;

            AnimateWidthTo(activeElement.ActualWidth * 1.25);
            AnimateHeightTo(activeElement.ActualWidth * 1.25);

            Canvas.SetLeft(this, fromX - Width * scale / 2);
            Canvas.SetTop(this, toX - Height * scale / 2);

            ActiveTime = activeTime;

            var activeAnimation = new DoubleAnimation
            {
                From = activeTime,
                To = 0,
                Duration = TimeSpan.FromSeconds(activeTime)
            };

            Storyboard.SetTarget(activeAnimation, this);
            Storyboard.SetTargetProperty(activeAnimation, new PropertyPath("ActiveTime"));

            storyboard = new Storyboard();
            storyboard.Children.Add(activeAnimation);
            storyboard.Completed += OnCompleted_EndActiveNode;

            Dispatcher.BeginInvoke(new Action(() => { storyboard.Begin(); TreeToPlay.IsPlaying = true; }));
        }

        private void OnCompleted_EndActiveNode(object sender, EventArgs e)
        {
            storyboard.Completed -= OnCompleted_EndActiveNode;
            TreeToPlay?.OnEndActiveNode();
        }

        public void StartTransition(FrameworkElement fromElement, FrameworkElement toElement)
        {
            FromElement = fromElement;
            ToElement = toElement;

            var transitionAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(transitionTime)
            };

            Storyboard.SetTarget(transitionAnimation, this);
            Storyboard.SetTargetProperty(transitionAnimation, new PropertyPath("TransitionAlpha"));

            storyboard = new Storyboard();
            storyboard.Children.Add(transitionAnimation);
            storyboard.Completed += OnCompleted_EndTransition;

            Dispatcher.BeginInvoke(new Action(() => { storyboard.Begin(); TreeToPlay.IsPlaying = true; }));
        }

        private void OnCompleted_EndTransition(object sender, EventArgs e)
        {
            storyboard.Completed -= OnCompleted_EndTransition;
            TreeToPlay?.OnEndTransition(ToElement.DataContext);
        }

        public void PauseUnpause()
        {
            if (storyboard != null)
            {
                if (!storyboard.GetIsPaused())
                {
                    storyboard.Pause();
                    TreeToPlay.IsPlaying = false;
                }
                else
                {
                    storyboard.Resume();
                    TreeToPlay.IsPlaying = true;
                }
            }
        }

        public void Stop()
        {
            if (storyboard != null)
            {
                storyboard.Stop();
                storyboard.Completed -= OnCompleted_EndActiveNode;
                storyboard.Completed -= OnCompleted_EndTransition;
                
                storyboard = null;
            }
        }

        private void AnimateHeightTo(double width)
        {
            ((ScaleTransform)ellipse.RenderTransform).ScaleX = width / Width;

            //if (GetValue(WidthProperty) == DependencyProperty.UnsetValue || double.IsNaN(Width))
            //{
            //    Width = width;
            //}
            //else
            //{
            //    var activeAnimation = new DoubleAnimation
            //    {
            //        From = Width,
            //        To = width,
            //        Duration = TimeSpan.FromSeconds(0.2),
            //    };

            //    Storyboard.SetTarget(activeAnimation, this);
            //    Storyboard.SetTargetProperty(activeAnimation, new PropertyPath("Width"));

            //    var widthStoryboard = new Storyboard();
            //    widthStoryboard.Children.Add(activeAnimation);
            //    Dispatcher.BeginInvoke(new Action(() => widthStoryboard.Begin()));
            //}
        }

        private void AnimateWidthTo(double height)
        {
            ((ScaleTransform)ellipse.RenderTransform).ScaleY = height / Height;

            //if (GetValue(HeightProperty) == DependencyProperty.UnsetValue || double.IsNaN(Height))
            //{
            //    Height = height;
            //}
            //else
            //{
            //    var activeAnimation = new DoubleAnimation
            //    {
            //        From = Height,
            //        To = height,
            //        Duration = TimeSpan.FromSeconds(0.2),
            //    };

            //    Storyboard.SetTarget(activeAnimation, this);
            //    Storyboard.SetTargetProperty(activeAnimation, new PropertyPath("Height"));

            //    var heightStoryboard = new Storyboard();
            //    heightStoryboard.Children.Add(activeAnimation);
            //    Dispatcher.BeginInvoke(new Action(() => heightStoryboard.Begin()));
            //}
        }
    }
}