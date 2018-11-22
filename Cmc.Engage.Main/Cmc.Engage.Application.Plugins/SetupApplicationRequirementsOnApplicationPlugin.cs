using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Microsoft.Xrm.Sdk;
using System;
using Cmc.Engage.Application.Contracts;

namespace Cmc.Engage.Application.Plugins
{
    public class SetupApplicationRequirementsOnApplicationPlugin : PluginBase, IPlugin
    {
        public SetupApplicationRequirementsOnApplicationPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {
        }

        protected override void Execute(IExecutionContext context)
        {
            var applicationService = context.IocScope.Resolve<IApplicationService>();
            applicationService.SetupApplicationRequirements(context);
        }
    }
}
