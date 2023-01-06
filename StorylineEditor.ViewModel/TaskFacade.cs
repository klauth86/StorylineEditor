/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorylineEditor.ViewModel
{
    public static class MonoTaskFacade
    {
        private static readonly object _locker = new object();

        private static bool _isPaused = false;

        private static CancellationTokenSource _cancellationTokenSource;

        public static void Stop()
        {
            _isPaused = false; // cancellation should be consiedered inside tickAction

            _cancellationTokenSource?.Cancel();
        }

        public static void SetIsPaused(bool isPaused) { _isPaused = isPaused; }

        public static async void Start(Func<CancellationToken, double, TaskStatus> tickAction, TimeSpan tickTimeSpan, double alphaStep, Action<TaskStatus> finAction, Action<TaskStatus> callbackAction)
        {
            Stop();

            Monitor.Enter(_locker);

            TaskStatus taskStatus = TaskStatus.WaitingForActivation;
            double alpha = 0;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                do
                {
                    if (!_isPaused)
                    {
                        taskStatus = tickAction(_cancellationTokenSource.Token, alpha);
                        alpha += alphaStep;
                    }

                    await Task.Delay(tickTimeSpan, _cancellationTokenSource.Token);
                }
                while (taskStatus == TaskStatus.Running);

                finAction(taskStatus);
            }
            catch (TaskCanceledException taskCanceledException) { }
            catch (Exception exception) { } ////// TODO
            finally
            {
                Monitor.Exit(_locker);

                callbackAction?.Invoke(taskStatus);
            }
        }
    }
}