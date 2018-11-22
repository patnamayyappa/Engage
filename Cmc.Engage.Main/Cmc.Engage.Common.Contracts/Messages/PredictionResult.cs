using Newtonsoft.Json;
using System;

namespace Cmc.Engage.Common
{
    public class PredictionResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PredictionResult Results { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "output1")]
        public PredictionResult Output1 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "value")]
        public PredictionResult Value { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String[][] Values { get; set; }
    }
}
