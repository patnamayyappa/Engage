using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common.Plugins
{
    public class PortalWebServicePlugin : PluginBase, IPlugin
    {
        public PortalWebServicePlugin(string unsecuredParameters, string securedParameters)
        : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var logic = context.IocScope.Resolve<IPortalWebServicePluginBaseLogic>();
            logic.Run(context);
        }
    }
}
