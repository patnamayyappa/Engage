using System;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
	public class StaffSurveyPostOperationPlugin: PluginBase, IPlugin
	{
		public StaffSurveyPostOperationPlugin(string unsecuredParameters, string securedParameters)
		 : base(unsecuredParameters, securedParameters)
		{

		}

		protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
		{
			var facultySurveyTemplatePluginService = context.IocScope.Resolve<IStaffSurveyService>();
			facultySurveyTemplatePluginService.PerformStaffSurveyPostOperationLogic(context);
		}
	}
}
