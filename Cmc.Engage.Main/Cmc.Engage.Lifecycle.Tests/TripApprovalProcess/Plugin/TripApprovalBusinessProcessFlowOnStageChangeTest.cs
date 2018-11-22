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

namespace Cmc.Engage.Lifecycle.Tests.TripApprovalProcess.Plugin
{
    [TestClass]
    public class TripApprovalBusinessProcessFlowOnStageChangeTest:XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripApprovalProcessService_OnMoveNextStage_RunWorkFlow()
        {
            #region Arrange
            var tripId = new cmc_trip()
            {
                Id = Guid.NewGuid()
            };

            var tripApprovalProcess = new cmc_tripapprovalprocess()
            {
            Id = Guid.NewGuid(),
            ActiveStageId=new EntityReference("cmc_tripapprovalprocess", new Guid("b09f2827-2f6e-42e0-88fb-36f29e3f3730")),
            TraversedPath= "d397ee00-1b33-4474-b689-daf68ea41db4,ab919e2f-ad3f-4bcf-bb36-c805c36608ce,b09f2827-2f6e-42e0-88fb-36f29e3f3730",
            bpf_cmc_tripid= tripId.ToEntityReference()
            };

            var config = GetActiveConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                tripId,
                tripApprovalProcess,
                config

            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, tripApprovalProcess , Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {
               
            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };

            #endregion
            #region Act 
            var mockLogger = new Mock<ILogger>();
            var mockConfig = new Mock<IConfigurationService>();
            mockConfig.Setup(r => r.GetActiveConfiguration()).Returns(config);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var mockTripApprovalProcessService = new TripApprovalProcessService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfig.Object);
            mockTripApprovalProcessService.TripApprovalProcessExecute(mockExecutionContext.Object);
            #endregion
            #region Assert
            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripApprovalProcessService_OnMoveNextStage_WorkFlowNull()
        {
            #region Arrange
            var tripId = new cmc_trip()
            {
                Id = Guid.NewGuid()
            };

            var tripApprovalProcess = new cmc_tripapprovalprocess()
            {
                Id = Guid.NewGuid(),
                ActiveStageId = new EntityReference("cmc_tripapprovalprocess", new Guid("b09f2827-2f6e-42e0-88fb-36f29e3f3730")),
                TraversedPath = "d397ee00-1b33-4474-b689-daf68ea41db4,ab919e2f-ad3f-4bcf-bb36-c805c36608ce,b09f2827-2f6e-42e0-88fb-36f29e3f3730",
                bpf_cmc_tripid = tripId.ToEntityReference()
            };

            var config = GetActiveConfiguration();
            config.cmc_tripapprovalassignworkflow = null;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                tripId,
                tripApprovalProcess,
                config

            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, tripApprovalProcess, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {

            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };

            #endregion
            #region Act 
            var mockLogger = new Mock<ILogger>();
            var mockConfig = new Mock<IConfigurationService>();
            mockConfig.Setup(r => r.GetActiveConfiguration()).Returns(config);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var mockTripApprovalProcessService = new TripApprovalProcessService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfig.Object);
            mockTripApprovalProcessService.TripApprovalProcessExecute(mockExecutionContext.Object);
            #endregion
            #region Assert
            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripApprovalProcessService_OnMoveNextStage_ActiveStageNull()
        {
            #region Arrange
            var tripId = new cmc_trip()
            {
                Id = Guid.NewGuid()
            };

            var tripApprovalProcess = new cmc_tripapprovalprocess()
            {
                Id = Guid.NewGuid(),
                ActiveStageId = null,
                TraversedPath = "d397ee00-1b33-4474-b689-daf68ea41db4,ab919e2f-ad3f-4bcf-bb36-c805c36608ce,b09f2827-2f6e-42e0-88fb-36f29e3f3730",
                bpf_cmc_tripid = tripId.ToEntityReference()
            };

            var config = GetActiveConfiguration();
            config.cmc_tripapprovalassignworkflow = null;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                tripId,
                tripApprovalProcess,
                config

            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, tripApprovalProcess, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {

            };
            ExecuteWorkflowResponse response = new ExecuteWorkflowResponse
            {

            };

            #endregion
            #region Act 
            var mockLogger = new Mock<ILogger>();
            var mockConfig = new Mock<IConfigurationService>();
            mockConfig.Setup(r => r.GetActiveConfiguration()).Returns(config);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<ExecuteWorkflowRequest>._)).Returns(response);
            var mockTripApprovalProcessService = new TripApprovalProcessService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfig.Object);
            mockTripApprovalProcessService.TripApprovalProcessExecute(mockExecutionContext.Object);
            #endregion
            #region Assert
            #endregion
        }
    }
}
