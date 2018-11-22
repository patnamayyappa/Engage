using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Application.Tests.RequirementDefinitionDetail.Plugin
{
    [TestClass]
    public class ApplicationRequirementsDefinitionServiceTest: XrmUnitTestBase
    {
        Mock<ILanguageService> _languageService;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IExecutionContext> _mockExecutionContext;
        private IOrganizationService _mockOrgService;
        private XrmFakedContext _xrmFakedContext;

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        [DataRow(cmc_applicationrequirementtype.UnofficialTranscript, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.UnofficialTranscript, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.OfficialTranscript, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.OfficialTranscript, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, Operation.Update)]
        public void OnCreateUpdateOnlyOneRequirementCanBeOfOfIncomingValue(cmc_applicationrequirementtype incomingValue, Operation operation)
        {
            #region Arrange

            var parentGuid = Guid.NewGuid();
            var testEntity = GetDetailEntity(parentGuid, incomingValue, Guid.NewGuid());
            var objectUnderTest = ArrangeAndGetObjectUnderTest(new List<Entity>()
                {
                    new cmc_applicationrequirementdefinition(){Id = parentGuid},
                    testEntity,
                    GetDetailEntity(parentGuid, incomingValue, null)
                },
                testEntity, operation);

            #endregion

            #region ActAndAssert

            Assert.ThrowsException<InvalidPluginExecutionException>(
                () => objectUnderTest.CreateUpdateApplicationRequirementsDefinitionDetail(_mockExecutionContext.Object));
            _languageService.Verify(_=>_.Get(It.IsAny<string>(),null),Times.Once);

            #endregion

        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        [DataRow(cmc_applicationrequirementtype.UnofficialTranscript, cmc_applicationrequirementtype.General, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.UnofficialTranscript, cmc_applicationrequirementtype.General, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.OfficialTranscript, cmc_applicationrequirementtype.General, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.OfficialTranscript, cmc_applicationrequirementtype.General, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, cmc_applicationrequirementtype.General, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, cmc_applicationrequirementtype.General, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, cmc_applicationrequirementtype.OfficialTranscript, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, cmc_applicationrequirementtype.OfficialTranscript, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, cmc_applicationrequirementtype.UnofficialTranscript, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.Recommendation, cmc_applicationrequirementtype.UnofficialTranscript, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.Upload, cmc_applicationrequirementtype.UnofficialTranscript, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.Upload, cmc_applicationrequirementtype.UnofficialTranscript, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.TestScore, cmc_applicationrequirementtype.General, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.TestScore, cmc_applicationrequirementtype.General, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.Upload, cmc_applicationrequirementtype.General, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.Upload, cmc_applicationrequirementtype.General, Operation.Update)]
        [DataRow(cmc_applicationrequirementtype.General, cmc_applicationrequirementtype.General, Operation.Create)]
        [DataRow(cmc_applicationrequirementtype.General, cmc_applicationrequirementtype.General, Operation.Update)]
        public void OnCreateUpdateDefaultBehaviourPassThrough(cmc_applicationrequirementtype incomingValue, cmc_applicationrequirementtype existingValue, Operation operation)
        {
            #region Arrange

            var parentGuid = Guid.NewGuid();
            var testEntity = GetDetailEntity(parentGuid, incomingValue, Guid.NewGuid());
            var objectUnderTest = ArrangeAndGetObjectUnderTest(new List<Entity>()
                {
                    new cmc_applicationrequirementdefinition(){Id = parentGuid},
                    testEntity,
                    GetDetailEntity(Guid.NewGuid(), incomingValue, null),
                    GetDetailEntity(parentGuid, existingValue, null),
                    GetDetailEntity(parentGuid, incomingValue, null, cmc_applicationrequirementdefinitiondetailState.Inactive)
                },
                testEntity, operation);
            
            #endregion

            #region Act

            objectUnderTest.CreateUpdateApplicationRequirementsDefinitionDetail(_mockExecutionContext.Object);

            #endregion
        }

        
        private ApplicationRequirementsDefinitionService ArrangeAndGetObjectUnderTest(IEnumerable<Entity> testDataSet, Entity testEntity, Operation operation)
        {
            var mockLogger = new Mock<ILogger>();
            _languageService = new Mock<ILanguageService>();
            _xrmFakedContext = new XrmFakedContext();
            _xrmFakedContext.Initialize(testDataSet);
            _mockServiceProvider = InitializeMockService(_xrmFakedContext, testEntity, operation);
            _mockExecutionContext = GetMockExecutionContext(_mockServiceProvider);
            _mockOrgService = _xrmFakedContext.GetFakedOrganizationService();
            _languageService.Setup(_ => _.Get(It.IsAny<string>(), null)).Returns("{0}");
            var objectUnderTest = new ApplicationRequirementsDefinitionService(mockLogger.Object, _mockOrgService, _languageService.Object);
            return objectUnderTest;
        }

        private cmc_applicationrequirementdefinitiondetail GetDetailEntity(Guid parentId, cmc_applicationrequirementtype? requirementType, Guid? id, cmc_applicationrequirementdefinitiondetailState stateCode = cmc_applicationrequirementdefinitiondetailState.Active)
        {
            return new cmc_applicationrequirementdefinitiondetail()
            {
                Id = id ?? Guid.NewGuid(),
                cmc_requirementtype = new OptionSetValue()
                {
                    Value = (int) (requirementType ?? cmc_applicationrequirementtype.General)
                },
                cmc_applicationrequirementdefinition = new EntityReference("cmc_applicationrequirementdefinition", parentId),
                statecode = stateCode
            };
        }
    }
}
