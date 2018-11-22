using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System.Activities;
using Cmc.Engage.Models;
using System.Linq;

namespace Cmc.Engage.Common.Activities
{
    /// <summary>
    /// This step consumes a Student Id and SuccessPlanTemplate Id as input values and it then assigns a success plan to the selected student.
    /// </summary>
    public class AssignSuccessPlanActivity : ActivityBase
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            var assignSuccessplanService = executionContext.IocScope.Resolve<IContactService>();
            //var CreateStudentSuccessPlansFromTemplatePluginService = executionContext.IocScope.Resolve<ICreateStudentSuccessPlansFromTemplatePluginService>();
            var tracer = executionContext.LoggerFactory.GetLogger(this.GetType());
            tracer.Error("Reading In Arguments new Framework demo");
            var studentId = StudentId.Get(executionContext.ActivityContext);
            var successPlanTemplateId = SuccessPlanTemplateId.Get(executionContext.ActivityContext);

            var result = assignSuccessplanService.AssignSuccessPlan(studentId, successPlanTemplateId);

            if (result?.SuccessPlanIds.Any() == true)
            {
                SuccessPlanId.Set(executionContext.ActivityContext, result.SuccessPlanIds.FirstOrDefault());
            }
        }

        /// <summary>
        /// The ID of the Contact with which the success plan will be associated.
        /// </summary>
        [ReferenceTarget(Contact.EntityLogicalName)]
        [RequiredArgument]
        [Input("StudentId")]
        public InArgument<EntityReference> StudentId { get; set; }

        /// <summary>
        /// The ID of the Success Plan Template with which the Contact will be associated.
        /// </summary>
        [ReferenceTarget(cmc_successplantemplate.EntityLogicalName)]
        [RequiredArgument]
        [Input("SuccessPlanTemplateId")]
        public InArgument<EntityReference> SuccessPlanTemplateId { get; set; }

        /// <summary>
        /// The ID of the resultant success plan record.
        /// </summary>
        [ReferenceTarget(cmc_successplan.EntityLogicalName)]
        [Output("StudentSuccessPlanId")]
        public OutArgument<EntityReference> SuccessPlanId { get; set; }
    }
}
