using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExcutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Communication.Plugins
{
    /// <summary>
    /// Update TripActivity Appointment Details
    /// </summary>
    public class UpdateTripActivityAppointmentDetailsPlugin : PluginBase, IPlugin
    {
        /// <summary>
        /// Update TripActivity Appointment Details
        /// </summary>
        /// <param name="unsecuredParameters"></param>
        /// <param name="securedParameters"></param>
        public UpdateTripActivityAppointmentDetailsPlugin(string unsecuredParameters, string securedParameters) : base(unsecuredParameters, securedParameters) { }
        protected override void Execute(IExcutionContext context)                                       
        {
            var appointmentService = context.IocScope.Resolve<IAppointmentService>();
            appointmentService.UpdateTripActivityAppointmentDetails(context);
        }
    }   
}











