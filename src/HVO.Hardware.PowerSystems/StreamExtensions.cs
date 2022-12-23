namespace HVO
{
    public static class StreamExtensions
    {
        public static async Task<int> ReadAsyncWithTimeout(this Stream stream, byte[] buffer, int offset, int count, int? readTimeout = null, CancellationToken cancellationToken = default)
        {
            if (stream.CanRead)
            {

                Task<int> readTask = stream.ReadAsync(buffer, offset, count, cancellationToken);

                Task delayTask = Task.Delay(readTimeout.GetValueOrDefault(stream.ReadTimeout), cancellationToken);

                Task task = await Task.WhenAny(readTask, delayTask);

                if (task == readTask)
                {
                    return await readTask;
                }
            }
            return -1;
        }

        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            return await task;
        }
    }
}
