using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    /// <summary>
    /// contract for trip activity service.
    /// </summary>
    public interface ITripActivityService
    {
        /// <summary>
        /// to update the latitude and longitude for give address in trip activity update or create.
        /// </summary>
        /// <param name="context">plugin execution context</param>
        void TripActivityUpdateLatitudeLongitude(IExecutionContext context);

        /// <summary>
        /// to associate and diassociate the staff members when changes happened at trip activity
        /// </summary>
        /// <param name="context">plugin context</param>
        void AssociateDisassociateStaffmembers(IExecutionContext context);
        
        /// <summary>
        /// Get Trip activity Timezone information from the executing user.
        /// </summary>
        /// <param name="tripActivityId">trip activity id</param>
        /// <param name="userId">user id</param>
        /// <returns>Timezone name</returns>
        string GetTripActivityTimezoneInformationActivity(EntityReference tripActivityId, Guid? userId);

        /// <summary>
        /// Send Email to corresponding staff memebers and volunteers
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="tripActivityId"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="location"></param>
        /// <param name="fileName"></param>
        /// <param name="subject"></param>
        /// <param name="description"></param>
        void AttachTripActivityICalFileActivity(EntityReference emailId, EntityReference tripActivityId, DateTime startDateTime
        , DateTime endDateTime, string location, string fileName, string subject, string description);
    }
}
