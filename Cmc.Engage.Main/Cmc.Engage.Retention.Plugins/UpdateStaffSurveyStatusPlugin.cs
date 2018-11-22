using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
    public class UpdateStaffSurveyStatusPlugin : PluginBase, IPlugin
    {
        public UpdateStaffSurveyStatusPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var StaffSurveyStatus = context.IocScope.Resolve<IStaffSurveyService>();
            StaffSurveyStatus.UpdateStaffSurveyCompletedCancellationDate(context);
        }
    }
}