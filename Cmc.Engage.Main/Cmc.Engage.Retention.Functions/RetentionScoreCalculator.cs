using System;
using System.Collections.Generic;
using Autofac;
using Cmc.Engage.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Cmc.Engage.Retention;
using Cmc.Engage.Lifecycle;

namespace Cmc.Engage.Retentions.Functions
{
    public static class RetentionScoreCalculator
    {
        [FunctionName("RetentionScoreCalculator")]
        public static void Run([TimerTrigger("%RetentionScoreCalculatorSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {          
            var registrationModulesList = new List<Type>
            {
                typeof(CommonRegistrationModule),
				typeof(LifecycleRegistrationModule),
                typeof(RetentionsRegistrationModule),
				
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);
            var retentionScoreCalculatorService = container.Resolve<IRetentionScoreCalculatorService>();
            retentionScoreCalculatorService.RetentionScoreLogic();
        }
    }
}
