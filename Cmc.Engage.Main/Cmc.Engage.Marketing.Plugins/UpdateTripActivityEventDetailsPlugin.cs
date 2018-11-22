using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExcutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Marketing.Plugins
{
    /// <summary>
    /// Update TripActivity Appointment Details
    /// </summary>
    public class UpdateTripActivityEventDetailsPlugin : PluginBase, IPlugin
    {
        /// <summary>
        /// Update TripActivity Appointment Details
        /// </summary>
        /// <param name="unsecuredParameters"></param>
        /// <param name="securedParameters"></param>
        public UpdateTripActivityEventDetailsPlugin(string unsecuredParameters, string securedParameters) : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExcutionContext context)
        {
            var eventService = context.IocScope.Resolve<IEventService>();
            eventService.UpdateTripActivityEventDetails(context);
        }
    }
}
