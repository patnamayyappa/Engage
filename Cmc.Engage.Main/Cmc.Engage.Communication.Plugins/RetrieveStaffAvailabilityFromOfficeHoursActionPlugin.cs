using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExcutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Communication.Plugins
{
    public class RetrieveStaffAvailabilityFromOfficeHoursActionPlugin : PluginBase, IPlugin
    {
        public RetrieveStaffAvailabilityFromOfficeHoursActionPlugin(string unsecuredParameters, string securedParameters) : base(unsecuredParameters, securedParameters)
        {

        }

        protected override void Execute(IExcutionContext context)
        {
            var logic = context.IocScope.Resolve<IAppointmentService>();
            logic.RetrieveStaffAvailability(context);
        }
    }
}
