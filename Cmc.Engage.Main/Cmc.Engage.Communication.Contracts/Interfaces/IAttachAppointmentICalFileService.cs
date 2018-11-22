using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Communication
{
    public interface IAttachAppointmentICalFileService
    {
        ActivityMimeAttachment RunActivity(IActivityExecutionContext executionContext, EntityReference emailId, EntityReference appointmentId);
    }
}
