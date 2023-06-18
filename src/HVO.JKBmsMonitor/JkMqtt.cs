using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HVO.JKBmsMonitor
{
    internal static class JkMqtt
    {

        public static (string ConfigTopic, dynamic ConfigData, string StateTopic, T Value) GenerateSensorData<T>(
            string deviceId,              // "jkbms_280_01"
            string sensorName,            // "Average Cell Voltage"
            string entityName,            // "cell_voltage_avg"
            string deviceClass,           // "voltage"
            string stateClass,            // "measurment"
            string unitOfMeasurment,      // "V"
            string sensorIcon,            // "mdi:flash-triangle"
            T value,
            string deviceName,            // "JK-B2A24S15P"
            string deviceManufacture,     // "Jikong"
            string deviceModel,           // "JK-B2A24S15P"
            string deviceHardwareVersion, // "10.XW"
            string deviceSoftwareVersion, // "10.08"
            string deviceSerialNumber)    // "SN_2042102033"
        {
            var topicBase = $"homeassistant/sensor/{deviceId.ToLower().Replace(" ", "_")}/{entityName.ToLower().Replace(" ", "_")}";
            var configTopic = $"{topicBase}/config";
            var stateTopic = $"{topicBase}/state";

            var configData = new
            {
                device = new
                {
                    ids = new[] { $"SN_{deviceSerialNumber}" },
                    mdl = deviceModel,
                    mf = deviceManufacture,
                    name = deviceName,
                    hw_version = deviceHardwareVersion,
                    sw_version = deviceSoftwareVersion
                },
                name = sensorName,
                icon = sensorIcon?.ToLower(),
                device_class = deviceClass?.ToLower(),
                state_class = stateClass?.ToLower(),
                state_topic = stateTopic,
                unique_id = $"{deviceId.ToLower().Replace(" ", "_")}_{entityName.ToLower().Replace(" ", "_")}",
                object_id = $"{deviceId.ToLower().Replace(" ", "_")}_{entityName.ToLower().Replace(" ", "_")}",
                unit_of_measurement = unitOfMeasurment
            };

            return (configTopic, configData, stateTopic, value);
        }

        public static (string ConfigTopic, dynamic ConfigData, string StateTopic, string Value) GenerateTextSensorData(
            string deviceId,              // "jkbms_280_01"
            string sensorName,            // "Average Cell Voltage"
            string entityName,            // "cell_voltage_avg"
            string deviceClass,           // "voltage"
            string stateClass,            // "measurment"
            string unitOfMeasurment,      // "V"
            string sensorIcon,            // "mdi:flash-triangle"
            string value,
            string deviceName,            // "JK-B2A24S15P"
            string deviceManufacture,     // "Jikong"
            string deviceModel,           // "JK-B2A24S15P"
            string deviceHardwareVersion, // "10.XW"
            string deviceSoftwareVersion, // "10.08"
            string deviceSerialNumber)    // "SN_2042102033"
        {
            var topicBase = $"homeassistant/textsensor/{deviceId.ToLower().Replace(" ", "_")}/{entityName.ToLower().Replace(" ", "_")}";
            var configTopic = $"{topicBase}/config";
            var stateTopic = $"{topicBase}/state";

            var configData = new
            {
                device = new
                {
                    ids = new[] { $"SN_{deviceSerialNumber}" },
                    mdl = deviceModel,
                    mf = deviceManufacture,
                    name = deviceName,
                    hw_version = deviceHardwareVersion,
                    sw_version = deviceSoftwareVersion
                },
                name = sensorName,
                icon = sensorIcon?.ToLower(),
                device_class = deviceClass?.ToLower(),
                state_class = stateClass?.ToLower(),
                state_topic = stateTopic,
                unique_id = $"{deviceId.ToLower().Replace(" ", "_")}_{entityName.ToLower().Replace(" ", "_")}",
                object_id = $"{deviceId.ToLower().Replace(" ", "_")}_{entityName.ToLower().Replace(" ", "_")}",
                unit_of_measurement = unitOfMeasurment
            };

            return (configTopic, configData, stateTopic, value);
        }

        public static (string ConfigTopic, dynamic ConfigData, string StateTopic, bool Value) GenerateBinarySensorData(
            string deviceId,              // "jkbms_280_01"
            string sensorName,            // "Average Cell Voltage"
            string entityName,            // "cell_voltage_avg"
            string deviceClass,           // "voltage"
            string stateClass,            // "measurment"
            string unitOfMeasurment,      // "V"
            string sensorIcon,            // "mdi:flash-triangle"
            string value,
            string deviceName,            // "JK-B2A24S15P"
            string deviceManufacture,     // "Jikong"
            string deviceModel,           // "JK-B2A24S15P"
            string deviceHardwareVersion, // "10.XW"
            string deviceSoftwareVersion, // "10.08"
            string deviceSerialNumber)    // "SN_2042102033"
        {
            var topicBase = $"homeassistant/binarysensor/{deviceId.ToLower().Replace(" ", "_")}/{entityName.ToLower().Replace(" ", "_")}";
            var configTopic = $"{topicBase}/config";
            var stateTopic = $"{topicBase}/state";

            var configData = new
            {
                device = new
                {
                    ids = new[] { $"SN_{deviceSerialNumber}" },
                    mdl = deviceModel,
                    mf = deviceManufacture,
                    name = deviceName,
                    hw_version = deviceHardwareVersion,
                    sw_version = deviceSoftwareVersion
                },
                name = sensorName,
                icon = sensorIcon?.ToLower(),
                device_class = deviceClass?.ToLower(),
                state_class = stateClass?.ToLower(),
                state_topic = stateTopic,
                unique_id = $"{deviceId.ToLower().Replace(" ", "_")}_{entityName.ToLower().Replace(" ", "_")}",
                object_id = $"{deviceId.ToLower().Replace(" ", "_")}_{entityName.ToLower().Replace(" ", "_")}",
                unit_of_measurement = unitOfMeasurment
            };

            return (configTopic, configData, stateTopic, value);
        }



        public static async Task Publish(IManagedMqttClient mqttClient, string topic, string data)
        {
            await mqttClient.EnqueueAsync(topic, data);
        }

        public static async Task Publish(IManagedMqttClient mqttClient, string topic, dynamic data)
        {
            var payload = JsonSerializer.Serialize<dynamic>(data, new JsonSerializerOptions() 
            { 
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull 
            });

            await Publish(mqttClient, topic, payload);
        }
    }
}
