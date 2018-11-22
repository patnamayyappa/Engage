using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle.Plugins
{
    public class ValidateDomDefinitionLogicPlugin : PluginBase, IPlugin
    {
        protected override void Execute(IExecutionContext context)
        {
            var ValidateDomDefinitionLogic = context.IocScope.Resolve<IDomDefinitionLogicService>();
            ValidateDomDefinitionLogic.ValidateDomDefinitionLogic(context);
        }
    }
}
