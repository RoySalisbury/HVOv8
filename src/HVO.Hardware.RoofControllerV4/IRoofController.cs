
using System.ComponentModel;

namespace HVO.Hardware.RoofControllerV4
{
    public interface IRoofController : INotifyPropertyChanged
    {
        bool IsInitialized { get; set; }

        RoofControllerStatus Status { get; }

        Task<bool> Initialize(CancellationToken cancellationToken);

        void Stop();
        void Open();
        void Close();
    }
}
