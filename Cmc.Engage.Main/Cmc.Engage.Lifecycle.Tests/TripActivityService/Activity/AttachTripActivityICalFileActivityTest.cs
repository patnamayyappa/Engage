using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.TripActivityService.Activity
{
    [TestClass]
    public  class AttachTripActivityICalFileActivityTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Activity"), TestCategory("Positive")]
        public void AttachTripActivityICalFileActivity()
        {
            var creatingEmail = PreparingEmaailInstance();
            var user = PreparingSystemUserInstance();
            var tripActivity = PreparingTripActivityInstance(user.Id);
            
            var voluteer = PreparingvolunteersInstance();
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {
                ["cmc_tripactivity_systemuserid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = tripActivity.Id,
                ["systemuserid"] = user.Id
            };
            var associatevoluteerForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {
                ["cmc_tripactivity_contactid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = tripActivity.Id,
                ["contactid"] = voluteer.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
              
                associateStaffMemberForTripActivity,
                associatevoluteerForTripActivity,
                creatingEmail,
                user,
                tripActivity,
                voluteer
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = user.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = voluteer.LogicalName,
                Entity2Attribute = "contactid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", tripActivity}
            };
            var sendEmailResponse = new SendEmailResponse()
            {
                ResponseName = "SendEmailResponse",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<SendEmailRequest>._)).Returns(sendEmailResponse);                          

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AttachTripActivityICalFileActivity(creatingEmail.ToEntityReference(), tripActivity.ToEntityReference(),tripActivity.cmc_StartDateTime.Value,tripActivity.cmc_EndDateTime.Value,tripActivity.cmc_location, "FileName", "Subject", "Description");
            #endregion ACT

            #region ASSERT


           var emailResult= xrmFakedContext.GetFakedOrganizationService()
                .Retrieve(creatingEmail.LogicalName, creatingEmail.Id, new ColumnSet(true));
            var receiptants = emailResult.Attributes["to"];
            var receiptantsCount = ((EntityCollection)receiptants).Entities.Count;
            Assert.IsNotNull(receiptants);
            Assert.IsTrue(receiptantsCount == 2);
            #endregion ASSERT
        }
     
        [TestMethod]
        [TestCategory("Activity"), TestCategory("Negative")]
        public void AttachTripActivityICalFileActivity_NoStaffMembersAndVolunteers()
        {
            var creatingEmail = PreparingEmaailInstance();
            var user = PreparingSystemUserInstance();
            var tripActivity = PreparingTripActivityInstance(user.Id);
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {
                ["cmc_tripactivity_systemuserid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = tripActivity.Id,
                ["systemuserid"] = user.Id
            };

            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateStaffMemberForTripActivity,
                creatingEmail,              
                tripActivity,
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = user.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", tripActivity}
            };
            var sendEmailResponse = new SendEmailResponse()
            {
                ResponseName = "SendEmailResponse",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<SendEmailRequest>._)).Returns(sendEmailResponse);

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AttachTripActivityICalFileActivity(creatingEmail.ToEntityReference(), tripActivity.ToEntityReference(), tripActivity.cmc_StartDateTime.Value, tripActivity.cmc_EndDateTime.Value, tripActivity.cmc_location,string.Empty,string.Empty,string.Empty);
            #endregion ACT

            #region ASSERT


            var emailResult = xrmFakedContext.GetFakedOrganizationService()
                 .Retrieve(creatingEmail.LogicalName, creatingEmail.Id, new ColumnSet(true));
            var receiptants = emailResult.Attributes.Keys.Contains("to");       
            Assert.IsTrue(receiptants==false);
          
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Activity"), TestCategory("Negative")]
        public void AttachTripActivityICalFileActivity_NoTripActivity()
        {
            var creatingEmail = PreparingEmaailInstance();

        //    cmc_tripactivity tripActivity = null;
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {            
                creatingEmail                             
            });
                           
            var sendEmailResponse = new SendEmailResponse()
            {
                ResponseName = "SendEmailResponse",            
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<SendEmailRequest>._)).Returns(sendEmailResponse);

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AttachTripActivityICalFileActivity(creatingEmail.ToEntityReference(), null, DateTime.Now, DateTime.Now.AddDays(5), string.Empty, null, string.Empty, string.Empty);
            #endregion ACT

            #region ASSERT


            var emailResult = xrmFakedContext.GetFakedOrganizationService()
                 .Retrieve(creatingEmail.LogicalName, creatingEmail.Id, new ColumnSet(true));
            var receiptants = emailResult.Attributes.Keys.Contains("to");
            Assert.IsTrue(receiptants == false);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Activity"), TestCategory("Negative")]
        public void AttachTripActivityICalFileActivity_NoFilename()
        {
            var creatingEmail = PreparingEmaailInstance();
            var user = PreparingSystemUserInstance();
            var tripActivity = PreparingTripActivityInstance(user.Id);

            var voluteer = PreparingvolunteersInstance();
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {
                ["cmc_tripactivity_systemuserid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = tripActivity.Id,
                ["systemuserid"] = user.Id
            };
            var associatevoluteerForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {
                ["cmc_tripactivity_contactid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = tripActivity.Id,
                ["contactid"] = voluteer.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {

                associateStaffMemberForTripActivity,
                associatevoluteerForTripActivity,
                creatingEmail,
                tripActivity,
                voluteer
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = user.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = voluteer.LogicalName,
                Entity2Attribute = "contactid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", tripActivity}
            };
            var sendEmailResponse = new SendEmailResponse()
            {
                ResponseName = "SendEmailResponse",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<SendEmailRequest>._)).Returns(sendEmailResponse);

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AttachTripActivityICalFileActivity(creatingEmail.ToEntityReference(), tripActivity.ToEntityReference(), tripActivity.cmc_StartDateTime.Value, tripActivity.cmc_EndDateTime.Value, tripActivity.cmc_location, null, string.Empty, string.Empty);
            #endregion ACT

            #region ASSERT


            var emailResult = xrmFakedContext.GetFakedOrganizationService()
                 .Retrieve(creatingEmail.LogicalName, creatingEmail.Id, new ColumnSet(true));
            var receiptants = emailResult.Attributes["to"];
            var receiptantsCount = ((EntityCollection)receiptants).Entities.Count;
            Assert.IsNotNull(receiptants);
            Assert.IsTrue(receiptantsCount == 1);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Activity"), TestCategory("Negative")]
        public void AttachTripActivityICalFileActivity_NoFrom()
        {
            var creatingEmail = PreparingEmaailInstanceForNoFrom();
            var user = PreparingSystemUserInstance();
            var tripActivity = PreparingTripActivityInstanceEmailSequenceIsnotNull(user.Id);

            var voluteer = PreparingvolunteersInstance();
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {
                ["cmc_tripactivity_systemuserid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = tripActivity.Id,
                ["systemuserid"] = user.Id
            };
            var associatevoluteerForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {
                ["cmc_tripactivity_contactid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = tripActivity.Id,
                ["contactid"] = voluteer.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {

                associateStaffMemberForTripActivity,
                associatevoluteerForTripActivity,
                creatingEmail,
                tripActivity,
                voluteer
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = user.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = tripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = voluteer.LogicalName,
                Entity2Attribute = "contactid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", tripActivity}
            };
            var sendEmailResponse = new SendEmailResponse()
            {
                ResponseName = "SendEmailResponse",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<SendEmailRequest>._)).Returns(sendEmailResponse);

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AttachTripActivityICalFileActivity(creatingEmail.ToEntityReference(), tripActivity.ToEntityReference(), tripActivity.cmc_StartDateTime.Value, tripActivity.cmc_EndDateTime.Value, tripActivity.cmc_location, null, string.Empty, string.Empty);
            #endregion ACT

            #region ASSERT


            var emailResult = xrmFakedContext.GetFakedOrganizationService()
                 .Retrieve(creatingEmail.LogicalName, creatingEmail.Id, new ColumnSet(true));
            var receiptants = emailResult.Attributes.Keys.Contains("to");
            Assert.IsTrue(receiptants == false);
            #endregion ASSERT
        }

        private Email PreparingEmaailInstance()
        {
            var activityParty = new ActivityParty
            {
                Id = new Guid(),
                AddressUsed = "Test@campusmgmgt.com"
            };
          
            var email = new Email()
            {
                Id = Guid.NewGuid(),
                AttachmentOpenCount = null,
                To =null,
                From = new List<ActivityParty> { activityParty }
            };

            return email;
        }
        private Email PreparingEmaailInstanceForNoFrom()
        {
          

            var email = new Email()
            {
                Id = Guid.NewGuid(),
                AttachmentOpenCount = null,
                To = null,
                From = null
            };

            return email;
        }

        private cmc_tripactivity PreparingTripActivityInstanceEmailSequenceIsnotNull(Guid userId)
        {
            var tripActivity = new cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                cmc_StartDateTime = DateTime.Now,
                cmc_EndDateTime = DateTime.Now.AddDays(5),
                cmc_location = "bangalore",
                OwnerId = new EntityReference("systemuser", userId),
                cmc_emailsequence = 1
            };
            return tripActivity;
        }

        private cmc_tripactivity PreparingTripActivityInstance(Guid userId)
        {
            var tripActivity = new cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                cmc_StartDateTime = DateTime.Now,
                cmc_EndDateTime = DateTime.Now.AddDays(5),
                cmc_location = "bangalore",
                OwnerId = new EntityReference("systemuser", userId),
                cmc_emailsequence = null
            };
            return tripActivity;
        }

        private SystemUser PreparingSystemUserInstance()
        {
            var sustemUser = new SystemUser()
            {
                Id = Guid.NewGuid(),           
               
             };
            return sustemUser;
        }

        private Contact PreparingvolunteersInstance()
        {
            var contact = new Contact()
            {
                Id = Guid.NewGuid(),

            };
            return contact;
        }
    }
   

}
