
namespace HVO.Hardware.RoofControllerV4
{
    public interface IRoofController
    {
        bool IsInitialized { get; set; }

        RoofControllerStatus Status { get; }

        Task<bool> Initialize(CancellationToken cancellationToken);

        void Stop();
        void Open();
        void Close();
    }
}
