using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Communication;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.WebSerivce
{
    
    [TestClass]
    public class CreateStaffAppointmentPortalTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]

        [TestMethod]
        public void CreateStaffAppointmentPortal_Test()
        {
            #region ARRANGE

            var systemUserInstance = PrepareSystemUser();
            var userlocationInstance = PrepareUserLocation();
            var contactInstance = PrepareContactInstance();
            var activityPartyInstance = PrepareActivityParty(systemUserInstance.Id, contactInstance.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                systemUserInstance,
                activityPartyInstance,
                //userlocationInstance,
                contactInstance
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contactInstance, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockAppointmentService = new AppointmentService(mockLogger.Object,xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);

            var stringInput = "{'ContactId':'" + contactInstance.Id+ "','UserId':'" + systemUserInstance.Id + "','LocationId':'" + userlocationInstance.Id + "','StartDate':'" + DateTime.Today+ "','EndDate':'" + DateTime.Today.AddDays(5)+ "','Title':'Test Title','Description': 'Test Discripiton'}";
            var createStaffAppointmentPortalLogic = new CreateStaffAppointmentPortalLogic(mockLogger.Object, mockAppointmentService, xrmFakedContext.GetFakedOrganizationService());
            
            var createdAppointmentJson=createStaffAppointmentPortalLogic.DoWork(mockExecutionContext.Object, stringInput);
            #endregion

            #region ASSERT
            Assert.IsNotNull(createdAppointmentJson);
            #endregion
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void CreateStaffAppointmentPortal_Test_ArgumentException()
        {
            #region ARRANGE

            var xrmFakedContext = new XrmFakedContext();
                        
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            #endregion

            #region ASSERT
            Assert.ThrowsException<ArgumentException>(() => new CreateStaffAppointmentPortalLogic(null, null, null));
            Assert.ThrowsException<ArgumentException>(() => new CreateStaffAppointmentPortalLogic(mockLogger.Object, null, null));
            Assert.ThrowsException<ArgumentException>(() => new CreateStaffAppointmentPortalLogic(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentNullException>(() => new AppointmentService(null, null, mockILanguageService.Object));
            #endregion
        }
        #region Data Preparation
        private SystemUser PrepareSystemUser()
        {
            var systemUserInstance=new SystemUser()
            {
                Id = Guid.NewGuid()
            };
            return systemUserInstance;
        }

        private Models.Contact PrepareContactInstance()
        {
            var contactInstance=new Models.Contact()
            {
                ContactId = Guid.NewGuid()              
            };
            return contactInstance;
        }

        private Entity PrepareUserLocation()
        {
            var userLocation=new Entity("cmc_userlocation",Guid.NewGuid())
            {

            };
            return userLocation;
        }

        private Entity PrepareActivityParty(Guid userId,Guid contactId)
        {
            var activityParty=new Entity("activityparty",Guid.NewGuid())
            {
                ["partyid"]=new EntityReference("systemuser", userId),
                ["partyid"] = new EntityReference("contact", contactId),
            };
            return activityParty;
        }

        #endregion
    }
}
