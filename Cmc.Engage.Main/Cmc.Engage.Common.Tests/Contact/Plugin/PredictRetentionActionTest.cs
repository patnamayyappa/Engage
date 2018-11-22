using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.Contact.Plugin
{
    [TestClass]
    public class PredictRetentionActionTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void PredictRetentionAction_CmcPredictRetentionActionForEntity()
        {
            #region ARRANGE
            var contactInstance = PrepareContactInstance();
            var previousEducation = PreviousEducation(contactInstance);   
            var apiKey = PrepareRetentionPredictionApi();
            var apiUrl = PrepareRetentionPredictionApi();//.GetAttributeValue<cmc_configuration>("cmc_retentionpredictionapiurl");
            var academicProgress = AcademicProgress(contactInstance);

            var xrmFakedContext = new XrmFakedContext();
            
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactInstance,
                previousEducation,
                academicProgress,
                
                apiKey,
                apiUrl,
            });
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_configuration));
            #endregion

            #region ACT
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contactInstance, Operation.cmc_PredictRetentionAction);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            AddInputParameters(mockServiceProvider, "ContactId", contactInstance.Id.ToString());

            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.PredictRetentionAction(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var resultData = mockPluginExecutionContext.OutputParameters["RetentionProbability"];
            if (resultData == null || ((decimal)resultData) < 0) Assert.Fail("contact retention prediction score calculation failed.");
            #endregion
        }

        private Entity PrepareContactInstance()
        {
            var contactId = Guid.NewGuid();

            var contact = new Entity("contact", contactId)
            {
                ["cmc_recentsat"] = 70,
                ["cmc_recentact"] = 80,
                ["GenderCode"] = new OptionSetValue(2),
                ["cmc_age"] = 25,
                ["cmc_firstgeneration"] = true,
                ["cmc_retentionprobability"] = 0
                //cmc_academicperiod_contact_currentacademicperiodid = new cmc_academicperiod()
            };


            return contact;
        }

        private Entity AcademicProgress(Entity contactid)
        {
            return new Entity("mshied_academicperioddetails", Guid.NewGuid())
            {
                ["cmc_residence"]=new OptionSetValue(1),
                ["mshied_attendancetype"] = new OptionSetValue(1),
                ["mshied_studentid"] = contactid.ToEntityReference(),
                ["cmc_midtermdeficiency"] = true,
            };
            
        }

        private Entity PreviousEducation(Entity contactid)
        {
            var previousEducation = new Entity("mshied_previouseducationId", Guid.NewGuid())
            {
                ["mshied_studentid"] = contactid.ToEntityReference(),
            };
            return previousEducation;
        }

        private Entity PrepareRetentionPredictionApi()
        {
            var retentionPredictionApi = new Entity("cmc_configuration", Guid.NewGuid())//Models.cmc_configuration()
            {
                ["cmc_retentionpredictionapikey"] = "bstXj9QbPUeX4bw0Pwyozo10R0j+OGEQr1zAJCWHSsTmUf5S7SgfxIQmrqHH1Lze9M4UKLkSkAsCB5ykiztAkQ==",
                ["cmc_retentionpredictionapiurl"] = "https://ussouthcentral.services.azureml.net/workspaces/15cc478a2f4344fca0eb3b2a534f5de2/services/c54a84046bd048c4a08fa9495b9cdea1/execute?api-version=2.0&details=true"
                
            };
            return retentionPredictionApi;
        }
        
    }
}