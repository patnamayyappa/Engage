using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle.Plugins
{
    public class ValidateDomMasterPlugin : PluginBase, IPlugin
    {
        public ValidateDomMasterPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var ValidateDomMaster = context.IocScope.Resolve<IDomMasterService>();
            ValidateDomMaster.ValidateDomMasterService(context);
        }
    }
}
