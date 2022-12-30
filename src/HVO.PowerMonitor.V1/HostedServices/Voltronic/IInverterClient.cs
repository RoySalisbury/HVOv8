namespace HVO.PowerMonitor.V1.HostedServices.Voltronic
{
    public interface IInverterClient
    {
        bool IsOpen { get; }

        void Open(CancellationToken cancellationToken = default);
        void Close(CancellationToken cancellationToken = default);

        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
        Task FlushAsync(CancellationToken cancellationToken = default);
    }
}
