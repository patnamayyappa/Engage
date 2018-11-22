using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;


namespace Cmc.Engage.Common.Functions
{
    public static class CampusDistanceCalculator
    {
        [FunctionName("CampusDistanceCalculator")]
        public static void Run([TimerTrigger("%CampusDistanceCalculatorSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var registrationModulesList = new List<Type>
            {
                typeof(CommonRegistrationModule)
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);
            var contactService = container.Resolve<IContactService>();         
            contactService.CampusDistanceCalculatorLogic();
        }
    }
}
