namespace Cmc.Engage.Contracts
{
    public class QueueMessageSchedule
    {
        public int? ScheduleId { get; set; }
        public string MessageName { get; set; }
        public string MessageBody { get; set; }
        public string RecurrencePattern { get; set; }
        public string QueueName { get; set; }
    }
}
