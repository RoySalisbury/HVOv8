using HVO.Hardware.PowerSystems.Voltronic;

namespace HVO.Test.InverterQuery
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //using var client = new InverterCommunicationsClient();

            //var request1 = new InverterGetSerialNumberRequest();
            //var request2 = new InverterGetDeviceProtocolIDRequest();

            //client.Open();

            //var result1 = await client.SendRequest<InverterGetSerialNumberResponse>(request1);
            //var result2 = await client.SendRequest<InverterGetDeviceProtocolIDResponse>(request2);

            using var client = new InverterClient();
            client.Open();

            var r1 = await client.GetDeviceProtocolID();
            var r2 = await client.GetDeviceSerialNumber();
            var r3 = await client.GetDeviceSerialNumberEx();
            var r4 = await client.GetMainCPUFirmwareVersion();
            var r5 = await client.GetAnotherCPUFirmwareVersion(); //NAK
            var r6 = await client.GetRemotePanelCPUFirmwareVersion();
            var r7 = await client.GetBLECPUFirmwareVersion(); // NAK
            var r8 = await client.GetDeviceRatingInformation();
            var r9 = await client.GetDeviceFlagStatus();
            var r10 = await client.GetDeviceGeneralStatusParameters();
            var r11 = await client.GetDeviceMode();
            var r12 = await client.GetDeviceWarningStatus();
            var r13 = await client.GetDefaultSettingInformation();

            var r14 = await client.GetSelectableMaxChargingCurrentValues();
            var r15 = await client.GetSelectableMaxUtilityChargingCurrentValues();
            var r16 = await client.GetDeviceOutputSourcePriorityTime();
            var r17 = await client.GetDeviceChargerSourcePriorityTime();
            var r18 = await client.GetDeviceTime();
            var r19 = await client.GetDeviceModelName();
            var r20 = await client.GetDeviceGeneralModelName();
            var r21 = await client.GetBatteryEqualizationStatusParameters();

        }
    }
}