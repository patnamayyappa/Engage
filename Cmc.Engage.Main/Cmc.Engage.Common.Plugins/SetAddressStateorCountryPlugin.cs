using System;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common.Plugins
{
    public class SetAddressStateorCountryPlugin: PluginBase, IPlugin
    {
        public SetAddressStateorCountryPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }
        protected override void Execute(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var AddressService = context.IocScope.Resolve<IAddressService>();
            AddressService.SetAddressStateorCountry(context);
        }
    }
}
