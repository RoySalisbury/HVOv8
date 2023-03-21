using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HVO.Power.OutbackPowerSystems {
  public interface IOutbackMateRecord {
    OutbackMateRecordType RecordType { get; }
    string RawData { get; }
  }
}
