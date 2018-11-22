using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class ActivateStudentGroupPlugin : PluginBase, IPlugin
    {
        public ActivateStudentGroupPlugin(string unsecuredParameters, string securedParameters) : base(unsecuredParameters, securedParameters)
        {
        }
        protected override void Execute(IExecutionContext context)
        {
            var marketingListService = context.IocScope.Resolve<IMarketingListService>();
            marketingListService.ActivateStudentGroup(context);

        }
    }
}
