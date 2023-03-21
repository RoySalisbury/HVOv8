using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.HostedService
{
    public sealed class DavisVantageProAgentServiceOptions
    {
        public uint RestartOnFailureWaitTime { get; set; } = 15;
        public string RemoteConsoleAddress { get; set; } = "192.168.0.121";
        public int RemoteConsolePort { get; set; } = 22222;
        public string StorageQueueName { get; set; } = "weatherrecords";
    }
}
