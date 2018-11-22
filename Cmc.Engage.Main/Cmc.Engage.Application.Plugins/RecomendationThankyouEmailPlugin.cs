using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Application.Plugins
{
    public class RecomendationThankyouEmailPlugin : PluginBase
    {
        public RecomendationThankyouEmailPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExecutionContext context)
        {
            var service = context.IocScope.Resolve<IRecommendationService>();
            service.SendThankyouEmail(context);
        }
    }
}
