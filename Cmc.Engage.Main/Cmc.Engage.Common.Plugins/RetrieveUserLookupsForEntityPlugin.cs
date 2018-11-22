using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class RetrieveUserLookupsForEntityPlugin : PluginBase, IPlugin
    {
        public RetrieveUserLookupsForEntityPlugin(string unsecuredParameters, string securedParameters) : base(unsecuredParameters, securedParameters)
        {
        }

        protected override void Execute(IExecutionContext context)
        {
            var customAttributePicker = context.IocScope.Resolve<ICustomAttributePickerUIService>();
            customAttributePicker.RetrieveUserLookupsForEntity(context);
        }
    }
}
