using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core.IoC;

namespace Cmc.Engage.Common
{
    /// <summary>
    /// place register/resolve all the class refrences for common directory.
    /// </summary>
    public class CommonRegistrationModule : StaticRegistrationModule
    {
        /// <summary>
        /// override the load method.
        /// </summary>
        /// <param name="container"></param>
        protected override void Load(ContainerBuilder container)
        {
            base.Load(container);

            //plugin 
            container.RegisterType<ContactService>().As<IContactService>().InstancePerLifetimeScope();
            container.RegisterType<ConfigurationService>().As<IConfigurationService>().InstancePerLifetimeScope();
            container.RegisterType<LanguageService>().As<ILanguageService>().InstancePerLifetimeScope();
            
            container.RegisterType<AddressService>().As<IAddressService>().InstancePerLifetimeScope();
            container.RegisterType<MarketingListService>().As<IMarketingListService>().InstancePerLifetimeScope();       
            container.RegisterType<BingMapService>().As<IBingMapService>().InstancePerLifetimeScope();
            container.RegisterType<CustomAttributePickerUIService>().As<ICustomAttributePickerUIService>().InstancePerLifetimeScope();

          
            //PortalWebService plugin
            container.RegisterType<PortalWebServicePluginBase>().As<IPortalWebServicePluginBaseLogic>().InstancePerLifetimeScope();

            container.RegisterType<PortalRetrieveMultiLingualValues>().Named<PortalWebServiceLogicBase>(typeof(PortalRetrieveMultiLingualValues).Name).InstancePerLifetimeScope();
            container.RegisterType<CreateStaffAppointmentPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(CreateStaffAppointmentPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<DeleteStaffAppointmentPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(DeleteStaffAppointmentPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<MarkToDoAsReadPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(MarkToDoAsReadPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<GetTodosPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(GetTodosPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<MarkToDoAsReadPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(MarkToDoAsReadPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<RetrieveContactImageLogic>().Named<PortalWebServiceLogicBase>(typeof(RetrieveContactImageLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<RetrieveNumberOfUnreadToDosPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(RetrieveNumberOfUnreadToDosPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<RetrieveStaffAppointmentsPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(RetrieveStaffAppointmentsPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<RetrieveStaffAvailabilityFromOfficeHoursPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(RetrieveStaffAvailabilityFromOfficeHoursPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<RetrieveStaffDetailsPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(RetrieveStaffDetailsPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<RetrieveStudentSuccessNetworkPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(RetrieveStudentSuccessNetworkPortalLogic).Name).InstancePerLifetimeScope();
            container.RegisterType<TextPhoneNumberUtilitieslogic>().Named<WebServiceLogicBase>(typeof(TextPhoneNumberUtilitieslogic).Name).InstancePerLifetimeScope();
            container.RegisterType<UpdateToDoStatusReasonPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(UpdateToDoStatusReasonPortalLogic).Name).InstancePerLifetimeScope();

            container.RegisterType<GetPortalUserLanguageCode>().InstancePerLifetimeScope();
            container.RegisterType<GetTodosLogic>().InstancePerLifetimeScope();
            container.RegisterType<MarkToDoAsReadLogic>().InstancePerLifetimeScope();
            container.RegisterType<RetrieveNumberOfUnreadToDosLogic>().InstancePerLifetimeScope();
            container.RegisterType<RetrieveStaffDetailsLogic>().InstancePerLifetimeScope();
            container.RegisterType<UpdateToDoStatusReasonLogic>().InstancePerLifetimeScope();
            container.RegisterType<SharePermissionService>().As<ISharePermissionService>().InstancePerLifetimeScope();

            //container.RegisterType<PortalWebServiceLogicBase>().Named<PortalWebServiceLogicBase>(typeof(PortalWebServiceLogicBase).Name).InstancePerLifetimeScope();
            //activities


            //functions

        }
    }
}
