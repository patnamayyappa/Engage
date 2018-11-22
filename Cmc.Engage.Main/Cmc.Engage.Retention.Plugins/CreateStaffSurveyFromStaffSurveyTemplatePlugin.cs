using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
	public class CreateStaffSurveyFromStaffSurveyTemplatePlugin : PluginBase, IPlugin
	{
		public CreateStaffSurveyFromStaffSurveyTemplatePlugin(string unsecuredParameters, string securedParameters)
		 : base(unsecuredParameters, securedParameters)
		{

		}
		protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
		{
			var staffSurveyService = context.IocScope.Resolve<IStaffSurveyService>();
			staffSurveyService.CreateStaffSurveyFromTemplate(context);
		}
	}
}
