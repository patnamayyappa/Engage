using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle.Plugins
{
    public class ValidateDomDefinitionExecutionOrderPlugin : PluginBase, IPlugin
    {
        public ValidateDomDefinitionExecutionOrderPlugin(string unsecuredParameters, string securedParameters) 
            : base(unsecuredParameters, securedParameters) { }      
        protected override void Execute(IExecutionContext context)
        {
            var ValidateDomDefinitionExecutionOrder = context.IocScope.Resolve<IDomDefinitionExecutionOrderService>();
            ValidateDomDefinitionExecutionOrder.ValidateDomDefinitionExecutionOrder(context);
        }
    }
}
