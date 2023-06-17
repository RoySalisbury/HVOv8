using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.JKBmsMonitor
{
    internal static class JkMqtt
    {

        public static (string ConfigTopic, string StateTopic, dynamic Configuration) GenerateSensorTopics(string deviceId, string sensorName, string deviceClass, string stateClass, string uom, string icon, string deviceName, string deviceSerialNumber, string deviceModel, string deviceManufacture, string deviceHardwareVersion, string deviceSoftwareVersion)
        {
            var topicBase = $"homeassistant/sensor/{deviceId.ToLower().Replace("", "_")}/{sensorName.ToLower().Replace("' ", "_")}";
            var configTopic = $"{topicBase}/config";
            var stateTopic = $"{topicBase}/state";

            var config = new
            {
                device = new
                {
                    ids = new[] { deviceSerialNumber },
                    mdl = deviceModel,
                    mf = deviceManufacture,
                    name = deviceName,
                    hw_version = deviceHardwareVersion,
                    sw_version = deviceSoftwareVersion
                },
                name = sensorName,
                icon = icon.ToLower(),
                device_class = deviceClass.ToLower(),
                state_class = stateClass.ToLower(),
                state_topic = stateTopic,
                unique_id = $"{deviceId.ToLower().Replace("", "_")}_{sensorName.ToLower().Replace("' ", "_")}",
                object_id = $"{deviceId.ToLower().Replace("", "_")}_{sensorName.ToLower().Replace("' ", "_")}",
                unit_of_measurement = uom
            };

            return (configTopic, stateTopic, config);
        }
    }
}
