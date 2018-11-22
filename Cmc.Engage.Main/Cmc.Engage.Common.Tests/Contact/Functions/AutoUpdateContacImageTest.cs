using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using Cmc.Engage.Models;

namespace Cmc.Engage.Common.Tests.Contact.Functions
{
    [TestClass]
    public class AutoUpdateContacImageTest
    {
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetContacImage_setIamge()
        {
            #region Arrange
            var student = creatStudent("ankurkushwaha47@gmail.com");
            var student1 = creatStudent("testdevatest@gmail.com");
            var student2 = PrepareContact2();
            var configuration = PrepareConfiguration("https://www.gravatar.com/avatar/{0}?d=404",true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                student1,
                student2,
                configuration,
            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService = new ContactService(mockLogger.Object, null,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            mockContactService.SetContactImage();
            #endregion

            #region Assert

            xrmFakedContext.Data["contact"].TryGetValue(student1.Id, out var data);
            Assert.IsNotNull(data.Attributes["cmc_imagesource"]);

            #endregion
        }
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void SetContacImage_NullContact()
        {
            #region Arrange

            var student = PrepareIncativeContact(true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student
            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService = new ContactService(mockLogger.Object, null,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            mockContactService.SetContactImage();
            #endregion

            #region Assert

            xrmFakedContext.Data["contact"].TryGetValue(student.Id, out var data);
            Assert.IsNull(data.GetAttributeValue<int?>("cmc_imagesource"));

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void SetContacImage_NullConfiguration()
        {
            #region Arrange

            var student = PrepareIncativeContact(false);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student
            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService = new ContactService(mockLogger.Object, null,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            mockContactService.SetContactImage();
            #endregion

            #region Assert

            xrmFakedContext.Data["contact"].TryGetValue(student.Id, out var data);
            Assert.IsNull(data.GetAttributeValue<int?>("cmc_imagesource"));

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void SetContacImage_EmptyConfiguration_Url()
        {
            #region Arrange
            var student = creatStudent("ankurkushwaha47@gmail.com");
            var configuration = PrepareConfiguration("", true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                configuration,
            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService = new ContactService(mockLogger.Object, null,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            mockContactService.SetContactImage();
            #endregion

            #region Assert

            xrmFakedContext.Data["contact"].TryGetValue(student.Id, out var data);
            Assert.IsNotNull(data.Attributes["cmc_imagesource"]);

            #endregion
        }
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void SetContacImage_InactiveItegration()
        {
            #region Arrange
            var student = creatStudent("ankurkushwaha47@gmail.com");
            var configuration = PrepareConfiguration("https://www.gravatar.com/avatar/{0}?d=404", false);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                configuration
            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService = new ContactService(mockLogger.Object, null,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            mockContactService.SetContactImage();
            #endregion

            #region Assert

            xrmFakedContext.Data["contact"].TryGetValue(student.Id, out var data);
            Assert.IsNotNull(data.Attributes["cmc_imagesource"]);

            #endregion
        }


        private Entity creatStudent(string email)
        {
            var contact = new Models.Contact()
            {
                Id = Guid.NewGuid(),
                ["emailaddress1"] = email,
                StatusCode = contact_statuscode.Active

            };
            return contact;
        }
        private cmc_configuration PrepareConfiguration(string url,bool flag)
        {
            return new cmc_configuration()
            {
                Id = Guid.NewGuid(),
                cmc_imageintegrationgravatarurl = url,
                ["cmc_imageintegrationgravatarrequired"] = flag
            };
        }
        private Models.Contact PrepareContact2()
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
                cmc_externalimagetimestamp = "6712334",
                cmc_imagesource = cmc_contact_cmc_imagesource.Gravatar,
                StatusCode = contact_statuscode.Active
            };
        }
        private Models.Contact PrepareIncativeContact(bool isinActive)
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
                cmc_externalimagetimestamp = "6712334",
                EMailAddress1 ="jojiunit@campusmgmt.com",
                StatusCode = isinActive ? contact_statuscode.Inactive : contact_statuscode.Active,
                cmc_imagesource = null
            };
        }
    }

}
