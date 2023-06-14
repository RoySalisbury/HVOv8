using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConsoleApp1
{
    internal class Program
    {
        // https://github.com/fl4p/batmon-ha/blob/master/bmslib/jikong.py
        // https://github.com/syssi/esphome-jk-bms/blob/main/components/jk_bms_ble/jk_bms_ble.cpp#L20
        // https://github.com/Jakeler/ble-serial
        // https://github.com/jblance/mpp-solar/blob/master/mppsolar/protocols/jk02.py



        static void Main(string[] args)
        {
            // getInfo = b'\xaa\x55\x90\xeb\x97\x00\x00         \x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x11'

            var getInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x97, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
            getInfo[19] = JKCRC(getInfo[0..19]);

            using JKBmsSocketClient client = new JKBmsSocketClient();
            client.Open(client.PortName);

            for (int i = 0; i < 50; i++)
            {
                client.SendRequest(getInfo, out var response);
                Console.WriteLine(response.Length);

                // seems that only the first 300 bytes are relevent, but the packet is 320.  Substract out the header and CRC and that leaves 15 bytes.  
                if ((response.Length >= 300) && (response.Length <= 320))
                {
                    var vendorId = ASCIIEncoding.ASCII.GetString(response.Slice(6, 16));
                    var hardwareVersion = ASCIIEncoding.ASCII.GetString(response.Slice(22, 8));
                    var softwareVersion = ASCIIEncoding.ASCII.GetString(response.Slice(30, 8));
                    var deviceName = ASCIIEncoding.ASCII.GetString(response.Slice(46, 16));
                    var devicePasscode = ASCIIEncoding.ASCII.GetString(response.Slice(62, 16));
                    var manufactureDate = ASCIIEncoding.ASCII.GetString(response.Slice(78, 8));
                    var serialNumber = ASCIIEncoding.ASCII.GetString(response.Slice(86, 11));
                    var passcode = ASCIIEncoding.ASCII.GetString(response.Slice(97, 5));
                    var userData = ASCIIEncoding.ASCII.GetString(response.Slice(102, 16));
                    var setupPasscode = ASCIIEncoding.ASCII.GetString(response.Slice(118, 16));

                    Console.WriteLine(deviceName);
                }

                var getCellInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x96, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
                getCellInfo[19] = JKCRC(getCellInfo[0..19]);

                client.SendRequest(getCellInfo, out var response2);
                Console.WriteLine(response2.Length);
            }

                Console.ReadLine();
        }



        static byte JKCRC(byte[] data)
        {
            return (byte)(data.Sum(x => x) & 0xFF);
        }
    }
}