using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.HostedService
{
    public sealed class RabbitMQConfigurationOptions
    {
        public bool Enabled { get; set; } = true;
        public string UserName { get; set; } = "hvo";
        public string Password { get; set; } = "salisbury";
        public string HostName { get; set; } = "100.106.103.14";
        public ushort Port { get; set; } = 5672;
        public bool AutomaticRecoveryEnabled { get; set; } = true;
    }
}
