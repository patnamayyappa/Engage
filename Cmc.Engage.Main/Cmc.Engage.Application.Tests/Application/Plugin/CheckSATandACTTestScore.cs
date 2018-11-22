using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using FakeItEasy;
using Microsoft.Xrm.Sdk.Query;
using cmc_testscoreconversion = Cmc.Engage.Models.Tests.cmc_testscoreconversion;
using cmc_testscoreconversionState = Cmc.Engage.Models.Tests.cmc_testscoreconversionState;
using cmc_testscoreconversion_statuscode = Cmc.Engage.Models.Tests.cmc_testscoreconversion_statuscode;
using Contact = Cmc.Engage.Models.Tests.Contact;
using mshied_testscore = Cmc.Engage.Models.Tests.mshied_testscore;

namespace Cmc.Engage.Application.Tests.Application.Plugin
{
    [TestClass]
    public class CheckSATandACTTestScore : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetSATandACTTestScore_SetACTScores()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType(true);
            var TestScore = PreparingTestScore(contact, TestType);
            var TestScoreConversion = PrepareTestcoreconversion(true, TestType);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestScore,
                TestType,
                TestScoreConversion

            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert

            xrmFakedContext.Data["contact"].TryGetValue(contact.Id, out contact);
            var satbest = contact.Attributes["cmc_actbest"];
            Assert.IsNotNull(satbest);
            #endregion Assert
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetSATandACTTestScore_SetSATScores()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType(false);
            var TestScore = PreparingTestScore(contact, TestType);
            var TestScoreConversion = PrepareTestcoreconversion(false, TestType);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestScore,
                TestType,
                TestScoreConversion
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert

            xrmFakedContext.Data["contact"].TryGetValue(contact.Id, out contact);
            var satbest = contact.Attributes["cmc_satbest"];
            Assert.IsNotNull(satbest);
            #endregion Assert
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_SetSATScores_NullTestType()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestScore = PreparingTestScore_nullreference(contact,null,250,true);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestScore,
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert

