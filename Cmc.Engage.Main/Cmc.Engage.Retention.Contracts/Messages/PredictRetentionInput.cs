using System;
using System.Runtime.Serialization;

namespace Cmc.Engage.Retention
{
    [DataContract]
    public class PredictRetentionInput
    {
        [DataMember(Name = "studentId")]
        public Guid StudentId { get; set; }
    }
}
