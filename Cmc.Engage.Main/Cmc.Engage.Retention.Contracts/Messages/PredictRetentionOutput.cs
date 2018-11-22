
using System.Runtime.Serialization;

namespace Cmc.Engage.Retention
{
    [DataContract]
    public class PredictRetentionOutput
    {
        [DataMember(Name = "retentionProbability")]
        public decimal RetentionProbability { get; set; }
    }
}
