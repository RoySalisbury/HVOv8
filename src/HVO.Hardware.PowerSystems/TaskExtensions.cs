namespace HVO
{
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, int timeout = 1000, CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), taskCompletionSource))
            {
                // Setup the timeout task
                var timeoutTask = Task.Delay(timeout, cancellationToken);

                // See which task completes first
                var resultTask = await Task.WhenAny(task, taskCompletionSource.Task, timeoutTask);

                if (resultTask == taskCompletionSource.Task)
                {
                    // The original task was cancelled
                    throw new OperationCanceledException(cancellationToken);
                }

                if (resultTask == timeoutTask)
                {
                    // The execution of the task took too long.
                    throw new TimeoutException();
                }
            }
            return await task;
        }
    }
}
