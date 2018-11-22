using System;
using System.Collections.Generic;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Functions;
using Cmc.Engage.Common;
using Cmc.Engage.FunctionExtensions;
using Cmc.Engage.Retention;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Cmc.Engage.Retentions.Functions
{
    public static class SuccessPlanAssignment
    {
        [FunctionName("SuccessPlanAssignment")]
        public static void Run([TimerTrigger("%SuccessPlanAssignmentSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {            
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var registrationModulesList = new List<Type>
            {
                typeof(CommonRegistrationModule),             
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);         
            var contactService = container.Resolve<IContactService>();         
            contactService.SuccessPlanAssignmentLogic(CommonIocRegistrations.GetNewOrganizationServiceInstance());
        }
    }
}
