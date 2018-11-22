using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Application.Plugins
{
    public class SetDefaultFieldsOnApplicationPlugin : PluginBase, IPlugin
    {
        public SetDefaultFieldsOnApplicationPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {
        }

        protected override void Execute(IExecutionContext context)
        {
            var applicationService = context.IocScope.Resolve<IApplicationService>();
            applicationService.SetDefaultFields(context);
        }
    }
}
