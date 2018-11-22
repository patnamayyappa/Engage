using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common.Plugins
{
    public class ComputeMilesFromHomePlugin : PluginBase, IPlugin
    {
        public ComputeMilesFromHomePlugin(string unsecuredParameters, string securedParameters)
          : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var contactService = context.IocScope.Resolve<IContactService>();
            contactService.ComputeMilesFromHome(context);
        }
    }
}
