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
                    double fromX = Canvas.GetLeft(playingAdorner.FromElement) + playingAdorner.FromElement.ActualWidth / 2;
                    double toX = Canvas.GetLeft(playingAdorner.ToElement) + playingAdorner.ToElement.ActualWidth / 2;

                    double fromY = Canvas.GetTop(playingAdorner.FromElement) + playingAdorner.FromElement.ActualHeight / 2;
                    double toY = Canvas.GetTop(playingAdorner.ToElement) + playingAdorner.ToElement.ActualHeight / 2;

                    double alpha = (double)e.NewValue;
                    double x = fromX * (1 - alpha) + toX * alpha;
                    double y = fromY * (1 - alpha) + toY * alpha;

                    Canvas.SetLeft(playingAdorner, x - playingAdorner.ActualWidth / 2);
                    Canvas.SetTop(playingAdorner, y - playingAdorner.ActualHeight / 2);

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

        public PlayingAdorner()
        {
            var ellipse = new System.Windows.Shapes.Ellipse();

            ellipse.Fill = Brushes.Gold;

            Content = ellipse;
        }

        public void ToTransitionForm()
        {
            Width = 64;
            Height = 64;
        }

        public void StartActiveNode(FrameworkElement activeElement, double activeTime)
        {
            ActiveElement = activeElement;

            double fromX = Canvas.GetLeft(ActiveElement) + ActiveElement.ActualWidth / 2;
            double toX = Canvas.GetTop(ActiveElement) + ActiveElement.ActualHeight / 2;

            Width = activeElement.ActualWidth * 1.25;
            Height = activeElement.ActualWidth * 1.25;

            Canvas.SetLeft(this, fromX - Width / 2);
            Canvas.SetTop(this, toX - Height / 2);

            ActiveTime = activeTime;

            var activeAnimation = new DoubleAnimation
            {
                From = activeTime,
                To = 0,
                Duration = TimeSpan.FromSeconds(activeTime)
            };

            activeAnimation.Completed += (o, e) => TreeToPlay?.OnEndActiveNode();

            Storyboard.SetTarget(activeAnimation, this);
            Storyboard.SetTargetProperty(activeAnimation, new PropertyPath("ActiveTime"));

            storyboard = new Storyboard();
            storyboard.Children.Add(activeAnimation);
            Dispatcher.BeginInvoke(new Action(() => storyboard.Begin()));
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

            transitionAnimation.Completed += (o, e) => TreeToPlay?.OnEndTransition();

            Storyboard.SetTarget(transitionAnimation, this);
            Storyboard.SetTargetProperty(transitionAnimation, new PropertyPath("TransitionAlpha"));

            storyboard = new Storyboard();
            storyboard.Children.Add(transitionAnimation);
            Dispatcher.BeginInvoke(new Action(() => storyboard.Begin()));
        }

        public void PauseUnpause(bool isPaused)
        {
            if (storyboard != null)
            {
                if (storyboard.GetIsPaused() != isPaused)
                {
                    if (isPaused)
                        storyboard.Pause();
                    else
                        storyboard.Resume();
                }
            }
        }

        public void Stop()
        {
            if (storyboard != null)
            {
                storyboard.Stop();
                storyboard = null;
            }
        }
    }
}