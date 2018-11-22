using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using IExcutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Communication.Plugins
{
    public class CreateStaffAppointmentActionPlugin : PluginBase, IPlugin
    {
        public CreateStaffAppointmentActionPlugin(string unsecuredParameters, string securedParameters)
            : base(unsecuredParameters, securedParameters)
        {

        }


        protected override void Execute(IExcutionContext context)
        {
            var logic = context.IocScope.Resolve<IAppointmentService>();
            logic.CreateStaffAppointment(context);
        }
    }
}
