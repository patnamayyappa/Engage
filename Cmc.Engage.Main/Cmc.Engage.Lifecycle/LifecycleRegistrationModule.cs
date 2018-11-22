using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;

namespace Cmc.Engage.Lifecycle
{
    public class LifecycleRegistrationModule : StaticRegistrationModule
    {
        protected override void Load(ContainerBuilder container)
        {
            base.Load(container);
            container.RegisterType<DOMService>().As<IDOMService>().InstancePerLifetimeScope();
            container.RegisterType<DomDefinitionService>().As<IDomDefinitionService>().InstancePerLifetimeScope();
            container.RegisterType<DomDefinitionLogicService>().As<IDomDefinitionLogicService>().InstancePerLifetimeScope();
            container.RegisterType<DomDefinitionExecutionOrderService>().As<IDomDefinitionExecutionOrderService>().InstancePerLifetimeScope();
            container.RegisterType<DomMasterService>().As<IDomMasterService>().InstancePerLifetimeScope();
            container.RegisterType<InboundInterestService>().As<IInboundInterestService>().InstancePerLifetimeScope();
            container.RegisterType<LifecycleService>().As<ILifecycleService>().InstancePerLifetimeScope();
            container.RegisterType<TripActivityService>().As<ITripActivityService>().InstancePerLifetimeScope();
            container.RegisterType<TripService>().As<ITripService>().InstancePerLifetimeScope();
            container.RegisterType<TripApprovalProcessService>().As<ITripApprovalProcessService>().InstancePerLifetimeScope();
        }
    }
}
