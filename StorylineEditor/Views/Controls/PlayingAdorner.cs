using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace StorylineEditor.Views.Controls
{
    public class PlayingAdorner: ContentControl
    {
        FrameworkElement FromElement = null;
        FrameworkElement ToElement = null;

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
                    double fromX = Canvas.GetLeft(playingAdorner.FromElement) + playingAdorner.FromElement.ActualWidth/2;
                    double toX = Canvas.GetLeft(playingAdorner.ToElement) + playingAdorner.ToElement.ActualWidth/2;

                    double fromY = Canvas.GetTop(playingAdorner.FromElement) + playingAdorner.FromElement.ActualHeight / 2;
                    double toY = Canvas.GetTop(playingAdorner.ToElement) + playingAdorner.ToElement.ActualHeight / 2;

                    double alpha = (double)e.NewValue;
                    double x = fromX * (1 - alpha) + toX * alpha;
                    double y = fromY * (1 - alpha) + toY * alpha;

                    Canvas.SetLeft(playingAdorner, x - playingAdorner.ActualWidth / 2);
                    Canvas.SetTop(playingAdorner, y - playingAdorner.ActualHeight / 2);
                }
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
            (Content as System.Windows.Shapes.Ellipse).Width = 64;
            (Content as System.Windows.Shapes.Ellipse).Height = 64;
        }

        public void ToPlayForm(FrameworkElement activeElement)
        {

        }

        public void StartTransition(FrameworkElement fromElement, FrameworkElement toElement)
        {
            FromElement = fromElement;
            ToElement = toElement;

            var transitionAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000)
            };

            transitionAnimation.Completed += transitionAnimationCompleted;

            Storyboard.SetTarget(transitionAnimation, this);
            Storyboard.SetTargetProperty(transitionAnimation, new PropertyPath("TransitionAlpha"));

            var storyboard = new Storyboard();
            storyboard.Children.Add(transitionAnimation);
            storyboard.Begin();
        }

        private void transitionAnimationCompleted(object sender, EventArgs e)
        {
            if (sender is DoubleAnimation transitionAnimation)
            {
                transitionAnimation.Completed -= transitionAnimationCompleted;
            }
        }
    }
}