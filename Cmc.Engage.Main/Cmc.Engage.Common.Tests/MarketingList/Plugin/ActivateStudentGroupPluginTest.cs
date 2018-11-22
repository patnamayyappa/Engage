using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.MarketingList.Plugin
{
    [TestClass]
    public class ActivateStudentGroupPluginTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ActivateStudentGroupPluginTest_StudentGroup_StateCode_ActiveOrInAcive()
        {
            //Need to mock all the dependency Injections passed to the services constructor
            #region ARRANGE
            //Prepare the Target Instance for which the plugin would be trigerred
            var marketingListInstance = PrepareTargetInstance();

            //Prepare the FakeContext, here add all the entities 
            //which we expect to be in the database system
            //using the Initialize method
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity> { marketingListInstance });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, marketingListInstance, Operation.cmc_activatestudentgroups);

            //Add additional input parameters as required by the plugin. 
            //Required usually if the plugin is a custom action, else this step is omitted
            var marketingListEntityCollection = new EntityCollection();
            marketingListEntityCollection.Entities.Add(marketingListInstance);
            AddInputParameters(mockServiceProvider, "StudentGroups", marketingListEntityCollection);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion ARRANGE

            #region ACT
            //Instantiate the Class with the mocked dependencies.
            //Need to mock all the dependency Injections passed to the services constructor
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            //var mockOrganizationService = new Mock<IOrganizationService>();
            var marketingListService = new MarketingListService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            marketingListService.ActivateStudentGroup(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var postStudentGroupData = new Entity("List");
            xrmFakedContext.Data["List"].TryGetValue(marketingListInstance.Id, out postStudentGroupData);

            //Assert if the business logic performed is correct.
            var stateCode = postStudentGroupData.Attributes["statecode"].ToString();
            Assert.AreEqual("Active", stateCode);
            #endregion ASSERT
        }
        private Entity PrepareTargetInstance()
        {
            var marketingListInstance = new Entity("List", Guid.NewGuid())
            {
                ["cmc_marketinglisttype"] = cmc_list_cmc_marketinglisttype.StudentGroup,
                ["cmc_expirationdate"] = DateTime.Today.AddDays(-1).Date,
                ["CreatedFromCode"] = list_createdfromcode.Contact,
                ["participationtypemask"] = new OptionSetValue(2),
                ["statecode"] = ListState.Active
            };
            return marketingListInstance;
        }
    }
}