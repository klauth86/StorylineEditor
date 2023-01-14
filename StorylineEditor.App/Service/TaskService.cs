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

namespace StorylineEditor.App.Service
{
    public class TaskService : ITaskService
    {
        private static long _lockIndex = 0;

        private static bool _isPaused = false;

        private static CancellationTokenSource _cancellationTokenSource;

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void SetIsPaused(bool isPaused) { _isPaused = isPaused; }

        public async void Start(double durationMsec, Func<CancellationToken, double, double, double, double, CustomTaskStatus> tickAction, Action<CustomTaskStatus, double, double, double, double> finAction, Action<CustomTaskStatus> callbackAction)
        {
            Stop();

            Interlocked.Add(ref _lockIndex, 1);

            while (Interlocked.Read(ref _lockIndex) > 1)
            {
                await Task.Delay(2);
            }

            CustomTaskStatus CustomTaskStatus = CustomTaskStatus.WaitingToRun;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                double startTimeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                double timeMsec = startTimeMsec;
                double prevTimeMsec = timeMsec;

                double finishTimeMsec = startTimeMsec + durationMsec;

                while (timeMsec < finishTimeMsec)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        CustomTaskStatus = CustomTaskStatus.Canceled;
                        break;
                    }
                    else if (!_isPaused)
                    {
                        CustomTaskStatus = tickAction(_cancellationTokenSource.Token, startTimeMsec, durationMsec, timeMsec, timeMsec - prevTimeMsec);
                    }

                    await Task.Delay(2);

                    prevTimeMsec = timeMsec;
                    timeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                }

                // Final tick
                CustomTaskStatus = tickAction(_cancellationTokenSource.Token, startTimeMsec, durationMsec, timeMsec, timeMsec - prevTimeMsec);

                finAction(CustomTaskStatus, startTimeMsec, durationMsec, timeMsec, timeMsec - prevTimeMsec);
            }
            catch (TaskCanceledException taskCanceledExc) { }
            catch (Exception exc) { } ////// TODO
            finally
            {
                Interlocked.Decrement(ref _lockIndex);

                callbackAction?.Invoke(CustomTaskStatus);
            }
        }
    }
}
