using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cmc.Engage.Application.Tests.Application.Plugin
{
    [TestClass]
    public class SetupApplicationRequirementTest : XrmUnitTestBase
    {
        private int _applicationDefinitionCounter = 0;
        private int _applicationRequirementCounter = 0;
        private int _applicationRequirementDefinitionDetailCounter = 0;
        private int _applicationRegistrationNameCounter = 0;

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithSingleApplicationRequirement()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.General);

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertMatchNoTestScore(mockOrgService, baseRecords.Application.ToEntityReference());
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationSkipTranscriptApplicationRequirement()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var applicationRequirementDefinitionId = baseRecords.ApplicationRequirementDefinition.ToEntityReference();
            var applicationRequirementDefinitionDetail1 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.OfficialTranscript);
            var applicationRequirementDefinitionDetail2 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.UnofficialTranscript);

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail1,
                applicationRequirementDefinitionDetail2
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(0, applicationRequirements.Entities.Count);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithNoApplicationRequirement()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(baseRecords.GetRecordsForInitialization());
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(0, applicationRequirements.Entities.Count);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithNoApplicationRegistration()
        {
            #region Arrange
            var contact = PrepareContact();
            var application = PrepareApplication(null,
                contact.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                application
            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                application.ToEntityReference());

            Assert.AreEqual(0, applicationRequirements.Entities.Count);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithSingleApplicationRequirement_TestScoreMatch()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType = PrepareTestType("ACT");
            var testTypeId = testType.ToEntityReference();
            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore, testTypeId,
                cmc_testsourcetype.OfficialTranscript);

            var testScore = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testTypeId, baseRecords.Contact.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                testScore
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(1, applicationRequirements.Entities.Count);

            var applicationRequirement = applicationRequirements.Entities.First().ToEntity<cmc_applicationrequirement>();
            AssertMatchingTestScore(applicationRequirement, testScore);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithSingleApplicationRequirement_TestScoreDuplicateMatches()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType = PrepareTestType("ACT");
            var testTypeId = testType.ToEntityReference();
            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore, testTypeId,
                cmc_testsourcetype.OfficialTranscript);

            var testScore1 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testTypeId, baseRecords.Contact.ToEntityReference());
            testScore1.mshied_TestDate = DateTime.Now;

            var testScore2 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testTypeId, baseRecords.Contact.ToEntityReference());
            testScore2.mshied_TestDate = DateTime.Now.AddDays(-1);

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                testScore1,
                testScore2
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(1, applicationRequirements.Entities.Count);

            var applicationRequirement = applicationRequirements.Entities.First().ToEntity<cmc_applicationrequirement>();
            AssertMatchingTestScore(applicationRequirement, testScore1);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithSingleApplicationRequirement_TestScoreMatchMissingFields()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore);

            var testScore = PrepareTestScore(null,
                null, baseRecords.Contact.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                testScore
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(1, applicationRequirements.Entities.Count);

            var applicationRequirement = applicationRequirements.Entities.First().ToEntity<cmc_applicationrequirement>();
            Assert.IsNull(applicationRequirement.cmc_testscoreid);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithSingleApplicationRequirement_TestScoreNoMatch()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType1 = PrepareTestType("ACT");
            var testType1Id = testType1.ToEntityReference();
            var testType2 = PrepareTestType("SAT");
            var testType2Id = testType2.ToEntityReference();
            var additionalContact = PrepareContact();

            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore, testType1Id,
                cmc_testsourcetype.OfficialTranscript);

            var testScore1 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType2Id, baseRecords.Contact.ToEntityReference());
            var testScore2 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType1Id, additionalContact.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                testScore1,
                testScore2,
                additionalContact
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertMatchNoTestScore(mockOrgService,
                baseRecords.Application.ToEntityReference());
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_CreateApplicationWithMultipleApplicationRequirements()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType1 = PrepareTestType("ACT");
            var testType1Id = testType1.ToEntityReference();
            var testType2 = PrepareTestType("SAT");
            var testType2Id = testType2.ToEntityReference();
            var applicationRequirementDefinitionId = baseRecords.ApplicationRequirementDefinition.ToEntityReference();
            var contactId = baseRecords.Contact.ToEntityReference();

            var applicationRequirementDefinitionDetail1 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.TestScore,
                testType1Id, cmc_testsourcetype.OfficialTranscript);

            var applicationRequirementDefinitionDetail2 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.TestScore,
                testType2Id, cmc_testsourcetype.SelfReported);

            var applicationRequirementDefinitionDetail3 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.General);

            var applicationRequirementDefinitionDetail4 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.UnofficialTranscript);

            var applicationRequirementDefinitionDetail5 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.TestScore,
                testType2Id, cmc_testsourcetype.OfficialTestingAgency);

            var testScore1 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType1Id, contactId);
            var testScore2 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTestingAgency,
                testType2Id, contactId);
            var testScore3 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTestingAgency,
                testType1Id, contactId);

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail1,
                applicationRequirementDefinitionDetail2,
                applicationRequirementDefinitionDetail3,
                applicationRequirementDefinitionDetail4,
                applicationRequirementDefinitionDetail5,
                testScore1,
                testScore2,
                testScore3,
                testType1,
                testType2
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(4, applicationRequirements.Entities.Count);

            // Just verify two Test Score were found. 
            // The Unit Test will not populate any additional fields on Application Requirement,
            // so we can't verify it populated the correct one here.
            Assert.AreEqual(2, applicationRequirements.Entities.Count(
                record => record.ToEntity<cmc_applicationrequirement>().cmc_testscoreid != null));
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_UpdateApplication_FindTestScoreMatch()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType = PrepareTestType("ACT");

            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore, testType.ToEntityReference(),
                cmc_testsourcetype.OfficialTranscript);

            var applicationRequirement = PrepareApplicationRequirement(
                baseRecords.Application.ToEntityReference(), cmc_applicationrequirementtype.TestScore,
                applicationRequirementDefinitionDetail.ToEntityReference());

            var testScore = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType.ToEntityReference(), baseRecords.Contact.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                applicationRequirement,
                testScore
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(1, applicationRequirements.Entities.Count);

            var updatedApplicationRequirement = applicationRequirements.Entities.First().ToEntity<cmc_applicationrequirement>();
            AssertMatchingTestScore(updatedApplicationRequirement, testScore);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_UpdateApplication_FindDuplicateTestScoreMatch()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType = PrepareTestType("ACT");

            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore, testType.ToEntityReference(),
                cmc_testsourcetype.OfficialTranscript);

            var applicationRequirement = PrepareApplicationRequirement(
                baseRecords.Application.ToEntityReference(), cmc_applicationrequirementtype.TestScore,
                applicationRequirementDefinitionDetail.ToEntityReference());

            var testScore1 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType.ToEntityReference(), baseRecords.Contact.ToEntityReference());
            testScore1.mshied_TestDate = DateTime.Now;

            var testScore2 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType.ToEntityReference(), baseRecords.Contact.ToEntityReference());
            testScore2.mshied_TestDate = DateTime.Now.AddDays(1);

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                applicationRequirement,
                testScore1,
                testScore2
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(1, applicationRequirements.Entities.Count);

            var updatedApplicationRequirement = applicationRequirements.Entities.First().ToEntity<cmc_applicationrequirement>();
            AssertMatchingTestScore(updatedApplicationRequirement, testScore2);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetupApplicationRequirement_UpdateApplication_DoNotFindTestScoreMatch()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType1 = PrepareTestType("ACT");
            var testType2 = PrepareTestType("SAT");
            var additionalContact = PrepareContact();
            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore, testType1.ToEntityReference(),
                cmc_testsourcetype.OfficialTranscript);

            var testScore1 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType2.ToEntityReference(), baseRecords.Contact.ToEntityReference());
            var testScore2 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType1.ToEntityReference(), additionalContact.ToEntityReference());

            var applicationRequirement = PrepareApplicationRequirement(
                baseRecords.Application.ToEntityReference(), cmc_applicationrequirementtype.TestScore,
                applicationRequirementDefinitionDetail.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                additionalContact,
                applicationRequirementDefinitionDetail,
                applicationRequirement,
                testScore1,
                testScore2,
                testType1,
                testType2
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertMatchNoTestScore(mockOrgService, baseRecords.Application.ToEntityReference());
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetupApplicationRequirement_UpdateApplication_DoNoOverrideTestScore()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType1 = PrepareTestType("ACT");
            var testType2 = PrepareTestType("SAT");
            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.TestScore, testType1.ToEntityReference(),
                cmc_testsourcetype.OfficialTranscript);

            var contactId = baseRecords.Contact.ToEntityReference();
            var testScore1 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType2.ToEntityReference(), contactId);
            var testScore2 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType1.ToEntityReference(), contactId);

            var applicationRequirement = PrepareApplicationRequirement(
                baseRecords.Application.ToEntityReference(), cmc_applicationrequirementtype.TestScore,
                applicationRequirementDefinitionDetail.ToEntityReference(),
                testScore1.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                applicationRequirement,
                testScore1,
                testScore2,
                testType1,
                testType2
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(1, applicationRequirements.Entities.Count);

            var updatedApplicationRequirement = applicationRequirements.Entities.First().ToEntity<cmc_applicationrequirement>();
            Assert.AreEqual(testScore1.ToEntityReference(), updatedApplicationRequirement.cmc_testscoreid);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetupApplicationRequirement_UpdateApplication_NoTestScoreRequirements()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType = PrepareTestType("ACT");
            var applicationRequirementDefinitionDetail = PrepareApplicationRequirementDefinitionDetail(
                baseRecords.ApplicationRequirementDefinition.ToEntityReference(),
                cmc_applicationrequirementtype.General);

            var applicationRequirement = PrepareApplicationRequirement(
                baseRecords.Application.ToEntityReference(), cmc_applicationrequirementtype.General,
                applicationRequirementDefinitionDetail.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirementDefinitionDetail,
                applicationRequirement
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            AssertMatchNoTestScore(mockOrgService,
                baseRecords.Application.ToEntityReference());
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetupApplicationRequirement_UpdateApplicationWithMultipleApplicationRequirements()
        {
            #region Arrange
            var baseRecords = SetupBaseRecords();
            var testType1 = PrepareTestType("ACT");
            var testType1Id = testType1.ToEntityReference();
            var testType2 = PrepareTestType("SAT");
            var testType2Id = testType2.ToEntityReference();
            var applicationRequirementDefinitionId = baseRecords.ApplicationRequirementDefinition.ToEntityReference();
            var contactId = baseRecords.Contact.ToEntityReference();

            var applicationRequirementDefinitionDetail1 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.TestScore,
                testType1Id, cmc_testsourcetype.OfficialTranscript);

            var applicationRequirementDefinitionDetail2 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.TestScore,
                testType2Id, cmc_testsourcetype.SelfReported);

            var applicationRequirementDefinitionDetail3 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.General);

            var applicationRequirementDefinitionDetail4 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.UnofficialTranscript);

            var applicationRequirementDefinitionDetail5 = PrepareApplicationRequirementDefinitionDetail(
                applicationRequirementDefinitionId, cmc_applicationrequirementtype.TestScore,
                testType2Id, cmc_testsourcetype.OfficialSIS);

            var testScore1 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType1Id, contactId);
            var testScore2 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialSIS,
                testType2Id, contactId);
            var testScore3 = PrepareTestScore(mshied_testscore_mshied_testsource.OfficialTranscript,
                testType2Id, contactId);

            var applicationId = baseRecords.Application.ToEntityReference();
            var applicationRequirement1 = PrepareApplicationRequirement(applicationId,
                cmc_applicationrequirementtype.TestScore,
                applicationRequirementDefinitionDetail1.ToEntityReference());
            var applicationRequirement2 = PrepareApplicationRequirement(applicationId,
                cmc_applicationrequirementtype.TestScore,
                applicationRequirementDefinitionDetail2.ToEntityReference());
            var applicationRequirement3 = PrepareApplicationRequirement(applicationId,
                cmc_applicationrequirementtype.General,
                applicationRequirementDefinitionDetail3.ToEntityReference());
            var applicationRequirement4 = PrepareApplicationRequirement(applicationId,
                cmc_applicationrequirementtype.TestScore,
                applicationRequirementDefinitionDetail5.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            var initializeRecords = new List<Entity>()
            {
                applicationRequirement1,
                applicationRequirement2,
                applicationRequirement3,
                applicationRequirement4,
                applicationRequirementDefinitionDetail1,
                applicationRequirementDefinitionDetail2,
                applicationRequirementDefinitionDetail3,
                applicationRequirementDefinitionDetail4,
                applicationRequirementDefinitionDetail5,
                testScore1,
                testScore2,
                testScore3,
                testType1,
                testType2
            };
            initializeRecords.AddRange(baseRecords.GetRecordsForInitialization());
            xrmFakedContext.Initialize(initializeRecords);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, baseRecords.Application, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            var applicationService = new ApplicationService(mockOrgService, mockLogger.Object);

            applicationService.SetupApplicationRequirements(mockExecutionContext.Object);
            #endregion

            #region Assert
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService,
                baseRecords.Application.ToEntityReference());

            Assert.AreEqual(4, applicationRequirements.Entities.Count);

            // Just verify two Test Scores were found. 
            // The Unit Test will not populate any additional fields on Application Requirement,
            // so we can't verify it populated the correct one here.
            Assert.AreEqual(2, applicationRequirements.Entities.Count(
                record => record.ToEntity<cmc_applicationrequirement>().cmc_testscoreid != null));
            #endregion
        }


        private cmc_applicationdefinition PrepareApplicationDefinition(EntityReference requirementDefinitionId)
        {
            return new cmc_applicationdefinition()
            {
                Id = Guid.NewGuid(),
                cmc_requirementdefinitionid = requirementDefinitionId,
                cmc_applicationdefinitionname = $"Application Definition {++_applicationDefinitionCounter}"
            };
        }

        private cmc_applicationrequirementdefinition PrepareApplicationRequirementDefinition()
        {
            return new cmc_applicationrequirementdefinition()
            {
                Id = Guid.NewGuid(),
                cmc_applicationrequirementdefinitionname = $"Application Requirement Definition {++_applicationRequirementCounter}"
            };
        }

        private mshied_testtype PrepareTestType(string testTypeName)
        {
            return new mshied_testtype()
            {
                Id = Guid.NewGuid(),
                mshied_name = testTypeName
            };
        }

        private mshied_testscore PrepareTestScore(mshied_testscore_mshied_testsource? testSource, EntityReference testTypeId, EntityReference contactId)
        {
            return new mshied_testscore()
            {
                Id = Guid.NewGuid(),
                mshied_TestSource = testSource,
                mshied_TestTypeId = testTypeId,
                mshied_StudentID = contactId
            };
        }

        private cmc_applicationrequirementdefinitiondetail PrepareApplicationRequirementDefinitionDetail(EntityReference applicationRequirementDefinitionId, cmc_applicationrequirementtype requirementType, EntityReference testTypeId = null, cmc_testsourcetype? testSourceType = null)
        {
            return new cmc_applicationrequirementdefinitiondetail()
            {
                Id = Guid.NewGuid(),
                cmc_applicationrequirementdefinition = applicationRequirementDefinitionId,
                cmc_requirementtype = new OptionSetValue((int)requirementType),
                cmc_name = $"Application Requirement Definition Detail {++_applicationRequirementDefinitionDetailCounter}",
                cmc_testscoretype = testTypeId,
                cmc_testsourcetype = testSourceType == null
                    ? null
                    : new OptionSetValue((int)testSourceType)
            };
        }

        private cmc_applicationdefinitionversion PrepareApplicationDefinitionVersion(EntityReference applicationDefinitionId)
        {
            return new cmc_applicationdefinitionversion()
            {
                Id = Guid.NewGuid(),
                cmc_applicationdefinitionid = applicationDefinitionId
            };
        }

        private Contact PrepareContact()
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User"
            };
        }

        private cmc_applicationregistration PrepareApplicationRegistration(
            EntityReference applicationDefinitionVersionId, EntityReference contactId)
        {
            return new cmc_applicationregistration()
            {
                Id = Guid.NewGuid(),
                cmc_applicationdefinitionversionid = applicationDefinitionVersionId,
                cmc_applicationregistration1 = $"Application Registration {++_applicationRegistrationNameCounter}",
                cmc_contactid = contactId
            };
        }

        private cmc_application PrepareApplication(EntityReference applicationRegistrationId,
            EntityReference contactId)
        {
            return new cmc_application()
            {
                Id = Guid.NewGuid(),
                cmc_applicationregistration = applicationRegistrationId,
                cmc_contactid = contactId
            };
        }

        private cmc_applicationrequirement PrepareApplicationRequirement(EntityReference applicationId,
            cmc_applicationrequirementtype requirementType,
            EntityReference requirementDefinitionDetailId = null, EntityReference testScoreId = null)
        {
            return new cmc_applicationrequirement()
            {
                Id = Guid.NewGuid(),
                cmc_applicationid = applicationId,
                cmc_requirementtype = new OptionSetValue((int)requirementType),
                cmc_requirementdefinitiondetailid = requirementDefinitionDetailId,
                cmc_testscoreid = testScoreId
            };
        }

        private BaseRecords SetupBaseRecords()
        {
            var applicationRequirementDefinition = PrepareApplicationRequirementDefinition();
            var applicationDefinition = PrepareApplicationDefinition(
                applicationRequirementDefinition.ToEntityReference());
            var applicationDefinitionVersion = PrepareApplicationDefinitionVersion(
                applicationDefinition.ToEntityReference());
            var contact = PrepareContact();
            var applicationRegistration = PrepareApplicationRegistration(
                applicationDefinitionVersion.ToEntityReference(), contact.ToEntityReference());

            var application = PrepareApplication(applicationRegistration.ToEntityReference(),
                contact.ToEntityReference());

            return new BaseRecords()
            {
                Application = application,
                ApplicationDefinition = applicationDefinition,
                ApplicationDefinitionVersion = applicationDefinitionVersion,
                ApplicationRegistration = applicationRegistration,
                ApplicationRequirementDefinition = applicationRequirementDefinition,
                Contact = contact
            };
        }

        private void AssertMatchNoTestScore(IOrganizationService mockOrgService, EntityReference applicationId)
        {
            var applicationRequirements = RetrieveApplicationRequirements(mockOrgService, applicationId);

            Assert.AreEqual(1, applicationRequirements.Entities.Count);

            var applicationRequirement = applicationRequirements.Entities.First().ToEntity<cmc_applicationrequirement>();
            Assert.IsNull(applicationRequirement.cmc_testscoreid);
        }

        private void AssertMatchingTestScore(cmc_applicationrequirement applicationRequirement, mshied_testscore testScore)
        {
            Assert.IsNotNull(applicationRequirement.cmc_testscoreid);
            Assert.AreEqual(testScore.Id, applicationRequirement.cmc_testscoreid.Id);
            Assert.IsNotNull(applicationRequirement.cmc_receiveddatetime);
            Assert.AreEqual((int)cmc_applicationrequirementstatus.Received,
                applicationRequirement.cmc_requirementstatus?.Value);

        }

        private EntityCollection RetrieveApplicationRequirements(IOrganizationService mockOrgService, EntityReference applicationId)
        {
            return mockOrgService.RetrieveMultiple(new FetchExpression(
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical'>
                      <entity name='cmc_applicationrequirement'>
                        <attribute name='cmc_applicationrequirementid' />
                        <attribute name='cmc_receiveddatetime' />
                        <attribute name='cmc_requirementstatus' />
                        <attribute name='cmc_testscoreid' />
                        <filter type='and'>
                          <condition attribute='cmc_applicationid' operator='eq' value='{applicationId.Id}' />
                        </filter>
                      </entity>
                    </fetch>"));
        }
        private class BaseRecords
        {
            public cmc_applicationrequirementdefinition ApplicationRequirementDefinition { get; set; }
            public cmc_applicationdefinition ApplicationDefinition { get; set; }
            public cmc_applicationdefinitionversion ApplicationDefinitionVersion { get; set; }
            public Contact Contact { get; set; }
            public cmc_applicationregistration ApplicationRegistration { get; set; }
            public cmc_application Application { get; set; }

            public IEnumerable<Entity> GetRecordsForInitialization()
            {
                return new List<Entity>
                {
                    Application,
                    ApplicationRequirementDefinition,
                    ApplicationDefinition,
                    ApplicationDefinitionVersion,
                    Contact,
                    ApplicationRegistration
                };
            }
        }
    }
}
