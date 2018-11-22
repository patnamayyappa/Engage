using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
	public class CopyStaffSurveyTemplatePlugin : PluginBase, IPlugin
	{
		public CopyStaffSurveyTemplatePlugin(string unsecuredParameters, string securedParameters)
			: base(unsecuredParameters, securedParameters)
		{

		}

		protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
		{
			var CopyStaffSurveyTemplate = context.IocScope.Resolve<IStaffSurveyService>();
			CopyStaffSurveyTemplate.CopyStaffSurveyTemplate(context);
		}
	}
}
