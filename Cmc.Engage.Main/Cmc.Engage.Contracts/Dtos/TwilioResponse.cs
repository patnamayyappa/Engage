using Newtonsoft.Json;

namespace Cmc.Engage.Contracts
{
    public class TwilioResponse
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "more_info")]
        public string MoreInfo { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}
