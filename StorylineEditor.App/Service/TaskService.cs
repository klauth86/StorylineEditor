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

        private static bool _isCancellationRequested;

        public bool IsPaused {get;set;}

        public async void Start(double durationMsec, Func<double, double, double, double, CustomStatus> tickAction, Func<double, double, double, double, CustomStatus, CustomStatus> finAction, Action<CustomStatus> callbackAction)
        {
            Stop();

            Interlocked.Add(ref _lockIndex, 1);

            while (Interlocked.Read(ref _lockIndex) > 1)
            {
                await Task.Delay(2);
            }

            _isCancellationRequested = false;

            CustomStatus customStatus = CustomStatus.WaitingToRun;

            try
            {
                double startTimeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                double finishTimeMsec = durationMsec >= 0 ? (startTimeMsec + durationMsec) : double.MaxValue;

                double prevTimeMsec = startTimeMsec;
                double timeMsec = startTimeMsec;
                double deltaTimeMsec = 0;

                while (timeMsec < finishTimeMsec)
                {
                    if (_isCancellationRequested)
                    {
                        customStatus = CustomStatus.Canceled;
                    }
                    else if (!IsPaused)
                    {
                        customStatus = tickAction(startTimeMsec, durationMsec, timeMsec, deltaTimeMsec);
                    }

                    if (customStatus == CustomStatus.RanToCompletion || customStatus == CustomStatus.Canceled || customStatus == CustomStatus.Faulted)
                    {
                        break;
                    }

                    await Task.Delay(2);

                    prevTimeMsec = timeMsec;
                    timeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                    deltaTimeMsec = timeMsec - prevTimeMsec;
                }

                // Mark as completed if cycle was broken by time limits

                if (customStatus == CustomStatus.Running)
                {
                    customStatus = CustomStatus.RanToCompletion;
                }

                prevTimeMsec = timeMsec;
                timeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                deltaTimeMsec = timeMsec - prevTimeMsec;

                customStatus = finAction(startTimeMsec, durationMsec, timeMsec, deltaTimeMsec, customStatus);
            }
            catch (TaskCanceledException taskCanceledExc) { throw taskCanceledExc; }
            catch (Exception exc) { throw exc; }
            finally
            {
                Interlocked.Decrement(ref _lockIndex);

                callbackAction?.Invoke(customStatus);
            }
        }

        public void Stop()
        {
            _isCancellationRequested = true;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
