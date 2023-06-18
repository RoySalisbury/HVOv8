namespace HVO.JKBmsMonitor
{
    public class JkBmsMonitorHostOptions 
    {
        public uint RestartOnFailureWaitTime { get; set; } = 15;
        public string MqttUserName { get; set; } = "homeassistant";
        public string MqttPassword { get; set; } = "iawuaPhoNg9ohp1top7oowangushahNgaegeehuegheiba0Pa8em2cahjae9hod1";
        public string MqttHost { get; set; } = "192.168.0.10";
        public int? MqttPort { get; set; } = 1883;

        public string MqttBluetoothDeviceId { get; set; } // = "jkbms_280_01";
        public string BluetoothAdapterName { get; set; } = "hci0";
        public string BluetoothDeviceAddress { get; set; } // = "C8:47:8C:E4:54:B1"; // BMS-280-01
        public int BluetoothScanTimeout { get; set; } = 20;
    }
}

// BMS-280-05 - "C8:47:8C:EC:1E:B5"