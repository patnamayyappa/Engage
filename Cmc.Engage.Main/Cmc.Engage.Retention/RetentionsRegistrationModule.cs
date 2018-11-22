using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;

namespace Cmc.Engage.Retention
{
    public class RetentionsRegistrationModule : StaticRegistrationModule
    {
        protected override void Load(ContainerBuilder container)
        {
            base.Load(container);

            //plugin 
            container.RegisterType<SuccessPlanService>().As<ISuccessplanService>().InstancePerLifetimeScope();
            container.RegisterType<ToDoService>().As<IToDoService>().InstancePerLifetimeScope();
			container.RegisterType<StaffSurveyService>().As<IStaffSurveyService>().InstancePerLifetimeScope();
            container.RegisterType<ScoreDefinitionService>().As<IScoreDefinitionService>().InstancePerLifetimeScope();
            //activities

            //functions
            container.RegisterType<RetentionScoreCalculatorService>().As<IRetentionScoreCalculatorService>().InstancePerLifetimeScope();
            container.RegisterType<SuccessNetworkService>().As<ISuccessNetworkService>().InstancePerLifetimeScope();

        }
    }
}
