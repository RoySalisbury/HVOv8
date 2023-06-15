using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace HVO.JKBmsMonitor
{
    public class JkBmsGetDeviceInfoResponse : JkBmsResponse
    {
        public JkBmsGetDeviceInfoResponse(int protocolVersion) : base(protocolVersion) { }

        protected override void InitializeFromPayload(ReadOnlySpan<byte> payload)
        {
            base.InitializeFromPayload(payload);

            var vendorId = ASCIIEncoding.ASCII.GetString(payload.Slice(6, 16));
            var hardwareVersion = ASCIIEncoding.ASCII.GetString(payload.Slice(22, 8));
            var softwareVersion = ASCIIEncoding.ASCII.GetString(payload.Slice(30, 8));
            var uptime = BitConverter.ToInt32(payload.Slice(38, 4));
            var powerOnCount = BitConverter.ToInt32(payload.Slice(42, 4));
            var deviceName = ASCIIEncoding.ASCII.GetString(payload.Slice(46, 16));
            var devicePasscode = ASCIIEncoding.ASCII.GetString(payload.Slice(62, 16));
            var manufactureDate = ASCIIEncoding.ASCII.GetString(payload.Slice(78, 8));
            var serialNumber = ASCIIEncoding.ASCII.GetString(payload.Slice(86, 11));
            var passcode = ASCIIEncoding.ASCII.GetString(payload.Slice(97, 5));
            var userData = ASCIIEncoding.ASCII.GetString(payload.Slice(102, 16));
            var setupPasscode = ASCIIEncoding.ASCII.GetString(payload.Slice(118, 16));
        }
    }



}
