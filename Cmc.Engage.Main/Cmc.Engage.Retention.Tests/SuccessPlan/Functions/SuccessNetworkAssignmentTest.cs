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

namespace Cmc.Engage.Retention.Tests.SuccessPlan.Functions
{
    [TestClass]
    public class SuccessNetworkAssignmentTest : XrmUnitTestBase
    {
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        [TestCategory("Function"), TestCategory("Positive")]
        public void SuccessNetwork_Assignment_SuccessNetworkAssigned()
        {
            #region Arrange
            var xrmFakedContext = new XrmFakedContext();
            var userEntity = PrepareUserEntity();
            var accountEntity = PrepareAccountEntity();
            var academicPeriod = PrepareAcademicperiod();
            var title = PrepareTitle();
            var contact = PrepareContactEntity(accountEntity, academicPeriod);
            var successNetworkAssignment = PrepareSuccessNetworkAssignment(userEntity, contact,title);
            var listEntity = PrepareListEntity(userEntity);
            var successNetwork = PrepareSuccessNetwork();

            var associationEntity = new Entity("cmc_successnetworkassignment_list", Guid.NewGuid())
            {
                ["listid"] = listEntity.Id,
                ["cmc_successnetworkassignmentid"] = successNetworkAssignment.Id
            };

            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = listEntity.Id                
            };

           

           

            var contactList = new List<Contact>();
            var newcontact=new Contact
            {
                Id=Guid.NewGuid(),
             
                FirstName = "Test2",
                LastName = "Name2",
               ParentCustomerId = accountEntity.ToEntityReference(),
                Telephone1 = "3232322323",
                cmc_currentretentionscoredate = DateTime.Now,
                mshied_CurrentAcademicPeriodId = academicPeriod.ToEntityReference()
            };
            contactList.Add(contact);
            //contactList.Add(newcontact);

            var associateSuccessnetworkSuccessnetworkassignment = new Entity("cmc_successnetwork_successnetworkassignment", Guid.NewGuid())
            {
                ["cmc_successnetworkid"] =  successNetwork.Id,
                ["cmc_successnetworkassignmentid"] = successNetworkAssignment.Id,
                ["cmc_successnetwork1.cmc_studentid"] = new AliasedValue("cmc_successnetwork", "cmc_studentid", new EntityReference("contact", newcontact.Id))
            };

            var assocationListMemberNew = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = newcontact.Id,
                ["listid"] = listEntity.Id
            };

            var entityList = new List<Entity>()
            {
                assocationListMember,
                assocationListMemberNew,
                associationEntity,
                userEntity,
                accountEntity,
                successNetworkAssignment,
                listEntity,
                //contact,
                academicPeriod,
                associateSuccessnetworkSuccessnetworkassignment,
                successNetwork,
                
            };
           entityList.AddRange(contactList);

            xrmFakedContext.Initialize(entityList);

            xrmFakedContext.AddRelationship("listlead_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });

            xrmFakedContext.AddRelationship("cmc_successnetworkassignment_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_successnetworkassignment_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = cmc_successnetworkassignment.EntityLogicalName,
                Entity2Attribute = "listid"
            });

            xrmFakedContext.AddRelationship("cmc_successnetwork_successnetworkassignment", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_successnetwork_successnetworkassignment",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_successnetwork.EntityLogicalName,
                Entity1Attribute = "cmc_successnetworkid",
                Entity2LogicalName = cmc_successnetworkassignment.EntityLogicalName,
                Entity2Attribute = "cmc_successnetworkassignmentid"
            });


            #endregion Arrange

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService =  new ContactService(mockLogger.Object, null , xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);
            var mockSuccessNetworkService = new SuccessNetworkService(xrmFakedContext.GetFakedOrganizationService(), mockLogger.Object, mockContactService);
            mockSuccessNetworkService.SuccessNetworkAssignment(xrmFakedContext.GetFakedOrganizationService());

            #endregion Act

            #region Assert
            
            var dataSuccessNetworkAssignment = xrmFakedContext.Data["cmc_successnetwork_successnetworkassignment"];
            var dataSuccessNetwork = xrmFakedContext.Data["cmc_successnetwork"];
          
            Assert.AreEqual(dataSuccessNetwork.Count, 2);

            #endregion Assert
        }


        private SystemUser PrepareUserEntity()
        {
            return new SystemUser()
            {
                Id = Guid.NewGuid(),
            };
        }

        private Account PrepareAccountEntity()
        {
            return new Account()
            {
                Id = Guid.NewGuid(),
            };
        }

        private List PrepareListEntity(SystemUser user)
        {
            return new List()
            {
                Id = Guid.NewGuid(),
                ListName = "Test List",
                Type = false,
                Query = "&lt;fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"false\"&gt;&lt;entity name=\"contact\"&gt;&lt;attribute name=\"contactid\" /&gt;&lt;order attribute=\"fullname\" descending=\"false\" /&gt;&lt;filter type=\"and\"&gt;&lt;condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /&gt;&lt;/filter&gt;&lt;attribute name=\"fullname\" /&gt;&lt;attribute name=\"emailaddress1\" /&gt;&lt;attribute name=\"parentcustomerid\" /&gt;&lt;attribute name=\"telephone1\" /&gt;&lt;attribute name=\"cmc_currentretentionscore\" /&gt;&lt;attribute name=\"cmc_currentretentionscoredate\" /&gt;&lt;/entity&gt;&lt;/fetch&gt;",
                OwnerId = user.ToEntityReference(),
                cmc_marketinglisttype = cmc_list_cmc_marketinglisttype.StudentGroup,
                CreatedFromCode = list_createdfromcode.Contact,
                StateCode=ListState.Active
            };
        }
        private Contact PrepareContactEntity(Account account, mshied_academicperiod academicPeriod)
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Name",
                ParentCustomerId = account.ToEntityReference(),
                Telephone1 = "3232322323",
                cmc_currentretentionscoredate = DateTime.Now,
                mshied_CurrentAcademicPeriodId = academicPeriod.ToEntityReference(),
                
            };
        }

        private cmc_successnetworkassignment PrepareSuccessNetworkAssignment(SystemUser user,Contact contact,Entity title)
        {
            return new cmc_successnetworkassignment()
            {
                Id = Guid.NewGuid(),
               // statuscode=cmc_successnetworkassignment_statuscode.Active,
                OwnerId = user.ToEntityReference(),
                statecode =cmc_successnetworkassignmentState.Active,
                cmc_successnetworkassignmentname ="Adviser Test",
                cmc_assigntoid = new EntityReference("contact",contact.Id),
                cmc_staffroleid = title.ToEntityReference()
            };
        }

        private Entity PrepareTitle()
        {
            return new Entity("cmc_title", Guid.NewGuid())
            {
                ["cmc_titlename"] = "Advisor"
            };
        }

        private mshied_academicperiod PrepareAcademicperiod()
        {
            return new mshied_academicperiod() {
            Id = Guid.NewGuid(),
            statecode=mshied_academicperiodState.Active
            };
        }

        private cmc_successnetwork PrepareSuccessNetwork()
        {
            return new cmc_successnetwork() {
                Id=Guid.NewGuid()
            };
        }
    }
}
