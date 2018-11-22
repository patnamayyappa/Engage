using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Retention.Plugins
{
    public class AssociateDisassociateScoreDefinitionPlugin : PluginBase, IPlugin
    {
        public AssociateDisassociateScoreDefinitionPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }

        protected override void Execute(IExecutionContext context)
        {
            var scoreDefinitionService = context.IocScope.Resolve<IScoreDefinitionService>();
            scoreDefinitionService.AssociateDisassociateScoreDefinition(context);
        }
    }

}
