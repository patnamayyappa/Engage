using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;

namespace Cmc.Engage.Communication
{
    public class CommunicationRegistrationModule : StaticRegistrationModule
    {
        protected override void Load(ContainerBuilder container)
        {
            base.Load(container);
            container.RegisterType<AppointmentService>().As<IAppointmentService>().InstancePerLifetimeScope();
        }
    }
}
