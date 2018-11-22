using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Cmc.Engage.Common.Functions
{
    public static class StudentGroupAutoExpire
    {
        [FunctionName("StudentGroupAutoExpire")]
        public static void Run([TimerTrigger("%StudentGroupAutoExpireSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var registrationModulesList = new List<Type>
            {
                typeof(CommonRegistrationModule)
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);
            var marketingListService = container.Resolve<IMarketingListService>();       
            marketingListService.StudentGroupAutoExpireLogic();
        }
    }
}
