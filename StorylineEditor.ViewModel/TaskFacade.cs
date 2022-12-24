using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorylineEditor.ViewModel
{
    public static class TaskFacade
    {
        private static Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);

        private static CancellationTokenSource cancellationTokenSource;

        public static async void StartMonoTask(Action<CancellationToken> action, Action<bool> callbackAction)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            semaphoreObject.WaitOne();
            await Task.Run(() => action(cancellationTokenSource.Token));
            semaphoreObject.Release();

            callbackAction?.Invoke(cancellationTokenSource.Token.IsCancellationRequested);
        }
    }
}
