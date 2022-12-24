using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorylineEditor.ViewModel
{
    public static class TaskFacade
    {
        private static readonly object locker = new object();

        private static CancellationTokenSource cancellationTokenSource;

        public static void StopMonoTask()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
        }

        public static async void StartMonoTask(Func<CancellationToken, double, TaskStatus> tickAction, TimeSpan tickTimeSpan, double alphaStep, Action<TaskStatus> finAction, Action<TaskStatus> callbackAction)
        {
            StopMonoTask();

            Monitor.Enter(locker);
            System.Diagnostics.Trace.WriteLine("Monitor.Enter(locker)", "@@@");

            TaskStatus taskStatus = TaskStatus.WaitingForActivation;
            double alpha = 0;

            try
            {
                cancellationTokenSource = new CancellationTokenSource();

                do
                {
                    taskStatus = tickAction(cancellationTokenSource.Token, alpha);
                    alpha += alphaStep;

                    await Task.Delay(tickTimeSpan, cancellationTokenSource.Token);
                }
                while (taskStatus == TaskStatus.Running);

                finAction(taskStatus);
            }
            catch (TaskCanceledException taskCanceledException) { }
            catch (Exception exception) { } // TODO
            finally
            {
                System.Diagnostics.Trace.WriteLine("Monitor.Exit(locker)", "@@@");
                Monitor.Exit(locker);

                callbackAction?.Invoke(taskStatus);
            }
        }
    }
}