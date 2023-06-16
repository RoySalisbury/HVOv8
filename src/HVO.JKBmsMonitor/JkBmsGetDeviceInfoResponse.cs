using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace HVO.JKBmsMonitor
{
    public class JkBmsGetDeviceInfoResponse : JkBmsResponse
    {
        public JkBmsGetDeviceInfoResponse(ReadOnlyMemory<byte> data) : base(data) 
        {
            this.InitializeFromPayload();
        }

        protected override void InitializeFromPayload()
        {
            var payload = this.Payload.Span;

            this.VendorId = ASCIIEncoding.ASCII.GetString(payload.Slice(6, 16)).TrimEnd('\0');
            this.HardwareVersion = ASCIIEncoding.ASCII.GetString(payload.Slice(22, 8)).TrimEnd('\0');
            this.SoftwareVersion = ASCIIEncoding.ASCII.GetString(payload.Slice(30, 8)).TrimEnd('\0');

            var uptime = BitConverter.ToUInt32(payload.Slice(38, 4));
            this.Uptime = TimeSpan.FromSeconds(uptime);

            this.PowerOnCount = BitConverter.ToInt32(payload.Slice(42, 4));
            this.DeviceName = ASCIIEncoding.ASCII.GetString(payload.Slice(46, 16)).TrimEnd('\0');
            this.DevicePasscode = ASCIIEncoding.ASCII.GetString(payload.Slice(62, 16)).TrimEnd('\0');
            this.ManufactureDate = ASCIIEncoding.ASCII.GetString(payload.Slice(78, 8)).TrimEnd('\0');
            this.SerialNumber = ASCIIEncoding.ASCII.GetString(payload.Slice(86, 11)).TrimEnd('\0');
            this.Passcode = ASCIIEncoding.ASCII.GetString(payload.Slice(97, 5)).TrimEnd('\0');
            this.UserData = ASCIIEncoding.ASCII.GetString(payload.Slice(102, 16)).TrimEnd('\0');
            this.SetupPasscode = ASCIIEncoding.ASCII.GetString(payload.Slice(118, 16)).TrimEnd('\0');
        }

        public string VendorId { get; private set; }
        public string HardwareVersion { get; private set; }
        public string SoftwareVersion { get; private set; }
        public TimeSpan Uptime { get; private set; }
        public int PowerOnCount { get; private set; }
        public string DeviceName { get; private set; }
        public string DevicePasscode { get; private set; }
        public string ManufactureDate { get; private set; }
        public string SerialNumber { get; private set; }
        public string Passcode { get; private set; }
        public string UserData { get; private set; }
        public string SetupPasscode { get; private set; }
    }
}
