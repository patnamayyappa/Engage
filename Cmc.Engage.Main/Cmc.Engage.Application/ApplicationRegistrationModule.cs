using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;
using Cmc.Engage.Application.Contracts;
using Cmc.Engage.Application.Contracts.Interfaces;

namespace Cmc.Engage.Application
{
    public class ApplicationRegistrationModule : StaticRegistrationModule
    {
        protected override void Load(ContainerBuilder container)
        {
            base.Load(container);
            container.RegisterType<ApplicationService>().As<IApplicationService>().InstancePerLifetimeScope();
            container.RegisterType<InvoiceService>().As<IInvoiceService>().InstancePerLifetimeScope();
            container.RegisterType<TestScoreService>().As<ITestScoreService>().InstancePerLifetimeScope();
            container.RegisterType<ApplicationRequirementsDefinitionService>().As<IApplicationRequirementsDefinitionService>().InstancePerLifetimeScope();
            container.RegisterType<RecommendationService>().As<IRecommendationService>().InstancePerLifetimeScope();
        }
    }
}
