using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Retention.Plugins
{
	public class CreateStaffSurveyQuestionResponsePlugin : PluginBase, IPlugin
	{
		public CreateStaffSurveyQuestionResponsePlugin(string unsecuredParameters, string securedParameters)
			: base(unsecuredParameters, securedParameters)
		{

		}
		protected override void Execute(IExecutionContext context)
		{
			var CreateQuestionResponse = context.IocScope.Resolve<IStaffSurveyService>();
			CreateQuestionResponse.CreateStaffSurveyQuestionResponses(context);
		}
	}
}

