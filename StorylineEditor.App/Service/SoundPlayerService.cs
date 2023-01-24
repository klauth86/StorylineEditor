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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StorylineEditor.App.Service
{
    public class SoundPlayerService : ISoundPlayerService
    {
        private static MediaElement _mediaElement = new MediaElement();

        private static long _lockIndex = 0;

        private static bool _isPaused = false;

        public void Stop()
        {
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

        public async void Play(string sourceFilePath, Action successCallback, Action failureCallback)
        {
            Stop();

            Interlocked.Add(ref _lockIndex, 1);

            while (Interlocked.Read(ref _lockIndex) > 1)
            {
                await Task.Delay(2);
            }

            try
            {
                _mediaElement.Source = new Uri("sourceFilePath");
                _mediaElement.Play();
            }
            finally
            {
                Interlocked.Decrement(ref _lockIndex);


            }
        }
    }
}