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
    }
}
