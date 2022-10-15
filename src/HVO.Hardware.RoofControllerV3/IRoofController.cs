using System.Threading;

namespace HVO.Hardware.RoofControllerV3
{
    public interface IRoofController
    {
        bool IsInitialized { get; }

        RoofControllerStatus Status { get; }

        void Initialize(CancellationToken cancellationToken);

        void Stop();
        void Open();
        void Close();   
    }
}