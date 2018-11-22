using System;
using System.Collections.Generic;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;
using Cmc.Core.Xrm.ServerExtension.Functions;
using Cmc.Engage.Common;
using Cmc.Engage.FunctionExtensions;
using Cmc.Engage.Retention;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Cmc.Engage.Retentions.Functions
{
    public static class SuccessNetworkAssignment
    {
        [FunctionName("SuccessNetworkAssignment")]
        public static void Run([TimerTrigger("%SuccessNetworkAssignmentSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
                      
            var registrationModulesList = new List<Type>
            {
                typeof(CommonRegistrationModule),
                typeof(RetentionsRegistrationModule)
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);         
            var successNetworkService = container.Resolve<ISuccessNetworkService>();          
            successNetworkService.SuccessNetworkAssignment(CommonIocRegistrations.GetNewOrganizationServiceInstance());
        }
    }
}
