using Cmc.Engage.Contracts;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention
{
  public  interface ICreateStudentSuccessPlansFromTemplatePluginService
    {
        void Run(Core.Xrm.ServerExtension.Core.IExecutionContext context);
        AssignSuccessPlanResponse CreateSuccessPlansForSelectedStudents(EntityReference successPlanTemplateId, EntityCollection students);
    }
}
