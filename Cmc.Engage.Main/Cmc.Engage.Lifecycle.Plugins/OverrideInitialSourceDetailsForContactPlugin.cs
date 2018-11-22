using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;


namespace Cmc.Engage.Lifecycle.Plugins
{
  public  class OverrideInitialSourceDetailsForContactPlugin: PluginBase
    {
        public OverrideInitialSourceDetailsForContactPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var inboundInterestService = context.IocScope.Resolve<IInboundInterestService>();
            inboundInterestService.OverrideInitialSourceDetailsForContact(context);
        }
    }
}
