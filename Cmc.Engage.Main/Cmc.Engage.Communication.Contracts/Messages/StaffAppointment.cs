using System;
using Cmc.Engage.Common.Utilities;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Communication
{
    public class StaffAppointment
    {
        public Guid appointmentId { get; set; }
        public Guid ownerId { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string description { get; set; }

        public StaffAppointment(Entity crmAppointment)
        {
            appointmentId = crmAppointment.Id;
            ownerId = crmAppointment.GetAttributeValue<EntityReference>("ownerid").Id;
            start = crmAppointment.GetAttributeValue<DateTime>("scheduledstart").ToIso8601Date();
            end = crmAppointment.GetAttributeValue<DateTime>("scheduledend").ToIso8601Date();
            title = crmAppointment.GetAttributeValue<string>("subject");
            description = crmAppointment.GetAttributeValue<string>("description");
        }
    }
}
