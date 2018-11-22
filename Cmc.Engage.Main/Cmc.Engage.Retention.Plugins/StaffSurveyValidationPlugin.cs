using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
	public class StaffSurveyValidationPlugin:PluginBase, IPlugin
	{
		public StaffSurveyValidationPlugin(string unsecuredParameters, string securedParameters)
		 : base(unsecuredParameters, securedParameters)
		{

		}

		protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
		{
			var staffSurveyService = context.IocScope.Resolve<IStaffSurveyService>();
			staffSurveyService.ValidateSurveyInstance(context);
		}
	}
}
