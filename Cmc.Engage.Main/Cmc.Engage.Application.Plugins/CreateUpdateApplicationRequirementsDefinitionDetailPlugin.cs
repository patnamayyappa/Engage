using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Application.Contracts.Interfaces;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Application.Plugins
{
    public class CreateUpdateApplicationRequirementsDefinitionDetailPlugin: PluginBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unsecuredParameters"></param>
        /// <param name="securedParameters"></param>
        public CreateUpdateApplicationRequirementsDefinitionDetailPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var service = context.IocScope.Resolve<IApplicationRequirementsDefinitionService>();
            service.CreateUpdateApplicationRequirementsDefinitionDetail(context);
        }
    }
}
