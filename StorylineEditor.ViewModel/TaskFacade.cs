using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorylineEditor.ViewModel
{
    public static class TaskFacade
    {
        private static Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);

        private static CancellationTokenSource cancellationTokenSource;

        public static void StopMonoTask()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
        }

        public static async void StartMonoTask(Func<CancellationToken, double, TaskStatus> tickAction, TimeSpan tickTimeSpan, double alphaStep, Action<TaskStatus> finAction, Action<TaskStatus> callbackAction)
        {
            StopMonoTask();

            semaphoreObject.WaitOne();

            Task task = CreateAndRun((cancellationTokenSource = new CancellationTokenSource()).Token, tickAction, tickTimeSpan, alphaStep, finAction);
            await task;

            semaphoreObject.Release();

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