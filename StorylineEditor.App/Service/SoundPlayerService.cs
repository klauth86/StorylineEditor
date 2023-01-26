/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StorylineEditor.App.Service
{
    public class SoundPlayerService : ISoundPlayerService
    {
        private MediaElement _mediaElement;

        private long _lockIndex = 0;

        private Action<CustomStatus> _callbackAction = null;

        private CustomStatus _customStatus;

        private bool _isPaused = false;

        public SoundPlayerService()
        {
            _mediaElement = new MediaElement();
            _mediaElement.LoadedBehavior = MediaState.Manual;
            _mediaElement.MediaFailed += OnMediaFailed;
            _mediaElement.MediaEnded += OnMediaEnded;
        }

        private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _customStatus = CustomStatus.Faulted;
            Finish();
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            _customStatus = CustomStatus.RanToCompletion;
            Finish();
        }

        private void Finish()
        {
            Interlocked.Decrement(ref _lockIndex);
            _callbackAction?.Invoke(_customStatus);
        }

        public void Stop()
        {
            _customStatus = CustomStatus.Canceled;
            _mediaElement.Stop();
        }

        public void SetIsPaused(bool isPaused)
        {
            _isPaused = isPaused;
            
            if (isPaused)
            {
                _mediaElement.Pause();
            }
            else
            {
                _mediaElement.Play();
            }
        }

        public async void Play(string sourceFilePath, Action<CustomStatus> callbackAction)
        {
            Stop();

            Interlocked.Add(ref _lockIndex, 1);

            while (Interlocked.Read(ref _lockIndex) > 1)
            {
                await Task.Delay(2);
            }

            _callbackAction = callbackAction;

            _customStatus = CustomStatus.WaitingToRun;

            try
            {
                _mediaElement.Source = new Uri("sourceFilePath");
                _mediaElement.Play();
                _customStatus = CustomStatus.Running;
            }
            catch (Exception exc)
            {
                _customStatus = CustomStatus.Faulted;
                Finish();
            }
        }
    }
}