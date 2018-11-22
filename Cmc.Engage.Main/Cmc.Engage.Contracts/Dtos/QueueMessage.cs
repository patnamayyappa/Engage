using System;

namespace Cmc.Engage.Contracts
{
    public class QueueMessage
    {
        public int MessageId { get; set; }
        public string MessageName { get; set; }
        public byte ErrorCount { get; set; }
        public string QueueName { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public QueueMessageSchedule Schedule { get; set; }

        private string _messageBody;
        public string MessageBody
        {
            get
            {
                return _messageBody;
            }
            set
            {
                _messageBody = value;
                _deserializedMessageBody = null;
            }
        }

        private MessageBody _deserializedMessageBody { get; set; }
        public string OrgName
        {
            get
            {
                if (_deserializedMessageBody == null && MessageBody != null)
                {
                    _deserializedMessageBody = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageBody>(MessageBody);
                }

                return _deserializedMessageBody != null ? _deserializedMessageBody.OrgName : null;
            }
        }
    }
}
