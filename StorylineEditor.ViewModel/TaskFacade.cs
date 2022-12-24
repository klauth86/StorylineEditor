using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorylineEditor.ViewModel
{
    public static class TaskFacade
    {
        private static bool _isPaused = false;

        private static readonly object _locker = new object();

        private static CancellationTokenSource _cancellationTokenSource;

        public static void StopMonoTask()
        {
            _isPaused = false; // cancellation should be consiedered inside tickAction

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        public static void PauseUnpauseMonoTask(bool isPaused) { _isPaused = isPaused; }

        public static async void StartMonoTask(Func<CancellationToken, double, TaskStatus> tickAction, TimeSpan tickTimeSpan, double alphaStep, Action<TaskStatus> finAction, Action<TaskStatus> callbackAction)
        {
            StopMonoTask();

            Monitor.Enter(_locker);
            System.Diagnostics.Trace.WriteLine("Monitor.Enter(locker)", "@@@");

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
            catch (Exception exception) { } // TODO
            finally
            {
                System.Diagnostics.Trace.WriteLine("Monitor.Exit(locker)", "@@@");
                Monitor.Exit(_locker);

                callbackAction?.Invoke(taskStatus);
            }
        }
    }
}