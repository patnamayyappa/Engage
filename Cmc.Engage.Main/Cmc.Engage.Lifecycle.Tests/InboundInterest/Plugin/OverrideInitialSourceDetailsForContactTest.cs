using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.InboundInterest.Plugin
{
    [TestClass]
    public class OverrideInitialSourceDetailsForContactTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]

        //Creation Of Inbound Interest
        public void OverrideInitialSourceDetailsForContact_CreationOfInboundInterestForLead()
        {
            #region ARRANGE
            var creatingContact = PreparingContact();
            var creatingLeadFirst = PreparingLeadFirst(creatingContact,false);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingContact,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingLeadFirst, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService = new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockInboundInterestService.OverrideInitialSourceDetailsForContact(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            
            var resultData = new Entity("contact");
            xrmFakedContext.Data["contact"].TryGetValue(creatingContact.Id, out resultData);
            var data = (EntityReference)resultData.Attributes["cmc_sourcecampusid"];
            Assert.IsNotNull(data);

            #endregion

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]

        //Creation Of Inbound Interest
        public void OverrideInitialSourceDetailsForContact_CreationOfInboundInterestForLead_InboundInterestList_IsNotNull()
        {
            #region ARRANGE
            var creatingContact = PreparingContact();
            var creatingLeadFirst = PreparingLeadFirst(creatingContact,true);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingLeadFirst,
                creatingContact,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingLeadFirst, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService = new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockInboundInterestService.OverrideInitialSourceDetailsForContact(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var resultData = new Entity("contact");
            xrmFakedContext.Data["contact"].TryGetValue(creatingContact.Id, out resultData);
            var data = (EntityReference)resultData.Attributes["cmc_sourcecampusid"];
            Assert.IsNotNull(data);

            #endregion

        }


        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]

        //Updation Of Inbound Interest

        public void OverrideInitialSourceDetailsForContact_UpdaionOfInboundInterestForLead()
        {
            #region ARRANGE
            var creatingContact = PreparingContact();
            var PreviouscreatingLeadFirst = PreparingLeadFirst(creatingContact,false);
            var PostcreatingLeadSecond = PreparingLeadSecond(creatingContact);
            var creatingLeadpreImageEntity = PreparingImageLead(PreviouscreatingLeadFirst, creatingContact);
            var creatingLeadpostimageEntity = PreparingPostImageLead(PostcreatingLeadSecond, creatingContact);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    creatingContact,
                    PreviouscreatingLeadFirst,
                    PostcreatingLeadSecond
                });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, PostcreatingLeadSecond, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService = new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddPreEntityImage(mockServiceProvider, "Lead", creatingLeadpreImageEntity);
            AddPostEntityImage(mockServiceProvider, "Lead", creatingLeadpostimageEntity);
            mockInboundInterestService.OverrideInitialSourceDetailsForContact(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var resultData = new Entity("contact");
            var resultDATA = new Entity("lead");
            xrmFakedContext.Data["contact"].TryGetValue(creatingContact.Id, out resultData);
            xrmFakedContext.Data["lead"].TryGetValue(PostcreatingLeadSecond.Id, out resultDATA);
            EntityReference contactdata = (EntityReference)resultData.Attributes["cmc_sourcecampusid"];
            EntityReference leaddata= (EntityReference)resultDATA.Attributes["cmc_sourcecampusid"];
            Assert.AreEqual(contactdata.Id, leaddata.Id);

            #endregion

        }


        
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void OverrideInitialSourceDetailsForContact_UpdaionOfInboundInterestForLeadPreImageCustomerIdNull()
        {
            #region ARRANGE
            var creatingContact = PreparingContact();
            var PreviouscreatingLeadFirst = PreparingLeadFirst(creatingContact, false);
            var PostcreatingLeadSecond = PreparingLeadSecond(creatingContact);
            var creatingLeadpreImageEntity = PreparingImageLeadNegative(PreviouscreatingLeadFirst, creatingContact);            
            var creatingLeadpostimageEntity = PreparingPostImageLeadNegative(PostcreatingLeadSecond, creatingContact);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    creatingContact,
                    PreviouscreatingLeadFirst,
                    PostcreatingLeadSecond
                });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, PostcreatingLeadSecond, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService = new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            creatingLeadpreImageEntity.CustomerId = null;
            AddPreEntityImage(mockServiceProvider, "Lead", creatingLeadpreImageEntity);
            AddPostEntityImage(mockServiceProvider, "Lead", creatingLeadpostimageEntity);
            mockInboundInterestService.OverrideInitialSourceDetailsForContact(mockExecutionContext.Object);


            creatingLeadpreImageEntity.CustomerId = new EntityReference("contact", creatingContact.Id);
            creatingLeadpostimageEntity.CustomerId = null;
            mockInboundInterestService.OverrideInitialSourceDetailsForContact(mockExecutionContext.Object);

            creatingLeadpostimageEntity.CustomerId = new EntityReference("");
            mockInboundInterestService.OverrideInitialSourceDetailsForContact(mockExecutionContext.Object);

           // creatingLeadpostimageEntity.CustomerId = new EntityReference("contact", creatingContact.Id);
            //creatingLeadpostimageEntity.cmc_Primary = false;
            // 
            //mockInboundInterestService.OverrideInitialSourceDetailsForContact(mockExecutionContext.Object);


            #endregion

            #region ASSERT
            // Negative Case No Assert
            Assert.IsTrue(true);

            #endregion

        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]

        //When Logger is not passing
        public void OverrideInitialSourceDetailsForContactNegativeScenario()
        {
            #region ARRANGE
            
            var xrmFakedContext = new XrmFakedContext();
           
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
           #endregion

            #region ASSERT
            Assert.ThrowsException<ArgumentNullException>(() => new InboundInterestService(null, xrmFakedContext.GetFakedOrganizationService()));

            #endregion

        }

        #region DATA PREPARATION

        private Contact PreparingContact()
        {
            var contact1 = new Contact()
            {
                ContactId = Guid.NewGuid(),
                FirstName = "Ankur k",
            };
            return contact1;
        }

        private Lead PreparingLeadFirst(Entity contactInstance,bool value)
        {
            var lead = new Lead()
            {
                Id = Guid.NewGuid(),
                CustomerId = new EntityReference("contact", contactInstance.Id),
                FirstName = "FirstLead",
                cmc_Primary = value,
                cmc_sourcedate = DateTime.Today,
                cmc_sourcecampusid = new EntityReference("account", Guid.NewGuid()),
                cmc_sourceprgmid = new EntityReference("mshied_program", Guid.NewGuid()),
                cmc_sourceprgmlevelid = new EntityReference("mshied_programlevel", Guid.NewGuid()),
                cmc_expectedstartid = new EntityReference("mshied_academicperiod", Guid.NewGuid()),
                cmc_sourcemethodid = new EntityReference("cmc_sourcemethod", Guid.NewGuid()),
                cmc_sourcecategoryid = new EntityReference("cmc_sourcecategory", Guid.NewGuid()),
                cmc_sourcesubcategoryid = new EntityReference("cmc_sourcesubcategory", Guid.NewGuid()),
                cmc_sourcereferringcontactid = new EntityReference("contact", Guid.NewGuid()),
                cmc_sourcereferringorganizationid = new EntityReference("account", Guid.NewGuid()),
                cmc_sourcereferringstaffid = new EntityReference("systemuser", Guid.NewGuid()),
                cmc_sourcecampaignid = new EntityReference("campaign", Guid.NewGuid())
            };
            return lead;
        }

        private Lead PreparingLeadSecond(Entity contactInstance)
        {
            var lead = new Lead()
            {
                Id = Guid.NewGuid(),
                CustomerId = new EntityReference("contact", contactInstance.Id),
                FirstName = "SecondLead",
                cmc_Primary = true,
                cmc_sourcedate = DateTime.Today.AddDays(1),
                cmc_sourcecampusid = new EntityReference("account", Guid.NewGuid()),
                cmc_sourceprgmid = new EntityReference("mshied_program", Guid.NewGuid()),
                cmc_sourceprgmlevelid = new EntityReference("mshied_programlevel", Guid.NewGuid()),
                cmc_expectedstartid = new EntityReference("mshied_academicperiod", Guid.NewGuid()),
                cmc_sourcemethodid = new EntityReference("cmc_sourcemethod", Guid.NewGuid()),
                cmc_sourcecategoryid = new EntityReference("cmc_sourcecategory", Guid.NewGuid()),
                cmc_sourcesubcategoryid = new EntityReference("cmc_sourcesubcategory", Guid.NewGuid()),
                cmc_sourcereferringcontactid = new EntityReference("contact", Guid.NewGuid()),
                cmc_sourcereferringorganizationid = new EntityReference("account", Guid.NewGuid()),
                cmc_sourcereferringstaffid = new EntityReference("systemuser", Guid.NewGuid()),
                cmc_sourcecampaignid = new EntityReference("campaign", Guid.NewGuid())
            };
            return lead;
        }

        private Entity PreparingImageLead(Entity PreviousLead, Entity contactInstance)
        {
            var preLeadEntity = new Lead()
            {
                Id = PreviousLead.Id,
                CustomerId = new EntityReference("contact", contactInstance.Id),
                FirstName = "PreImageLead",
                cmc_Primary = false,
                cmc_sourcedate = DateTime.Today,
                cmc_sourcecampusid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcecampusid"),
                cmc_sourceprgmid = PreviousLead.GetAttributeValue<EntityReference>("mshied_programid"),
                cmc_sourceprgmlevelid = PreviousLead.GetAttributeValue<EntityReference>("mshied_programlevelid"),
                cmc_expectedstartid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourceexpectedstartid"),
                cmc_sourcemethodid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcemethodid"),
                cmc_sourcecategoryid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcecategoryid"),
                cmc_sourcesubcategoryid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcesubcategoryid"),
                cmc_sourcereferringcontactid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcereferringcontactid"),
                cmc_sourcereferringorganizationid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcereferringorganizationid"),
                cmc_sourcereferringstaffid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcereferringstaffid"),
                cmc_sourcecampaignid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcecampaignid")

            };
            return preLeadEntity;
        }

        private Entity PreparingPostImageLead(Entity postImage, Entity contactInstance)
        {
            var postLeadEntity = new Lead()
            {
                Id = postImage.Id,
                CustomerId = new EntityReference("contact", contactInstance.Id),
                FirstName = "PostImageLead",
                cmc_Primary = true,
                cmc_sourcedate = DateTime.Today,
                cmc_sourcecampusid = postImage.GetAttributeValue<EntityReference>("cmc_sourcecampusid"),
                cmc_sourceprgmid = postImage.GetAttributeValue<EntityReference>("mshied_programid"),
                cmc_sourceprgmlevelid = postImage.GetAttributeValue<EntityReference>("mshied_programlevelid"),
                cmc_expectedstartid = postImage.GetAttributeValue<EntityReference>("cmc_sourceexpectedstartid"),
                cmc_sourcemethodid = postImage.GetAttributeValue<EntityReference>("cmc_sourcemethodid"),
                cmc_sourcecategoryid = postImage.GetAttributeValue<EntityReference>("cmc_sourcecategoryid"),
                cmc_sourcesubcategoryid = postImage.GetAttributeValue<EntityReference>("cmc_sourcesubcategoryid"),
                cmc_sourcereferringcontactid = postImage.GetAttributeValue<EntityReference>("cmc_sourcereferringcontactid"),
                cmc_sourcereferringorganizationid = postImage.GetAttributeValue<EntityReference>("cmc_sourcereferringorganizationid"),
                cmc_sourcereferringstaffid = postImage.GetAttributeValue<EntityReference>("cmc_sourcereferringstaffid"),
                cmc_sourcecampaignid = postImage.GetAttributeValue<EntityReference>("cmc_sourcecampaignid")
            };
            return postLeadEntity;
        }

        private Lead PreparingImageLeadNegative(Entity PreviousLead, Entity contactInstance)
        {
           return new Lead()
            {
                Id = PreviousLead.Id,
                CustomerId = new EntityReference("contact", contactInstance.Id),
                FirstName = "PreImageLead",
                cmc_Primary = false,
                cmc_sourcedate = DateTime.Today,
                cmc_sourcecampusid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcecampusid"),
                cmc_sourceprgmid = PreviousLead.GetAttributeValue<EntityReference>("mshied_programid"),
                cmc_sourceprgmlevelid = PreviousLead.GetAttributeValue<EntityReference>("mshied_programlevelid"),
                cmc_expectedstartid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourceexpectedstartid"),
                cmc_sourcemethodid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcemethodid"),
                cmc_sourcecategoryid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcecategoryid"),
                cmc_sourcesubcategoryid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcesubcategoryid"),
                cmc_sourcereferringcontactid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcereferringcontactid"),
                cmc_sourcereferringorganizationid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcereferringorganizationid"),
                cmc_sourcereferringstaffid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcereferringstaffid"),
                cmc_sourcecampaignid = PreviousLead.GetAttributeValue<EntityReference>("cmc_sourcecampaignid")

            };
           
        }
        private Lead PreparingPostImageLeadNegative(Entity postImage, Entity contactInstance)
        {
            return new Lead()
            {
                Id = postImage.Id,
                CustomerId = new EntityReference("contact", contactInstance.Id),
                FirstName = "PostImageLead",
                cmc_Primary = true,
                cmc_sourcedate = DateTime.Today,
                cmc_sourcecampusid = postImage.GetAttributeValue<EntityReference>("cmc_sourcecampusid"),
                cmc_sourceprgmid = postImage.GetAttributeValue<EntityReference>("mshied_programid"),
                cmc_sourceprgmlevelid = postImage.GetAttributeValue<EntityReference>("mshied_programlevelid"),
                cmc_expectedstartid = postImage.GetAttributeValue<EntityReference>("cmc_sourceexpectedstartid"),
                cmc_sourcemethodid = postImage.GetAttributeValue<EntityReference>("cmc_sourcemethodid"),
                cmc_sourcecategoryid = postImage.GetAttributeValue<EntityReference>("cmc_sourcecategoryid"),
                cmc_sourcesubcategoryid = postImage.GetAttributeValue<EntityReference>("cmc_sourcesubcategoryid"),
                cmc_sourcereferringcontactid = postImage.GetAttributeValue<EntityReference>("cmc_sourcereferringcontactid"),
                cmc_sourcereferringorganizationid = postImage.GetAttributeValue<EntityReference>("cmc_sourcereferringorganizationid"),
                cmc_sourcereferringstaffid = postImage.GetAttributeValue<EntityReference>("cmc_sourcereferringstaffid"),
                cmc_sourcecampaignid = postImage.GetAttributeValue<EntityReference>("cmc_sourcecampaignid")
            };            
        }
        #endregion

    }
}
