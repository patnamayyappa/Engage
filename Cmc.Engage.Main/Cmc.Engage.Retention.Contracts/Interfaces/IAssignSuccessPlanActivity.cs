using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Contracts;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention
{
  public  interface IAssignSuccessPlanActivity
    {
        AssignSuccessPlanResponse RunActivity(IActivityExecutionContext executionContext, ICreateStudentSuccessPlansFromTemplatePluginService createStudentSuccessPlansFromTemplatePluginService, EntityReference studentId, EntityReference successPlanTemplateId);
    }
}
