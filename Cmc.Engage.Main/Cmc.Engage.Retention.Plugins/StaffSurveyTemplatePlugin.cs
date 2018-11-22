using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
	public class StaffSurveyTemplatePlugin: PluginBase, IPlugin
	{
		public StaffSurveyTemplatePlugin(string unsecuredParameters, string securedParameters)
		 : base(unsecuredParameters, securedParameters)
		{

		}

		protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
		{
			var staffSurveyService = context.IocScope.Resolve<IStaffSurveyService>();
			staffSurveyService.SaveStaffSurveyTemplate(context);
		}
	}
}
