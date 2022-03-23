using System.Collections.Generic;

namespace TRX_Merger.TrxModel
{
    public class ResultSummary
    {
        public string Outcome { get; set; }
        public Counters Counters { get; set; }
        public List<RunInfo> RunInfos { get; set; }
    }
}
