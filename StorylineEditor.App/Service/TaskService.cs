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

        private static CancellationTokenSource _cancellationTokenSource;

        public bool IsPaused {get;set;}

        public async void Start(double durationMsec, Func<CancellationToken, double, double, double, double, CustomStatus> tickAction, Action<CustomStatus, double, double, double, double> finAction, Action<CustomStatus> callbackAction)
        {
            Stop();

            Interlocked.Add(ref _lockIndex, 1);

            while (Interlocked.Read(ref _lockIndex) > 1)
            {
                await Task.Delay(2);
            }

            CustomStatus customStatus = CustomStatus.WaitingToRun;

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
                        customStatus = CustomStatus.Canceled;
                        break;
                    }
                    else if (!IsPaused)
                    {
                        customStatus = tickAction(_cancellationTokenSource.Token, startTimeMsec, durationMsec, timeMsec, timeMsec - prevTimeMsec);
                    }

                    await Task.Delay(2);

                    prevTimeMsec = timeMsec;
                    timeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                }

                // Final tick
                customStatus = tickAction(_cancellationTokenSource.Token, startTimeMsec, durationMsec, timeMsec, timeMsec - prevTimeMsec);

                finAction(customStatus, startTimeMsec, durationMsec, timeMsec, timeMsec - prevTimeMsec);
            }
            catch (TaskCanceledException taskCanceledExc) { }
            catch (Exception exc) { } ////// TODO
            finally
            {
                Interlocked.Decrement(ref _lockIndex);

                callbackAction?.Invoke(customStatus);
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
