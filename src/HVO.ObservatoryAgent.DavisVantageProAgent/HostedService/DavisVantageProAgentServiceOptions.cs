using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.HostedService
{
    public record DavisVantageProAgentServiceOptions
    {
        public uint RestartOnFailureWaitTime { get; init; } = 15;
        public string RemoteConsoleAddress { get; init; } = "192.168.0.121";
        public int RemoteConsolePort { get; init; } = 22222;
        public string StorageQueueName { get; init; } = "weatherrecords";
    }
}
