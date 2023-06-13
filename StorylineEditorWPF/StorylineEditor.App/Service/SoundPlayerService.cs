/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StorylineEditor.Service
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