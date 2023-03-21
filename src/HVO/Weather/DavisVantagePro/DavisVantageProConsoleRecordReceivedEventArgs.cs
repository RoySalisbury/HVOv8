using System;

namespace HVO.Weather.DavisVantagePro
{
    public sealed class DavisVantageProConsoleRecordReceivedEventArgs : EventArgs
    {

        public DavisVantageProConsoleRecordReceivedEventArgs(DateTimeOffset recordDateTime, byte[] consoleRecord)
        {
            this.RecordDateTime = recordDateTime;
            this.ConsoleRecord = consoleRecord;
        }

        public DateTimeOffset RecordDateTime { get; private set; }
        public byte[] ConsoleRecord { get; private set; }
    }
}
