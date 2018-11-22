using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.MarketingList.Functions
{
    [TestClass]
    public class StudentGroupAutoExpireTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Function"), TestCategory("Positive")]
        public void StudentGroupAutoExpire_UpdateStatus_ForDatesOnOrBeforeToday()
        {
            #region ARRANGE

            var studentGroup = GetStudentGroup(-1);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                studentGroup
            });

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var mockMarketingListService = new MarketingListService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService());
            mockMarketingListService.StudentGroupAutoExpireLogic();

            #endregion

            #region ASSERT

            xrmFakedContext.Data["list"].TryGetValue(studentGroup.Id, out var data);
            var stateCode = data?.Attributes["statecode"];

            if (stateCode != null) Assert.IsTrue((ListState) ((OptionSetValue) stateCode).Value == ListState.Inactive);

            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void StudentGroupAutoExpire_NoStatusUpdate_ForDatesAfterToday()
        {
            #region ARRANGE

            var studentGroup = GetStudentGroup(2);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                studentGroup
            });

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var mockMarketingListService = new MarketingListService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService());
            mockMarketingListService.StudentGroupAutoExpireLogic();

            #endregion

            #region ASSERT

            xrmFakedContext.Data["list"].TryGetValue(studentGroup.Id, out var data);
            var stateCode = data?.Attributes["statecode"];

            if (stateCode != null) Assert.IsTrue((ListState)((OptionSetValue)stateCode).Value == ListState.Active);

            #endregion ASSERT  
        }

        private static Entity GetStudentGroup(int numberOfDays)
        {
            return new List
            {
                ListId = Guid.NewGuid(),
                ListName = "Test Student Group",
                StateCode = ListState.Active,
                cmc_marketinglisttype = cmc_list_cmc_marketinglisttype.StudentGroup,
                cmc_expirationdate = DateTime.Now.Date.AddDays(numberOfDays)
            };
        }
    }
}