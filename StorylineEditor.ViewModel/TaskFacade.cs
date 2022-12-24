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

            Task task = null;
            try
            {
                task = CreateAndRun((cancellationTokenSource = new CancellationTokenSource()).Token, tickAction, tickTimeSpan, alphaStep, finAction);
                await task;
            }
            catch (TaskCanceledException taskCanceledException) { }
            catch (Exception exception) { } // TODO
            finally
            {
                System.Diagnostics.Trace.WriteLine("Monitor.Exit(locker)", "@@@");
                Monitor.Exit(locker);

                if (task != null) callbackAction?.Invoke(task.Status);
            }
        }

        private static async Task CreateAndRun(CancellationToken token, Func<CancellationToken, double, TaskStatus> tickAction, TimeSpan tickTimeSpan, double alphaStep, Action<TaskStatus> finAction)
        {
            TaskStatus tickStatus;
            double alpha = 0;

            do
            {
                tickStatus = tickAction(token, alpha);
                alpha += alphaStep;

                await Task.Delay(tickTimeSpan, token);
            }
            while (tickStatus == TaskStatus.Running);

            finAction(tickStatus);
        }
    }
}