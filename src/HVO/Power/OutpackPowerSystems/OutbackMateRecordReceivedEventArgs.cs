using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HVO.Power.OutbackPowerSystems {
  public sealed class OutbackMateRecordReceivedEventArgs : EventArgs {
    public OutbackMateRecordReceivedEventArgs(DateTimeOffset recordDateTime, string dataRecord) {
      this.RecordDateTime = recordDateTime;
      this.DataRecord = dataRecord;
    }

    public DateTimeOffset RecordDateTime { get; private set; }
    public string DataRecord { get; private set; }
  }
}
