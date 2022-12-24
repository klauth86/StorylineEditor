using System;
using System.Threading;

namespace StorylineEditor.ViewModel
{
    public static class TaskFacade
    {
        private static Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);
        
        private static CancellationTokenSource cancellationTokenSource;

        public static async void StartMonoTask(Action<CancellationToken> action)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            semaphoreObject.WaitOne();
            action(cancellationTokenSource.Token);
            semaphoreObject.Release();
        }
    }
}
