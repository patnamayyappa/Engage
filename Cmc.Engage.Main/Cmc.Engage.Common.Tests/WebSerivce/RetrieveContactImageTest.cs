using System;
using System.Collections.Generic;
using System.IO;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.WebSerivce
{
    [TestClass]
    public class RetrieveContactImageTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveContactImageTest_RetriveContactEntityImageValue()
        {

            #region Arrange
            var contact = CreateContactEntity();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contact, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockContactImageService = new RetrieveContactImageLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'ContactId':'" + contact.Id + "'}";
            
            var data = mockContactImageService.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert
            Assert.IsNotNull(data);
            #endregion
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveContactImageTest_RetriveContactEntityImageValue_ContactIsNull_ThrowNull()
        {

            #region Arrange
            var contact = CreateContactEntity();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contact, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockContactImageService = new RetrieveContactImageLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'ContactId':''}";

            var data = mockContactImageService.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert
            Assert.IsNull(data);
            #endregion
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveContactImageTest_RetriveContactEntityImageValue_StringIsNull_ThrowNull()
        {

            #region Arrange
            var contact = CreateContactEntity();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contact, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockContactImageService = new RetrieveContactImageLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "";

            var data = mockContactImageService.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert
            Assert.IsNull(data);
            #endregion
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveContactImageTest_RetriveContactEntityImageValue_ArgumentNullException()
        {

            #region Arrange
            
            var xrmFakedContext = new XrmFakedContext();
            
            #endregion Arrange

            #region Act

            var mockLogger = new Mock<ILogger>();
            
            #endregion Act

            #region Assert
            Assert.ThrowsException<ArgumentNullException>(() => new RetrieveContactImageLogic(null, null));
            Assert.ThrowsException<ArgumentException>(() => new RetrieveContactImageLogic(mockLogger.Object, null));
            #endregion
        }
        private Models.Contact CreateContactEntity()
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
                EntityImage = new []{byte.Parse("0101") }
            };
        }
    }
}
