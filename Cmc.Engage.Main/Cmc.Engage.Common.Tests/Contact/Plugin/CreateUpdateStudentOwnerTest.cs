using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using cmc_successnetwork = Cmc.Engage.Models.cmc_successnetwork;
using SystemUser = Cmc.Engage.Models.SystemUser;

namespace Cmc.Engage.Common.Tests.Contact.Plugin
{
    [TestClass]
    public class CreateUpdateStudentOwnerTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]

        public void CreateAndUpdateStudentOwner_CreateofContact()
        {
            #region ARRANGE
            var creatingcontact = PreparingContact();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
               creatingcontact

            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingcontact, Operation.Create);

             //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            //Mock the IBingMap
            var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            var contactService = new ContactService(mockLogger.Object, mockBingServices, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateUpdateStudentOwner(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var dataSuccessNetwork = xrmFakedContext.Data["cmc_successnetwork"];
            Assert.AreEqual(dataSuccessNetwork.Count, 1);

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateAndUpdateStudentOwner_UpdateofContact()
        {
            #region ARRANGE

            var systemUser = PrepareSystemUser();
            var systemUser1 = PrepareSystemUser();
            var creatingcontact = PreparingContactInstance(systemUser);
            var cmcTitleEntity = PrepareTitle();
            var successNetwork = PrepareSuccessNetwork(creatingcontact, cmcTitleEntity, systemUser1);
            var contactPreImage = PreparePreImageContact(creatingcontact, systemUser1);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingcontact,
                cmcTitleEntity,
                systemUser,
                successNetwork,
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingcontact, Operation.Update);
            AddPreEntityImage(mockServiceProvider,"PreImage", contactPreImage);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            //Mock the IBingMap
            var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            var contactService = new ContactService(mockLogger.Object, mockBingServices, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateUpdateStudentOwner(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            //Assert if the business logic performed is correct.
            Entity ValidData = new Entity("cmc_successnetwork");
            xrmFakedContext.Data["cmc_successnetwork"].TryGetValue(successNetwork.Id, out ValidData);
            var real = ValidData.Attributes["cmc_studentid"];
            Assert.AreEqual(creatingcontact.Id, ((EntityReference)real).Id);
            #endregion
        }

        private Entity PreparePreImageContact(Entity contact,Entity systemUser)
        {
            return new Entity("contact")
            {
                Id = contact.Id,
                ["fullname"]=contact.GetAttributeValue<string>("fullname"),
                ["cmc_isstudent"] = true,
                ["ownerid"]=systemUser.ToEntityReference()//contact.GetAttributeValue<EntityReference>("ownerid")
            };
        }

        private Entity PreparingContact()
        {
            var contact1 = new Entity("contact", Guid.NewGuid())
            {
               // Id = Guid.NewGuid(),
                ["fullname"] = "ankur k",
                ["cmc_isstudent"]=true
            };
            return contact1;
        }
        private Entity PreparingContactInstance(Entity systemUserEntity)
        {
            var contact1 = new Entity("contact", Guid.NewGuid())
            {
                ["fullname"] = "Test Contact",
                ["cmc_isstudent"] = true,
                ["ownerid"]= systemUserEntity.ToEntityReference()
            };
            return contact1;
        }

        private cmc_successnetwork PrepareSuccessNetwork(Entity contactEntity, Entity cmcTitleEntity,Entity systemUserEntity)
        {
            return new cmc_successnetwork()
            {
                Id = Guid.NewGuid(),
                cmc_studentid = contactEntity.ToEntityReference(),
                cmc_staffmemberid = systemUserEntity.ToEntityReference(),
                cmc_staffroleid = cmcTitleEntity.ToEntityReference(),
            };
        }

        private cmc_title PrepareTitle()
        {
            return new cmc_title()
            {
                Id = Guid.NewGuid(),
                cmc_titlename = "Advisor",
            };
        }
        private SystemUser PrepareSystemUser()
        {
            return new SystemUser()
            {
                Id = Guid.NewGuid()
            };
        }
    }
}
