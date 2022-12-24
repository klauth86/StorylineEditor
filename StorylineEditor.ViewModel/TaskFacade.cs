using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorylineEditor.ViewModel
{
    public static class TaskFacade
    {
        private static bool hasMonoTask = false;

        private static CancellationTokenSource cancellationTokenSource;

        public static void StopMonoTask()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
        }

        public static async void StartMonoTask(Func<CancellationToken, double, TaskStatus> tickAction, TimeSpan tickTimeSpan, double alphaStep, Action<TaskStatus> finAction, Action<TaskStatus> callbackAction)
        {
            StopMonoTask();

            while (hasMonoTask)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            hasMonoTask = true;

            Task task = CreateAndRun((cancellationTokenSource = new CancellationTokenSource()).Token, tickAction, tickTimeSpan, alphaStep, finAction);
            await task;

            hasMonoTask = false;

            callbackAction?.Invoke(task.Status);
        }

        private static async Task CreateAndRun(CancellationToken token, Func<CancellationToken, double, TaskStatus> tickAction, TimeSpan tickTimeSpan, double alphaStep, Action<TaskStatus> finAction)
        {
            TaskStatus tickStatus;
            double alpha = 0;

            do
            {
                tickStatus = tickAction(token, alpha);
                alpha += alphaStep;

                await Task.Delay(tickTimeSpan);
            }
            while (tickStatus == TaskStatus.Running);

            finAction(tickStatus);
        }
    }
}