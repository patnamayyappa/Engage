using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step attaches details of the retrieved trip schedule in an email to corresponding staff members and volunteers.
    /// </summary>
    public class AttachTripActivityICalFileActivity : ActivityBase
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            var tripActivityService = executionContext.IocScope.Resolve<ITripActivityService>();
            var tracer = executionContext.LoggerFactory.GetLogger(this.GetType());
            tracer.Trace("Reading In Arguments.");
            var emailId = EmailId.Get(executionContext.ActivityContext);
            tracer.Trace("EmailId is "+ emailId.Id);
            var tripActivityId = TripActivityId.Get(executionContext.ActivityContext);
            tracer.Trace("TripActivityId is " + tripActivityId.Id);
            var startDateTime = StartDateTime.Get(executionContext.ActivityContext);
            tracer.Trace("StartDateTime is " + startDateTime.ToLongDateString());
            var endDateTime = EndDateTime.Get(executionContext.ActivityContext);
            tracer.Trace("EndDateTime is " + endDateTime.ToLongDateString());
            var location = Location.Get(executionContext.ActivityContext);
            tracer.Trace("Location is " + location);
            var fileName = FileName.Get(executionContext.ActivityContext);
            tracer.Trace("File name is " + fileName);
            var subject = Subject.Get(executionContext.ActivityContext);
            tracer.Trace("Subject is " + subject);
            var description = Description.Get(executionContext.ActivityContext);
            tracer.Trace("Description is " + description);
            tripActivityService.AttachTripActivityICalFileActivity(emailId, tripActivityId, startDateTime, endDateTime,location, fileName, subject,description);          
        }

        /// <summary>
        /// The ID of the email that is drafted.
        /// </summary>
        [ReferenceTarget(Email.EntityLogicalName)]
        [RequiredArgument]
        [Input("Email")]
        public InArgument<EntityReference> EmailId { get; set; }

        /// <summary>
        /// The Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("TripActivity")]
        public InArgument<EntityReference> TripActivityId { get; set; }

        /// <summary>
        /// The start date and time of the Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("StartDateTime")]
        public InArgument<DateTime> StartDateTime { get; set; }

        /// <summary>
        /// The end date and time of the Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("EndDateTime")]
        public InArgument<DateTime> EndDateTime { get; set; }

        /// <summary>
        /// The location of the Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("Location")]
        public InArgument<string> Location { get; set; }

        /// <summary>
        /// The file name of the Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("FileName")]
        public InArgument<string> FileName { get; set; }

        /// <summary>
        /// The subject of the Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("Subject")]
        public InArgument<string> Subject { get; set; }

        /// <summary>
        /// The description of the Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("Description")]
        public InArgument<string> Description { get; set; }
    }
}
