using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention.Plugins
{
    public class CopySuccessPlanTemplatePlugin : PluginBase, IPlugin
    {
        public CopySuccessPlanTemplatePlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var CopySuccessPlan = context.IocScope.Resolve<ISuccessplanService>();
            CopySuccessPlan.CopySuccessPlanTemplate(context);
        }
    }
}
