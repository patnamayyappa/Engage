using System;
using System.Collections.Generic;
using Autofac;
using Cmc.Engage.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;


namespace Cmc.Engage.Retention.Functions
{
    public static class SendStaffSurveyReminder
    {
        [FunctionName("SendStaffSurveyReminder")]
        public static void Run([TimerTrigger("%SendStaffSurveyReminderSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var registrationModulesList = new List<Type>
            {
                typeof(RetentionsRegistrationModule),
                typeof(CommonRegistrationModule)
            };

            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);
            var staffSurveyService = container.Resolve<IStaffSurveyService>();
            staffSurveyService.SendStaffSurveyReminderEmail();
        }
    }

}

