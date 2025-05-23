using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.HostedService
{
    public record RabbitMQConfigurationOptions
    {
        public bool Enabled { get; init; } = true;
        public string UserName { get; init; } = "hvo";
        public string Password { get; init; } = "salisbury";
        public string HostName { get; init; } = "100.106.103.14";
        public ushort Port { get; init; } = 5672;
        public bool AutomaticRecoveryEnabled { get; init; } = true;
    }
}
