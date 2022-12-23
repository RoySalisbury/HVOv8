using HVO.Hardware.PowerSystems.Voltronic;

namespace HVO.Test.InverterQuery
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new InverterCommunicationsClient();

            var request = new InverterGetSerialNumberRequest();
            var bytes = request.ToBytes();

            client.Open();

            var result = await client.SendRequest(request);
            
        }
    }
}