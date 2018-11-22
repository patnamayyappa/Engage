using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Application.Tests.Recommendation.Plugin
{
    [TestClass]
    public class RecomendationThankyouEmailTest : XrmUnitTestBase
    {
        private int _workflowCounter = 0;
        private int _recommendationDefinitionCounter = 0;
        private int _applicationDefinitionCounter = 0;
        private int _applicationVersionCounter = 0;
        private int _applicationRegistrationCounter = 0;
        private int _applicationCounter = 0;
        private int _recommendationCounter = 0;
        

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SendThankyouEmail_OnCreate()
        {
            #region Arrange
            var thankyouWorkflow = PrepareActiveThankYouWorkflow();
            var workflowId = thankyouWorkflow.ToEntityReference();
            var recommendationDefinition = PrepareRecommendationDefinition(workflowId);
            var recommendationDefinitionId = recommendationDefinition.ToEntityReference();
            var applicationDefinition = PrepareapplicationDefinition(recommendationDefinitionId);
            var applicationDefinitionId = applicationDefinition.ToEntityReference();
            var applcationVersion = PrepareApplicationVersion(applicationDefinitionId);
            var applicationVersionId = applcationVersion.ToEntityReference();
            var applicationRegistration = PrepareapplicationRegistration(applicationVersionId);
            var applicationRegistrationId = applicationRegistration.ToEntityReference();
            var application = PrepareApplication(applicationRegistrationId);
            var applicationId = application.ToEntityReference();
            var applicationRecommendation = PrepareapplicationRecommendation(applicationId);
            
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                thankyouWorkflow,
                recommendationDefinition,
                applicationDefinition,
                applcationVersion,
                applicationRegistration,
                application,
                applicationRecommendation
            });
            #endregion

            #region Act

            
            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, applicationRecommendation, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {

            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var recommendationService = new RecommendationService(mockLogger.Object, mockOrgService);
            AddPostEntityImage(mockServiceProvider, "Target", applicationRecommendation);
            recommendationService.SendThankyouEmail(mockExecutionContext.Object);
            
            #endregion

            #region ASSERT

            // To do this Assert part.

            #endregion ASSERT  
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SendThankyouEmail_OnCreate_NullapplicationId()
        {
            #region Arrange

            var thankyouWorkflow = PrepareActiveThankYouWorkflow();
            var workflowId = thankyouWorkflow.ToEntityReference();
            var recommendationDefinition = PrepareRecommendationDefinition(workflowId);
            var recommendationDefinitionId = recommendationDefinition.ToEntityReference();
            var applicationDefinition = PrepareapplicationDefinition(recommendationDefinitionId);
            var applicationDefinitionId = applicationDefinition.ToEntityReference();
            var applcationVersion = PrepareApplicationVersion(applicationDefinitionId);
            var applicationVersionId = applcationVersion.ToEntityReference();
            var applicationRegistration = PrepareapplicationRegistration(applicationVersionId);
            var applicationRegistrationId = applicationRegistration.ToEntityReference();
            var application = PrepareApplication(applicationRegistrationId);
            var applicationId = application.ToEntityReference();
            var applicationRecommendation = PrepareapplicationRecommendation(null);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                thankyouWorkflow,
                recommendationDefinition,
                applicationDefinition,
                applcationVersion,
                applicationRegistration,
                application,
                applicationRecommendation
            });
            #endregion

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, applicationRecommendation, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {

            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var recommendationService = new  RecommendationService(mockLogger.Object, mockOrgService);
            AddPostEntityImage(mockServiceProvider, "Target", applicationRecommendation);
            Exception error = null;
            try
            {
                recommendationService.SendThankyouEmail(mockExecutionContext.Object);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            #endregion

            #region Assert
            // No assert, no error should've occurred
            Assert.IsNull(error, $"An unexpected error has occurred. {error?.Message}");
            #endregion 
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SendThankyouEmail_OnCreate_NoTarget()
        {
            #region Arrange

            var thankyouWorkflow = PrepareActiveThankYouWorkflow();
            var workflowId = thankyouWorkflow.ToEntityReference();
            var recommendationDefinition = PrepareRecommendationDefinition(workflowId);
            var recommendationDefinitionId = recommendationDefinition.ToEntityReference();
            var applicationDefinition = PrepareapplicationDefinition(recommendationDefinitionId);
            var applicationDefinitionId = applicationDefinition.ToEntityReference();
            var applcationVersion = PrepareApplicationVersion(applicationDefinitionId);
            var applicationVersionId = applcationVersion.ToEntityReference();
            var applicationRegistration = PrepareapplicationRegistration(applicationVersionId);
            var applicationRegistrationId = applicationRegistration.ToEntityReference();
            var application = PrepareApplication(applicationRegistrationId);
            var applicationId = application.ToEntityReference();
            var applicationRecommendation = PrepareapplicationRecommendation(applicationId);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                thankyouWorkflow,
                recommendationDefinition,
                applicationDefinition,
                applcationVersion,
                applicationRegistration,
                application,
                applicationRecommendation
            });
            #endregion

            #region Act


            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, applicationRecommendation, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {

            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };
            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var recommendationService = new RecommendationService(mockLogger.Object, mockOrgService);
            AddPostEntityImage(mockServiceProvider, "NoTarget", applicationRecommendation);
            Exception error = null;
            try
            {
                recommendationService.SendThankyouEmail(mockExecutionContext.Object);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            #endregion

            #region Assert
            // No assert, no error should've occurred
            Assert.IsNull(error, $"An unexpected error has occurred. {error?.Message}");
            #endregion
        }
        
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SendThankyouEmail_OnCreate_InactiveWorkflow()
        {
            #region Arrange
            var thankyouWorkflow = PrepareInActiveThankYouWorkflow();
            var workflowId = thankyouWorkflow.ToEntityReference();
            var recommendationDefinition = PrepareRecommendationDefinition(workflowId);
            var recommendationDefinitionId = recommendationDefinition.ToEntityReference();
            var applicationDefinition = PrepareapplicationDefinition(recommendationDefinitionId);
            var applicationDefinitionId = applicationDefinition.ToEntityReference();
            var applcationVersion = PrepareApplicationVersion(applicationDefinitionId);
            var applicationVersionId = applcationVersion.ToEntityReference();
            var applicationRegistration = PrepareapplicationRegistration(applicationVersionId);
            var applicationRegistrationId = applicationRegistration.ToEntityReference();
            var application = PrepareApplication(applicationRegistrationId);
            var applicationId = application.ToEntityReference();
            var applicationRecommendation = PrepareapplicationRecommendation(applicationId);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                thankyouWorkflow,
                recommendationDefinition,
                applicationDefinition,
                applcationVersion,
                applicationRegistration,
                application,
                applicationRecommendation
            });
            #endregion

            #region Act


            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, applicationRecommendation, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {

            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };

            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var recommendationService = new RecommendationService(mockLogger.Object, mockOrgService);
            AddPostEntityImage(mockServiceProvider, "Target", applicationRecommendation);
            Exception error = null;
            try
            {
                recommendationService.SendThankyouEmail(mockExecutionContext.Object);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            #endregion

            #region Assert
            // No assert, no error should've occurred
            Assert.IsNull(error, $"An unexpected error has occurred. {error?.Message}");
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SendThankyouEmail_OnCreate_NoWorkflow()
        {
            #region Arrange
            var recommendationDefinition = PrepareRecommendationDefinition(null);
            var recommendationDefinitionId = recommendationDefinition.ToEntityReference();
            var applicationDefinition = PrepareapplicationDefinition(recommendationDefinitionId);
            var applicationDefinitionId = applicationDefinition.ToEntityReference();
            var applcationVersion = PrepareApplicationVersion(applicationDefinitionId);
            var applicationVersionId = applcationVersion.ToEntityReference();
            var applicationRegistration = PrepareapplicationRegistration(applicationVersionId);
            var applicationRegistrationId = applicationRegistration.ToEntityReference();
            var application = PrepareApplication(applicationRegistrationId);
            var applicationId = application.ToEntityReference();
            var applicationRecommendation = PrepareapplicationRecommendation(applicationId);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                recommendationDefinition,
                applicationDefinition,
                applcationVersion,
                applicationRegistration,
                application,
                applicationRecommendation
            });
            #endregion

            #region Act


            var mockLogger = new Mock<ILogger>();
            var mockServiceProvider = InitializeMockService(xrmFakedContext, applicationRecommendation, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {

            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };

            var mockOrgService = xrmFakedContext.GetFakedOrganizationService();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var recommendationService = new RecommendationService(mockLogger.Object, mockOrgService);
            AddPostEntityImage(mockServiceProvider, "Target", applicationRecommendation);
            Exception error = null;
            try
            {
                recommendationService.SendThankyouEmail(mockExecutionContext.Object);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            #endregion

            #region Assert
            // No assert, no error should've occurred
            Assert.IsNull(error, $"An unexpected error has occurred. {error?.Message}");
            #endregion
        }

        private Workflow PrepareActiveThankYouWorkflow()
        {
            return new Workflow()
            {
                Id = Guid.NewGuid(),
                Name = $"RecommendationThankYou {++_workflowCounter}",
                StatusCode = workflow_statuscode.Activated,
                StateCode = WorkflowState.Activated, 
                Type = workflow_type.Definition

            };
        }

        private Workflow PrepareInActiveThankYouWorkflow()
        {
            return new Workflow()
            {
                Id = Guid.NewGuid(),
                Name = $"RecommendationThankYou {++_workflowCounter}",
                StatusCode = workflow_statuscode.Draft,
                StateCode = WorkflowState.Draft,
                Type = workflow_type.Definition

            };
        }

        private cmc_applicationrecommendationdefinition PrepareRecommendationDefinition(EntityReference recommendationthankyouworkflowid)
        {
            return new cmc_applicationrecommendationdefinition()
            {
                Id = Guid.NewGuid(),
                cmc_applicationrecommendationdefinitionname = $"Recommendation Definition {++_recommendationDefinitionCounter}",
                cmc_recommendationthankyouworkflow = recommendationthankyouworkflowid

            };
        }

        private cmc_applicationdefinition PrepareapplicationDefinition(EntityReference recommendationDefinitionid)
        {
            return new cmc_applicationdefinition()
            {
                Id = Guid.NewGuid(),
                cmc_applicationdefinitionname  = $"Application Definition {++_applicationDefinitionCounter}",
                cmc_recommendationdefinitionid = recommendationDefinitionid

            };
        }

        private cmc_applicationdefinitionversion PrepareApplicationVersion(EntityReference applicationDefinitionid)
        {
            return new cmc_applicationdefinitionversion()
            {
                Id = Guid.NewGuid(),
                cmc_applicationdefinitionversionname = $"Application Version {++_applicationVersionCounter}",
                cmc_applicationdefinitionid  = applicationDefinitionid

            };
        }

        private cmc_applicationregistration PrepareapplicationRegistration(EntityReference applicationDefinitionversionid)
        {
            return new cmc_applicationregistration()
            {
                Id = Guid.NewGuid(),
                cmc_applicationregistration1 = $"Application Registration {++_applicationRegistrationCounter}",
                cmc_applicationdefinitionversionid = applicationDefinitionversionid

            };
        }

        private cmc_application PrepareApplication(EntityReference applicationRegistrationId)
        {
            return new cmc_application()
            {
                Id = Guid.NewGuid(),
                cmc_applicationname = $"Application {++_applicationCounter}",
                cmc_applicationregistration = applicationRegistrationId

            };
        }
        private cmc_applicationrecommendation PrepareapplicationRecommendation(EntityReference applicationId)
        {
            return new cmc_applicationrecommendation()
            {
                Id = Guid.NewGuid(),
                cmc_applicationrecommendationname = $"Recommendation {++_recommendationCounter}",
                cmc_applicationid = applicationId

            };
        }


    }
}

