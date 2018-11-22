using System;
using System.Collections.Generic;
using System.Net;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Functions;
using Cmc.Engage.Common;
using Cmc.Engage.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Cmc.Engage.Lifecycle.Functions
{
    public static class DomEngineOpportunity
    {
        [FunctionName("DomEngineOpportunity")]
        public static void Run([TimerTrigger("%DomEngineOpportunitySchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var registrationModulesList = new List<Type>
            {
                typeof(LifecycleRegistrationModule),
                typeof(CommonRegistrationModule)
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);
            var domService = container.Resolve<IDOMService>();         
            domService.ProcessDomAssignment("opportunity", CommonIocRegistrations.GetNewOrganizationServiceInstance());
        }
    }
}
