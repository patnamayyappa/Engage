using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;

namespace Cmc.Engage.Marketing
{
    /// <summary>
    /// place register/resolve all the class refrences for marketing directory.
    /// </summary>
    public class MarketingRegistrationModule : StaticRegistrationModule
    {
        /// <summary>
        /// override the load method.
        /// </summary>
        /// <param name="container"></param>
        protected override void Load(ContainerBuilder container)
        {
            base.Load(container);
            container.RegisterType<EventService>().As<IEventService>().InstancePerDependency();

        }
    }
}




