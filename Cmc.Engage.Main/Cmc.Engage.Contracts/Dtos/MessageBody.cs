using System;

namespace Cmc.Engage.Contracts
{
    public class MessageBody
    {
        public string OrgName { get; set; }
    }

    public class JobMessageBody : MessageBody
    {
        public Guid Hlx_JobId { get; set; }
    }

    public class SendTextMessageBody : MessageBody
    {
        public Guid Hlx_TextId { get; set; }
    }
}
