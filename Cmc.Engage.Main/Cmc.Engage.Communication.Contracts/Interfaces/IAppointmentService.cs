using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using IExcutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Communication
{
    public interface IAppointmentService
    {
        void CreateStaffAppointment(IExcutionContext context);

        void DeleteStaffAppointment(IExcutionContext context);
        void RetrieveStaffAppointments(IExcutionContext context);
        void RetrieveStaffAvailability(IExcutionContext context);
        string CreateStaffAppointment(Guid? contactId, Guid userId, Guid locationId, DateTime startDate, DateTime endDate, string title, string description);

        string DeleteStaffAppointment(Guid appointmentId);
        string RetrieveStaffAvailability(Guid userId, Guid accountId);

        ActivityMimeAttachment AttachAppointmentICalFileService(
            EntityReference emilId,
            EntityReference appointmntId);

        string RetrieveDepartmentPhoneNumberService( List<object> departmentId);
        /// <summary>
        /// On Update of Appointment Subject,ScheduledStart,cmc_EndDateTime and Location this method will update all related Trip Activity details.
        /// </summary>
        /// <param name="executionContext"></param>
        void UpdateTripActivityAppointmentDetails(IExcutionContext context);
    }
}
