using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmc.Engage.Common;

namespace Cmc.Engage.Retention.Functions
{
    public static class UpdateOverdueStaffSurveys
    {
        [FunctionName("UpdateOverdueStaffSurveys")]
        public static void Run([TimerTrigger("%UpdateOverdueStaffSurveysSchedule%")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            log.Info("Azure Funtion to Update Overdue Staffsurveys...");
            var registrationModulesList = new List<Type>
            {
                typeof(RetentionsRegistrationModule),
                typeof(CommonRegistrationModule)
            };
            var container = FunctionExtensions.FunctionExtensions.GetServiceLocator(registrationModulesList);
            var StaffSurveyService = container.Resolve<IStaffSurveyService>();
            StaffSurveyService.UpdateSurveysCrossedDuedate();
        }
    }
}
