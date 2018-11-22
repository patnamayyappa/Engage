using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
namespace Cmc.Engage.Lifecycle.Plugins
{
    public class ValidateDomDefinitionPlugin : PluginBase, IPlugin
    {
        public ValidateDomDefinitionPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var ValidateDomDefinition = context.IocScope.Resolve<IDomDefinitionService>();
            ValidateDomDefinition.ValidateDomDefinition(context);
        }
    }
}
