using System.Activities;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Communication.Activities
{
    /// <summary>
    /// This step attaches the appointment in an email that is sent to the student.
    /// </summary>
    public class AttachAppointmentICalFileActivity : ActivityBase
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {

            var logic = executionContext.IocScope.Resolve<IAppointmentService>();
            var tracer = executionContext.LoggerFactory.GetLogger(this.GetType());
            tracer.Trace("Reading In Arguments.");
            var emailId = EmailId.Get(executionContext.ActivityContext);
            var appointmentId = AppointmentId.Get(executionContext.ActivityContext);

            var mimeAttachment = logic.AttachAppointmentICalFileService(emailId, appointmentId);

            IWorkflowContext context = executionContext.ActivityContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.ActivityContext.GetExtension<IOrganizationServiceFactory>();
            var orgService = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
            orgService.Create(mimeAttachment);
            orgService.Execute(new SendEmailRequest
            {
                EmailId = emailId.Id,
                IssueSend = true
            });
            tracer.Trace("Email Sent");

        }

        /// <summary>
        /// The ID of the email that is sent to the student.
        /// </summary>
        [ReferenceTarget(Email.EntityLogicalName)]
        [RequiredArgument]
        [Input("Email")]
        public InArgument<EntityReference> EmailId { get; set; }

        /// <summary>
        /// The appointment schedule for the selected user and/or the student.
        /// </summary>
        [ReferenceTarget(Appointment.EntityLogicalName)]
        [RequiredArgument]
        [Input("Appointment")]
        public InArgument<EntityReference> AppointmentId { get; set; }
    }
}

