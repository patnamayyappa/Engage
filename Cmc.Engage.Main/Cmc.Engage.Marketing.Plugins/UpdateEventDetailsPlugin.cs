using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExcutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
namespace Cmc.Engage.Marketing.Plugins
{
    /// <summary>
    /// On Update of msevtmgt_EventStartDate,msevtmgt_EventEndDate,msevtmgt_EventTimeZone of event this method will update cmc_startdatetime,cmc_enddatetime of event.
    /// </summary>
    public class UpdateEventDetailsPlugin : PluginBase, IPlugin
    {
        public UpdateEventDetailsPlugin(string unsecuredParameters, string securedParameters) : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExcutionContext context)
        {
            var eventService = context.IocScope.Resolve<IEventService>();
            eventService.UpdateEventDetails(context);
        }
    }
}
