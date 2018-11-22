using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class CheckAndValidatePrimaryAddressPlugin:PluginBase, IPlugin
    {
        public CheckAndValidatePrimaryAddressPlugin(string unsecuredParameters, string securedParameters)
          : base(unsecuredParameters, securedParameters) { }

        protected override void Execute(IExecutionContext context)
        {
            var addressService = context.IocScope.Resolve<IAddressService>();
            addressService.CheckAndValidatePrimaryAddress(context);
        }
    }
}
