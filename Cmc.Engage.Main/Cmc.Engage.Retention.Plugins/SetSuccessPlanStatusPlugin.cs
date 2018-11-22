using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
    public class SetSuccessPlanStatusPlugin : PluginBase, IPlugin
    {
        public SetSuccessPlanStatusPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var SuccessplanStatus = context.IocScope.Resolve<ISuccessplanService>();
            SuccessplanStatus.SetSuccessPlanStatus(context);
        }
    }
}
