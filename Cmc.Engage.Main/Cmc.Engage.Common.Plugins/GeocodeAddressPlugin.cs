using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common.Plugins
{
    public class GeocodeAddressPlugin : PluginBase, IPlugin
    {
        public GeocodeAddressPlugin(string unsecuredParameters, string securedParameters)
         : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var addressService = context.IocScope.Resolve<IAddressService>();
            addressService.GeocodeAddress(context);
        }
    }
}