            var testScoreData = new Entity();
            xrmFakedContext.Data["mshied_testscore"].TryGetValue(TestScore.Id, out testScoreData);
            var testtypeId = testScoreData.GetAttributeValue<EntityReference>("mshied_testtypeid");
            Assert.IsNull(testtypeId);
            #endregion Assert
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_NullContact()
        {
            #region Arrange
            var TestType = PrepareTestType(false);
            var TestScore = PreparingTestScore_nullreference(null,TestType,250,true);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                TestType,
                TestScore,
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            var testScoreData = new Entity();
            xrmFakedContext.Data["mshied_testscore"].TryGetValue(TestScore.Id, out testScoreData);
            var contactId = testScoreData.GetAttributeValue<EntityReference>("mshied_studentid");
            Assert.IsNull(contactId);
            #endregion Assert
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_InactiveSATTestScore()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType(false);
            var TestScore = PreparingTestScore_nullreference(contact, TestType, 250, false);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestType,
                TestScore,
            });

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>._)).Returns(new EntityCollection(new List<Entity> {  }));
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            xrmFakedContext.Data["contact"].TryGetValue(contact.Id, out contact);
            var satBest = contact.Attributes["cmc_satbest"];
            Assert.IsNull(satBest);
            #endregion Assert
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_InactiveACTTestScore()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType(true);
            var TestScore = PreparingTestScore_nullreference(contact, TestType, 250, false);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestType,
                TestScore,
            });

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>._)).Returns(new EntityCollection(new List<Entity> { }));
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            xrmFakedContext.Data["contact"].TryGetValue(contact.Id, out contact);
            var actBest = contact.Attributes["cmc_actbest"];
            Assert.IsNull(actBest);
            #endregion Assert
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_NullTestScoreSchemaName()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType_NullSchemaName(false);
            var TestScore = PreparingTestScore_nullreference(contact, TestType,250, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestType,
                TestScore,
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            var testScoreData = new Entity();
            xrmFakedContext.Data["mshied_testscore"].TryGetValue(TestScore.Id, out testScoreData);
            var actequivalentScore = testScoreData.GetAttributeValue<int?>("mshied_actequivalentscore");
            Assert.IsNull(actequivalentScore);
            #endregion Assert
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_NullorZeroEquivalent()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType_NullSchemaName(true);
            var TestScore = PreparingTestScore_nullreference(contact, TestType, 0, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestType,
                TestScore,
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            var testScoreData = new Entity();
            xrmFakedContext.Data["mshied_testscore"].TryGetValue(TestScore.Id, out testScoreData);
            var actequivalentScore = testScoreData.GetAttributeValue<int?>("mshied_actequivalentscore");
            Assert.IsNull(actequivalentScore);
            #endregion Assert
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_InactiveTestScoreConversion()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType_NullSchemaName(true);
            var TestScore = PreparingTestScore_nullreference(contact, TestType, 250, true);
            var TestScoreConversion = PrepareTestcoreconversion_Inactive(TestType, false, false);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestType,
                TestScore,
                TestScoreConversion
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            var testScoreData = new Entity();
            xrmFakedContext.Data["mshied_testscore"].TryGetValue(TestScore.Id, out testScoreData);
            var actequivalentScore = testScoreData.GetAttributeValue<int?>("mshied_actequivalentscore");
            Assert.IsNull(actequivalentScore);
            #endregion Assert
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetSATandACTTestScore_NullEquivalentScore()
        {
            #region Arrange
            var contact = PreparingContact();
            var TestType = PrepareTestType_NullSchemaName(true);
            var TestScore = PreparingTestScore_nullreference(contact, TestType, 250, true);
            var TestScoreConversion = PrepareTestcoreconversion_Inactive(TestType, true, false);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                TestType,
                TestScore,
                TestScoreConversion
            });

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, TestScore, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var testService = new TestScoreService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Arrange

            #region ACT

            AddPostEntityImage(mockServiceProvider, "PostImage", TestScore);
            testService.SetTestSuperandBestScores(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            var testScoreData = new Entity();
            xrmFakedContext.Data["mshied_testscore"].TryGetValue(TestScore.Id, out testScoreData);
            var actequivalentScore = testScoreData.GetAttributeValue<int?>("mshied_actequivalentscore");
            Assert.IsNull(actequivalentScore);
            #endregion Assert
        }


        private Entity PreparingContact()
        {
            return new Contact()
            {
                ContactId = Guid.NewGuid(),
                FirstName = "Ankur k",
                cmc_satsuperscore = null,
                cmc_satbest = null,
                cmc_actbest = null,
                cmc_actsuperscore = null
            };
        }
        private Models.mshied_testscore PreparingTestScore(Entity contact, Entity testType)
        {
            return new Models.mshied_testscore()
            {
                Id = Guid.NewGuid(),
                mshied_ACTMath = 5,
                mshied_ACTScience = 10,
                mshied_ACTEnglish = 15,
                mshied_ACTReading = 15,
                mshied_ACTWriting = 15,
                mshied_ACTComposite = 15,
                mshied_SATEvidenceBasedReadingandWritingSection = 250,
                mshied_SATMathSection = 250,
                mshied_SATTotalScore = 900,
                mshied_StudentID = contact.ToEntityReference(),
                mshied_TestTypeId = testType.ToEntityReference(),
                cmc_includeinscorecalculations = true,
                statecode = mshied_testscoreState.Active,
                statuscode=mshied_testscore_statuscode.Active,
            };
        }
        private mshied_testtype PrepareTestType(bool isact)
        {
            return new mshied_testtype()
            {
                Id = Guid.NewGuid(),
                mshied_name = isact ? "ACT" : "SAT",
                cmc_equivalentscoreschemaname = isact ? "mshied_testscore.cmc_satequivalentscore" : "mshied_testscore.mshied_ACTEquivalentScore",
                cmc_testscoreschemaname = isact ? "mshied_testscore.mshied_ACTComposite" : "mshied_testscore.mshied_SATTotalScore"
            };

        }
        private Entity PrepareTestcoreconversion(bool isact, Entity testType)
        {
            return new cmc_testscoreconversion()
            {
                Id = Guid.NewGuid(),
                cmc_maximumscore = isact ? 15 : 1000,
                cmc_minimumscore = isact ? 15 : 800,
                cmc_testtypeId = testType.ToEntityReference(),
                cmc_equivalentscore = isact? 600 : 36,
                statecode = cmc_testscoreconversionState.Active,
                statuscode= cmc_testscoreconversion_statuscode.Active
            };

        }
        #region NegativeCase Data
        private Models.mshied_testscore PreparingTestScore_nullreference(Entity contact, Entity testType,int? totalScore, bool isActive)
        {
            return new Models.mshied_testscore()
            {
                Id = Guid.NewGuid(),
                mshied_SATEvidenceBasedReadingandWritingSection = 0,
                mshied_SATMathSection = 0,
                mshied_ACTComposite = totalScore,
                mshied_SATTotalScore = totalScore,
                mshied_StudentID = contact !=null ? contact.ToEntityReference(): null,
                mshied_TestTypeId = testType != null ? testType.ToEntityReference() : null,
                cmc_includeinscorecalculations = false,
                statecode = isActive ? mshied_testscoreState.Active : mshied_testscoreState.Inactive,
                statuscode = isActive ? mshied_testscore_statuscode.Active: mshied_testscore_statuscode.Inactive
            };
        }
        private Entity PrepareTestType_NullSchemaName(bool isSchemaNameRequired)
        {
            return new mshied_testtype()
            {
                Id = Guid.NewGuid(),
                mshied_name = "SAT",
                cmc_equivalentscoreschemaname = isSchemaNameRequired ? "mshied_testscore.mshied_actequivalentscore" : null,
                cmc_testscoreschemaname = isSchemaNameRequired ? "mshied_testscore.mshied_sattotalscore" : null
            };
        }
        private Entity PrepareTestcoreconversion_Inactive(Entity testType,bool isActive,bool isEquivalentScore)
        {
            return new cmc_testscoreconversion()
            {
                Id = Guid.NewGuid(),
                cmc_maximumscore =400,
                cmc_minimumscore = 200,
                cmc_testtypeId = testType.ToEntityReference(),
                cmc_equivalentscore = isEquivalentScore ? 36 : (decimal?)null,
                statecode = isActive ? cmc_testscoreconversionState.Active : cmc_testscoreconversionState.Inactive,
                statuscode = isActive ? cmc_testscoreconversion_statuscode.Active : cmc_testscoreconversion_statuscode.Inactive,
            };

        }
        #endregion
    }
}
