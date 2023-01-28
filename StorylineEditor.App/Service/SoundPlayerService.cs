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
using System.Windows.Media;

namespace StorylineEditor.App.Service
{
    public class SoundPlayerService : ISoundPlayerService
    {
        private MediaPlayer _mediaPlayer;

        private long _lockIndex = 0;

        private Action _prePlayAction = null;
        private Action _postPlayAction = null;
        private Action<CustomStatus> _callbackAction = null;

        private CustomStatus _customStatus;

        public SoundPlayerService()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Volume = 1;

            _mediaPlayer.MediaFailed += OnMediaFailed;
            _mediaPlayer.MediaEnded += OnMediaEnded;

            _customStatus = CustomStatus.None;
            _isPaused = false;
        }

        private void OnMediaFailed(object sender, ExceptionEventArgs e)
        {
            _customStatus = CustomStatus.Faulted;

            OnPlayEnded();
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            if (_customStatus == CustomStatus.Running)
            {
                _customStatus = CustomStatus.RanToCompletion;
            }

            OnPlayEnded();
        }

        private void OnPlayEnded()
        {
            _postPlayAction?.Invoke();

            Interlocked.Decrement(ref _lockIndex);
            _callbackAction?.Invoke(_customStatus);

            _customStatus = CustomStatus.None;
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;

                    if (_isPaused)
                    {
                        _mediaPlayer.Pause();
                    }
                    else
                    {
                        _mediaPlayer.Play();
                    }
                }

            }
        }

        public async void Play(string sourceFilePath, Action prePlayAction, Action beforeFinishPlayingAction, Action<CustomStatus> callbackAction)
        {
            Stop();

            Interlocked.Add(ref _lockIndex, 1);

            while (Interlocked.Read(ref _lockIndex) > 1) await Task.Delay(2);

            _prePlayAction = prePlayAction;
            _postPlayAction = beforeFinishPlayingAction;
            _callbackAction = callbackAction;

            _customStatus = CustomStatus.WaitingToRun;

            try
            {
                _mediaPlayer.Open(new Uri(sourceFilePath));
                
                _mediaPlayer.Play();
                _customStatus = CustomStatus.Running;

                _prePlayAction?.Invoke();
            }
            catch
            {
                _customStatus = CustomStatus.Faulted;

                OnPlayEnded();
            }
        }

        public void Stop()
        {
            if (_customStatus == CustomStatus.Running)
            {
                _customStatus = CustomStatus.Canceled;
                
                _mediaPlayer.Stop();

                OnPlayEnded();
            }
        }

        public void Dispose()
        {
            Stop();

            _mediaPlayer.Close();
        }
    }
}