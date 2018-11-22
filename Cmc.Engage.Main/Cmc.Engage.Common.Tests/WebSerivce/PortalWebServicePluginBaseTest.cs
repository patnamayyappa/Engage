using System;
using System.Collections.Generic;
using Autofac;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Communication;
using Cmc.Engage.Models;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Common.Tests.WebSerivce
{

    [TestClass]
    public class PortalWebServicePluginBaseTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void PortalWebServicePlugin_Test()
        {
            #region ARRANGE

            var systemUserInstance = PrepareSystemUser();
            var portalactionInstance = PreparePortalactionInstance();
            var contactInstance = PrepareContactInstance();
            var userlocationInstance = PrepareUserLocation();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                systemUserInstance,
                //portalactionInstance,
                contactInstance
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, portalactionInstance, Operation.RetrieveMultiple, "cmc_portalaction");
            //
            //queryexp.EntityName = "cmc_portalaction";
            //AddInputParameters(mockServiceProvider, "Query", queryexp);

            EntityCollection entityCollection = new EntityCollection();
            entityCollection.Entities.Add(contactInstance);
            AddOutputParameters(mockServiceProvider, "BusinessEntityCollection", entityCollection);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);


            var stringInput = "{'ContactId':'" + contactInstance.Id + "','UserId':'" + systemUserInstance.Id + "','LocationId':'" + userlocationInstance.Id + "','StartDate':'" + DateTime.Today + "','EndDate':'" + DateTime.Today.AddDays(5) + "','Title':'Test Title','Description': 'Test Discripiton'}";
            QueryExpression queryexp = new QueryExpression();
            queryexp.Criteria = new FilterExpression
            {
                Filters ={
                    new FilterExpression{ Conditions={new ConditionExpression("cmc_logicclassname", ConditionOperator.Equal, typeof(CreateStaffAppointmentPortalLogic).AssemblyQualifiedName),
                        new ConditionExpression("cmc_data", ConditionOperator.Equal, stringInput) } }
                }
            };

            AddInputParameters(mockServiceProvider, "Query", queryexp);

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "cmc_portalaction"
            };

            StatusAttributeMetadata enumAttribute = new StatusAttributeMetadata() { LogicalName = "cmc_data" };
            MemoAttributeMetadata enuMemoAttributeMetadata = new MemoAttributeMetadata() { LogicalName = "cmc_data" , MaxLength = 5000};

            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { enumAttribute });
            entityMetadata.SetAttributeCollection(new List<MemoAttributeMetadata>()
            {
                enuMemoAttributeMetadata
            });
            
            xrmFakedContext.SetEntityMetadata(entityMetadata);

            #endregion

            #region ACT
            //Instantiate the Class with the mocked dependencies.
            //Need to mock all the dependency Injections passed to the services constructor
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();

            ContainerBuilder containerBuilder = MockIocScope(mockLogger.Object, xrmFakedContext);
            containerBuilder.RegisterType<LanguageService>().As<ILanguageService>();
            containerBuilder.RegisterType<AppointmentService>().As<IAppointmentService>();
            containerBuilder.RegisterType<CreateStaffAppointmentPortalLogic>().Named<PortalWebServiceLogicBase>(typeof(CreateStaffAppointmentPortalLogic).Name);
            mockExecutionContext.Setup(r => r.IocScope).Returns(containerBuilder.Build().BeginLifetimeScope());

            var portalWebServicePlugin = new PortalWebServicePluginBase(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            portalWebServicePlugin.Run(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            #endregion
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void PortalWebServicePlugin_Test_ArgumentNullException()
        {
            #region ARRANGE
            
            var xrmFakedContext = new XrmFakedContext();
            
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            
            #endregion

            #region ASSERT
            Assert.ThrowsException<ArgumentNullException>(() => new PortalWebServicePluginBase(null, null));
            Assert.ThrowsException<ArgumentException>(() => new PortalWebServicePluginBase(mockLogger.Object, null));
            #endregion
        }
        private Entity PreparePortalactionInstance()
        {
            var portalactionInstance = new Entity("cmc_portalaction", Guid.NewGuid())
            {
                //["cmc_data"]= "Test",
                ["cmc_logicclassname"] = "Test LogicalClassName",
                ["statecode"] = new OptionSetValue(0)
            };
            return portalactionInstance;
        }
        private Entity PrepareContactInstance()
        {
            var contactInstance = new Entity("contact", Guid.NewGuid())
            {
            };
            return contactInstance;
        }
        private SystemUser PrepareSystemUser()
        {
            var systemUserInstance = new SystemUser()
            {
                Id = Guid.NewGuid()
            };
            return systemUserInstance;
        }
        private Entity PrepareUserLocation()
        {
            var userLocation = new Entity("cmc_userlocation", Guid.NewGuid())
            {

            };
            return userLocation;
        }
    }


}
