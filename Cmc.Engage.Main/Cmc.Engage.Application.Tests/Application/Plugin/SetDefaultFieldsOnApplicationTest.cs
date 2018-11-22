using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Moq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using FakeXrmEasy;

namespace Cmc.Engage.Application.Tests.Application.Plugin
{
    [TestClass]
    public class SetDefaultFieldsOnApplicationTest : XrmUnitTestBase
    {
        private int _contactCounter = 0;
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_SetContactAndApplicationStatusOnCreate()
        {
            #region Arrange
            var contact = PrepareContact();
            var applicationRegistration = PrepareApplicationRegistration(contact.ToEntityReference());
            var application = PrepareApplication(applicationRegistration.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                applicationRegistration
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertApplicationFieldsSet(application, applicationRegistration, orgService);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_SetContactSkipApplicationStatusOnCreate()
        {
            #region Arrange
            var contact = PrepareContact();
            var applicationRegistration = PrepareApplicationRegistration(contact.ToEntityReference());
            var application = PrepareApplication(applicationRegistration.ToEntityReference());
            application.cmc_applicationstatus = new OptionSetValue((int)cmc_applicationstatus.UnderReview);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                applicationRegistration
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertApplicationFieldsSet(application, applicationRegistration, orgService,
                cmc_applicationstatus.UnderReview);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_NoApplicationRegistrationOnCreate()
        {
            #region Arrange
            var application = PrepareApplication(null);

            var xrmFakedContext = new XrmFakedContext();

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertApplicationFieldsSet(application, null, orgService);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_SetContactAndApplicationStatusOnCreateOverwriteContact()
        {
            #region Arrange
            var contact = PrepareContact();
            var originalContact = PrepareContact();
            var applicationRegistration = PrepareApplicationRegistration(contact.ToEntityReference());
            var application = PrepareApplication(applicationRegistration.ToEntityReference());
            application.cmc_contactid = originalContact.ToEntityReference();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                originalContact,
                applicationRegistration
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertApplicationFieldsSet(application, applicationRegistration, orgService);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_SetContactAndApplicationStatusOnUpdate()
        {
            #region Arrange
            var contact = PrepareContact();
            var applicationRegistration = PrepareApplicationRegistration(contact.ToEntityReference());
            var applicationPreImage = PrepareApplication(null);
            var application = PrepareApplication(applicationRegistration.ToEntityReference());
            application.Id = applicationPreImage.Id;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                applicationRegistration,
                applicationPreImage
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", applicationPreImage);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertApplicationFieldsSet(application, applicationRegistration, orgService);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_SetContactAndApplicationStatusOnUpdateAfterClear()
        {
            #region Arrange
            var contact = PrepareContact();
            var otherContact = PrepareContact();

            var applicationRegistration = PrepareApplicationRegistration(
                contact.ToEntityReference());

            var applicationPreImage = PrepareApplication(null);
            applicationPreImage.cmc_contactid = otherContact.ToEntityReference();
            applicationPreImage.cmc_applicationstatus = new OptionSetValue(
                (int)cmc_applicationstatus.Complete);
            
            // Clear Application Status to make sure it is overwritten. Contact should be overwritten
            var application = PrepareApplication(applicationRegistration.ToEntityReference());
            application.cmc_applicationstatus = null;
            application.Id = applicationPreImage.Id;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                applicationRegistration,
                applicationPreImage
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", applicationPreImage);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertApplicationFieldsSet(application, applicationRegistration, orgService);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_ChangeApplicationRegistrationOnUpdate()
        {
            #region Arrange
            var contactOriginal = PrepareContact();
            var originalContactId = contactOriginal.ToEntityReference();
            var applicationRegistrationOriginal = PrepareApplicationRegistration(originalContactId,
                cmc_applicationstatus.Pending);

            var contactNew = PrepareContact();
            var applicationRegistrationNew = PrepareApplicationRegistration(contactNew.ToEntityReference(),
                cmc_applicationstatus.Deffered);

            var applicationPreImage = PrepareApplication(applicationRegistrationOriginal.ToEntityReference());
            applicationPreImage.cmc_contactid = originalContactId;
            applicationPreImage.cmc_applicationstatus = applicationRegistrationOriginal.cmc_applicationstatus;

            var application = PrepareApplication(applicationRegistrationNew.ToEntityReference());
            application.cmc_contactid = originalContactId;
            application.Id = applicationPreImage.Id;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactOriginal,
                applicationRegistrationOriginal,
                contactNew,
                applicationRegistrationNew,
                applicationPreImage
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", applicationPreImage);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            // Application Status should not have been updated
            Assert.IsNull(application.cmc_applicationstatus);
            Assert.AreEqual(applicationRegistrationNew.cmc_contactid.Id, application.cmc_contactid.Id);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_ChangeApplicationRegistrationOnUpdateSameContact()
        {
            #region Arrange
            var contact = PrepareContact();
            var contactId = contact.ToEntityReference();
            var applicationRegistrationOriginal = PrepareApplicationRegistration(contactId,
                cmc_applicationstatus.Pending);

            var applicationRegistrationNew = PrepareApplicationRegistration(contactId,
                cmc_applicationstatus.Deffered);

            var applicationPreImage = PrepareApplication(applicationRegistrationOriginal.ToEntityReference());
            applicationPreImage.cmc_contactid = contactId;
            applicationPreImage.cmc_applicationstatus = applicationRegistrationOriginal.cmc_applicationstatus;

            var application = PrepareApplication(applicationRegistrationNew.ToEntityReference());
            application.Id = applicationPreImage.Id;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                applicationRegistrationOriginal,
                applicationRegistrationNew,
                applicationPreImage
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", applicationPreImage);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            // Application Status was not set on the Target, so it should not have been overwritten
            // as it was already set in the PreImage
            Assert.IsNull(application.cmc_contactid);
            Assert.IsNull(application.cmc_applicationstatus);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_DoNotChangeApplicationRegistrationOnUpdate()
        {
            #region Arrange
            var contactOriginal = PrepareContact();
            var originalContactId = contactOriginal.ToEntityReference();
            var contactNew = PrepareContact();
            var newContactId = contactNew.ToEntityReference();
            var applicationRegistration = PrepareApplicationRegistration(newContactId,
                cmc_applicationstatus.Incomplete);

            var application = PrepareApplication(applicationRegistration.ToEntityReference());
            application.cmc_contactid = originalContactId;
            application.cmc_applicationstatus = new OptionSetValue((int)cmc_applicationstatus.CanceledbyApplicant);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactOriginal,
                applicationRegistration,
                contactNew,
                application
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", application);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            Assert.AreEqual(originalContactId.Id, application.cmc_contactid.Id);
            Assert.AreEqual((int)cmc_applicationstatus.CanceledbyApplicant, application.cmc_applicationstatus.Value);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetDefaultFieldsOnApplication_ClearApplicationRegistrationOnUpdate()
        {
            #region Arrange
            var contact = PrepareContact();
            var contactId = contact.ToEntityReference();
            var applicationRegistration = PrepareApplicationRegistration(contactId,
                cmc_applicationstatus.Incomplete);

            var applicationPreImage = PrepareApplication(applicationRegistration.ToEntityReference());
            applicationPreImage.cmc_contactid = contactId;
            applicationPreImage.cmc_applicationstatus = new OptionSetValue((int)cmc_applicationstatus.Incomplete);

            var application = PrepareApplication(null);
            application.cmc_contactid = contactId;
            application.cmc_applicationstatus = new OptionSetValue((int)cmc_applicationstatus.Incomplete);
            application.Id = applicationPreImage.Id;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                applicationRegistration,
                applicationPreImage
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", applicationPreImage);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var orgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(orgService, mockLogger.Object);
            #endregion

            #region ACT
            applicationService.SetDefaultFields(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertApplicationFieldsSet(application, null, orgService, cmc_applicationstatus.Incomplete);
            #endregion
        }

        private Contact PrepareContact()
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = $"Contact{++_contactCounter}"
            };
        }

        private cmc_applicationregistration PrepareApplicationRegistration(EntityReference contactId, cmc_applicationstatus? applicationStatus = cmc_applicationstatus.InProgress)
        {
            return new cmc_applicationregistration()
            {
                Id = Guid.NewGuid(),
                cmc_contactid = contactId,
                cmc_applicationstatus = applicationStatus != null
                    ? new OptionSetValue((int)applicationStatus)
                    : null
            };
        }

        private cmc_application PrepareApplication(EntityReference applicationRegistrationId)
        {
            return new cmc_application()
            {
                Id = Guid.NewGuid(),
                cmc_applicationregistration = applicationRegistrationId
            };
        }

        private void AssertApplicationFieldsSet(cmc_application updatedApplication, cmc_applicationregistration applicationRegistration,
            IOrganizationService orgService, cmc_applicationstatus? originalApplicationStatus = null)
        {
            if (applicationRegistration == null)
            {
                Assert.IsNull(updatedApplication.cmc_contactid);
                Assert.AreEqual(((int?)originalApplicationStatus),
                    updatedApplication.cmc_applicationstatus?.Value);
                return;
            }

            if (originalApplicationStatus != null)
            {
                Assert.AreEqual((int)originalApplicationStatus.Value, updatedApplication.cmc_applicationstatus.Value);
            }
            else
            {
                Assert.AreEqual(applicationRegistration.cmc_applicationstatus?.Value, updatedApplication.cmc_applicationstatus?.Value);
            }

            Assert.AreEqual(applicationRegistration.cmc_contactid?.Id, updatedApplication.cmc_contactid?.Id);
        }
    }
}
