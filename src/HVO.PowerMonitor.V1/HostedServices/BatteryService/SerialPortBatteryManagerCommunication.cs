namespace HVO.PowerMonitor.V1.HostedServices.BatteryService
{
    public sealed class SerialPortBatteryManagerCommunication : BatteryManagerCommunicationDevice
    {
        internal SerialPortBatteryManagerCommunication() { }
    }

    public sealed class TcpSocketBatteryManagerCommunication : BatteryManagerCommunicationDevice
    {
        internal TcpSocketBatteryManagerCommunication() { }
    }

    public sealed class BluetoothLEBatteryManagerCommunication : BatteryManagerCommunicationDevice
    {
        internal BluetoothLEBatteryManagerCommunication() { }
    }

}
