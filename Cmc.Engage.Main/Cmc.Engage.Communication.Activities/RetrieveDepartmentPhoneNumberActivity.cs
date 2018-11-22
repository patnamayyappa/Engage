using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Collections.Generic;
using Cmc.Engage.Communication;

namespace Cmc.Engage.Communication.Activities
{
    /// <summary>
    /// This step retrieves the student's Department phone number.
    /// </summary>
    public class RetrieveDepartmentPhoneNumberActivity : ActivityBase
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {

            var logic = executionContext.IocScope.Resolve<IAppointmentService>();
            var tracer = executionContext.LoggerFactory.GetLogger(this.GetType());
            tracer.Trace("Reading In Arguments");
            var departmentId = DepartmentId.Get<EntityReference>(executionContext.ActivityContext);
            var phoneNumber = logic.RetrieveDepartmentPhoneNumberService(new List<object>() { departmentId });
            PhoneNumber.Set(executionContext.ActivityContext, phoneNumber);

        }

        /// <summary>
        /// The department to which the student belongs.
        /// </summary>
        [ReferenceTarget(cmc_department.EntityLogicalName)]
        [RequiredArgument]
        [Input("Department")]
        public InArgument<EntityReference> DepartmentId { get; set; }

        /// <summary>
        /// The phone number of the department which is associated with the student.
        /// </summary>
        [Output("PhoneNumber")]
        public OutArgument<string> PhoneNumber { get; set; }
    }
}
