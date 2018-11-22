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

namespace Cmc.Engage.Retention.Tests.SuccessPlan.Plugin
{
    [TestClass]
    public class SetSuccessPlanStatusTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatusIncomplete_cmcsuccessplanUpdate()
        {
            #region ARRANGE

            var creatingSuccesPlan = PrepareSuccessPlan();
            var creatingToDo = PrepareToDoEntity(creatingSuccesPlan.Id);
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id, out result);
            var stateCode = result.GetAttributeValue<OptionSetValue>("statecode");
            Assert.AreEqual(0, stateCode.Value);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatusCanceled_cmcsuccessplanUpdate()
        {
            #region ARRANGE

            cmc_successplan creatingSuccesPlan = PrepareSuccessPlan();
            cmc_todo creatingToDo = PrepareToDoEntity(creatingSuccesPlan.Id);
            creatingSuccesPlan.statecode = cmc_successplanState.Active;
            creatingToDo.statuscode = cmc_todo_statuscode.Canceled;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id,out result);
            var stateCode = result.GetAttributeValue<OptionSetValue>("statecode");
            Assert.AreEqual(1, stateCode.Value);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatusComplete_cmcsuccessplanUpdate()
        {
            #region ARRANGE

            cmc_successplan creatingSuccesPlan = PrepareSuccessPlan();
            cmc_todo creatingToDo = PrepareToDoEntity(creatingSuccesPlan.Id);
            creatingSuccesPlan.statecode = cmc_successplanState.Active;
            creatingToDo.statuscode = cmc_todo_statuscode.Complete;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id, out result);
            var stateCode = result.GetAttributeValue<OptionSetValue>("statecode");
            Assert.AreEqual(1, stateCode.Value);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatus_cmcsuccessplanUpdate_TodoHasNoSuccessPlan()
        {
            #region ARRANGE

            cmc_successplan creatingSuccesPlan = PrepareSuccessPlan();
            cmc_todo creatingToDo = PrepareToDoEntity1(creatingSuccesPlan.Id);
            creatingSuccesPlan.statecode = cmc_successplanState.Active;
            creatingToDo.statuscode = cmc_todo_statuscode.Complete;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id, out result);

            Assert.IsNotNull(result);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatus_cmcsuccessplanUpdate_SuccessPlanStatusCodeIsNull()
        {
            #region ARRANGE

            cmc_successplan creatingSuccesPlan = PrepareSuccessPlan();
            cmc_todo creatingToDo = PrepareToDoEntity(creatingSuccesPlan.Id);            
            creatingSuccesPlan.statuscode = null;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id, out result);

            Assert.IsFalse(result.Contains("statuscode"));
            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatus_cmcsuccessplanUpdate_SuccessplanStatuscodeIsActive()
        {
            #region ARRANGE

            cmc_successplan creatingSuccesPlan = PrepareSuccessPlan();
            cmc_todo creatingToDo = PrepareToDoEntity(creatingSuccesPlan.Id);
            creatingToDo.statuscode = cmc_todo_statuscode.Incomplete;
            creatingSuccesPlan.statuscode = cmc_successplan_statuscode.Active;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id, out result);

            Assert.IsTrue(result.Contains("statecode"));
            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatus_cmcsuccessplanUpdate_SuccessplanStatuscodeIsCanceled()
        {
            #region ARRANGE

            cmc_successplan creatingSuccesPlan = PrepareSuccessPlan();
            cmc_todo creatingToDo = PrepareToDoEntity(creatingSuccesPlan.Id);
            creatingToDo.statuscode = cmc_todo_statuscode.Canceled;
            creatingSuccesPlan.statuscode = cmc_successplan_statuscode.Canceled;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var  mockILanguageService= new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id, out result);

            Assert.IsNotNull(result);
            #endregion
        }
        
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetSuccessPlanStatusService_UpdateSuccessPlanStatus_cmcsuccessplanUpdate_SuccessplanStatuscodeIsCompleted()
        {
            #region ARRANGE

            cmc_successplan creatingSuccesPlan = PrepareSuccessPlan();
            cmc_todo creatingToDo = PrepareToDoEntity(creatingSuccesPlan.Id);
            creatingToDo.statuscode = null;
            creatingSuccesPlan.statuscode = cmc_successplan_statuscode.Completed;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingToDo,
                creatingSuccesPlan,
            });

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingToDo, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddPostEntityImage(mockServiceProvider, "PostImage", creatingToDo);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);
            successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            Entity result = new Entity();
            xrmFakedContext.Data["cmc_successplan"].TryGetValue(creatingSuccesPlan.Id, out result);

            Assert.IsNotNull(result);
            #endregion

        }
        
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void SetSuccessPlanStatusService_Exception()
        {
            #region ARRANGE

            var xrmFakedContext = new XrmFakedContext();

            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockILanguageService.Object);

            #endregion

            #region ASSERT

            Assert.ThrowsException<InvalidPluginExecutionException>(() => successPlanService.SetSuccessPlanStatus(mockExecutionContext.Object));
            #endregion

        }

        private cmc_todo PrepareToDoEntity(Guid successplanid)
        {
            var todo1 = new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_successplanid = new EntityReference("cmc_successplan", successplanid),
                cmc_todoname = "TEST_TO_DO",
                statecode = cmc_todoState.Active,
                statuscode = cmc_todo_statuscode.Incomplete
            };
            return todo1;
        }

        private cmc_todo PrepareToDoEntity1(Guid successplanid)
        {
            var todo1 = new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_todoname = "TEST_TO_DO",
                statecode = cmc_todoState.Active,
                statuscode = cmc_todo_statuscode.Incomplete
            };
            return todo1;
        }

        private cmc_successplan PrepareSuccessPlan()
        {
            var success1 = new cmc_successplan()
            {
                Id = Guid.NewGuid(),
                //cmc_successplanId = Guid.NewGuid(),
                cmc_successplanname = "TestSuccessPlan",
                statecode = cmc_successplanState.Inactive,
                statuscode = cmc_successplan_statuscode.Inactive
            };
            return success1;
        }




    }
}
