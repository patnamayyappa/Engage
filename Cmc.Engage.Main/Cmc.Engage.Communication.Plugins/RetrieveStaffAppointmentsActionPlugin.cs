using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExcutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Communication.Plugins
{
    public class RetrieveStaffAppointmentsActionPlugin : PluginBase, IPlugin
    {
        public RetrieveStaffAppointmentsActionPlugin(string unsecuredParameters, string securedParameters)
   : base(unsecuredParameters, securedParameters)
        {

        }
        protected override void Execute(IExcutionContext context)
        {
            var logic = context.IocScope.Resolve<IAppointmentService>();
            logic.RetrieveStaffAppointments(context);
        }
    }
}
