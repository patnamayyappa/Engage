using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class PredictRetentionActionPlugin : PluginBase, IPlugin
    {
        public PredictRetentionActionPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExecutionContext context)
        {
            var RetentionAction = context.IocScope.Resolve<IContactService>();
            RetentionAction.PredictRetentionAction(context);
        }
    }
}
