using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Retention.Tests.SuccessPlan.Functions
{
    [TestClass]
    public class SuccessPlanAssignmentTest : XrmUnitTestBase
    {
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void SuccessPlanAssignment_Static_AssignSucessPlan()
        {

            #region ARRANGE
            var userEntity = PrepareUserEntity();
            var AccountEntity = PrepareAccountEntity();
            var sucessPlanTemplateEntity = PrepareSucessPlanTemplateEntity();
            var sucessPlanAssignmentEntity = PrepareSucessPlanEntity(sucessPlanTemplateEntity);
            var academicPeriod = PrepareAcademicPeriodEntity();
            var contact = PrepareContactEntity(AccountEntity, academicPeriod);
            var listEntity = PrepareListEntity(userEntity, sucessPlanAssignmentEntity, contact);
            var toDoTemplateEntity = PrepareToDoTemplateEntity(sucessPlanTemplateEntity);

            var xrmFakedContext = new XrmFakedContext();

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "cmc_successplan" };
            xrmFakedContext.InitializeMetadata(entityMetadata);
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
            var entityMetadataCollection = new EntityMetadataCollection();
            entityMetadataCollection.Add(entityMetadata);
            var retrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadataCollection}
                        }
            };

            xrmFakedContext.AddRelationship("cmc_successplanassignment_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_successplanassignment_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_successplanassignment.EntityLogicalName,
                Entity1Attribute = "cmc_successplanassignmentid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });

            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid",
            });

            xrmFakedContext.AddRelationship("cmc_contact_successplanassignment", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_contact_successplanassignment",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Contact.EntityLogicalName,
                Entity1Attribute = "contactid",
                Entity2LogicalName = cmc_successplanassignment.EntityLogicalName,
                Entity2Attribute = "cmc_successplanassignmentid",
            });

            var associationEntity = new Entity("cmc_successplanassignment_list", Guid.NewGuid())
            {
                ["listid"] = listEntity.Id,
                ["cmc_successplanassignmentid"] = sucessPlanAssignmentEntity.Id
            };

            var associationListEntity = new Entity("listmember", Guid.NewGuid())
            {
                ["listid"] = listEntity.Id,
                ["entityid"] = contact.Id,
            };
            var associationcontactEntity = new Entity("cmc_contact_successplanassignment", Guid.NewGuid())
            {
                ["contactid"] = Guid.NewGuid(),
                ["cmc_successplanassignmentid"] = sucessPlanAssignmentEntity.Id
            };


            EntityReferenceCollection listCollection = new EntityReferenceCollection();
            listCollection.Add(new EntityReference(List.EntityLogicalName, listEntity.Id));


            xrmFakedContext.Initialize(new List<Entity>()
            {
                associationEntity,
                associationListEntity,
                associationcontactEntity,
                userEntity,
                AccountEntity,
                sucessPlanTemplateEntity,
                listEntity,
                academicPeriod,
                contact,
                sucessPlanAssignmentEntity,
                toDoTemplateEntity
            });

            #endregion

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>.Ignored)).Returns(retrieveMetadataChangesResponse);

            var contactService = new ContactService(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SuccessPlanAssignmentLogic(xrmFakedContext.GetFakedOrganizationService());

            #region Assert

            var data = xrmFakedContext.Data["cmc_successplan"].Count > 0;
            Assert.IsTrue(data);


            #endregion Assert

        }


        [TestMethod]
        [TestCategory("Function"), TestCategory("Positive")]
        public void SuccessPlanAssignment_Dynamic_AssignSucessPlan()
        {

            #region ARRANGE
            var userEntity = PrepareUserEntity();
            var AccountEntity = PrepareAccountEntity();
            var sucessPlanTemplateEntity = PrepareSucessPlanTemplateEntity();
            var sucessPlanAssignmentEntity = PrepareSucessPlanEntity(sucessPlanTemplateEntity);
            var academicPeriod = PrepareAcademicPeriodEntity();
            var contact = PrepareContactEntity(AccountEntity, academicPeriod);
            var listEntity = PrepareListEntity(userEntity, sucessPlanAssignmentEntity, contact);
            listEntity.Type = true;
            var toDoTemplateEntity = PrepareToDoTemplateEntity(sucessPlanTemplateEntity);

            var xrmFakedContext = new XrmFakedContext();

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "cmc_successplan" };
            xrmFakedContext.InitializeMetadata(entityMetadata);
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
            var entityMetadataCollection = new EntityMetadataCollection();
            entityMetadataCollection.Add(entityMetadata);
            var retrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadataCollection}
                        }
            };

            xrmFakedContext.AddRelationship("cmc_successplanassignment_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_successplanassignment_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_successplanassignment.EntityLogicalName,
                Entity1Attribute = "cmc_successplanassignmentid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });

            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid",
            });

            xrmFakedContext.AddRelationship("cmc_contact_successplanassignment", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_contact_successplanassignment",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Contact.EntityLogicalName,
                Entity1Attribute = "contactid",
                Entity2LogicalName = cmc_successplanassignment.EntityLogicalName,
                Entity2Attribute = "cmc_successplanassignmentid",
            });

            var associationEntity = new Entity("cmc_successplanassignment_list", Guid.NewGuid())
            {
                ["listid"] = listEntity.Id,
                ["cmc_successplanassignmentid"] = sucessPlanAssignmentEntity.Id
            };

            var associationListEntity = new Entity("listmember", Guid.NewGuid())
            {
                ["listid"] = listEntity.Id,
                ["entityid"] = contact.Id,
            };
            var associationcontactEntity = new Entity("cmc_contact_successplanassignment", Guid.NewGuid())
            {
                ["contactid"] = Guid.NewGuid(),
                ["cmc_successplanassignmentid"] = sucessPlanAssignmentEntity.Id
            };


            EntityReferenceCollection listCollection = new EntityReferenceCollection();
            listCollection.Add(new EntityReference(List.EntityLogicalName, listEntity.Id));


            xrmFakedContext.Initialize(new List<Entity>()
            {
                associationEntity,
                associationListEntity,
                associationcontactEntity,
                userEntity,
                AccountEntity,
                sucessPlanTemplateEntity,
                listEntity,
                academicPeriod,
                contact,
                sucessPlanAssignmentEntity,
                toDoTemplateEntity
            });
            ConditionExpression condition1 = new ConditionExpression();
            condition1.AttributeName = "statecode";
            condition1.Operator = ConditionOperator.Equal;
            condition1.Values.Add(0);

            FilterExpression filter1 = new FilterExpression();
            filter1.FilterOperator = LogicalOperator.Or;
            filter1.AddCondition(condition1);

            QueryExpression queryExpression = new QueryExpression()
            {
                EntityName = "contact",
                Distinct = false,
                Criteria = filter1

            };
            queryExpression.ColumnSet.AddColumns("contactid", "fullname", "emailaddress1", "parentcustomerid", "telephone1", "cmc_currentretentionscore", "cmc_currentretentionscoredate");

            FetchXmlToQueryExpressionResponse fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse() {
                Results = new ParameterCollection
                        {
                            { "Query", queryExpression}
                        }
            };

            #endregion

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>.Ignored)).Returns(retrieveMetadataChangesResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.Ignored)).Returns(fetchXmlToQueryExpressionResponse);

            var contactService = new ContactService(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SuccessPlanAssignmentLogic(xrmFakedContext.GetFakedOrganizationService());

            #region Assert

            var data = xrmFakedContext.Data["cmc_successplan"].Count > 0;
            Assert.IsTrue(data);


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

        private cmc_successplantemplate PrepareSucessPlanTemplateEntity()
        {
            return new cmc_successplantemplate()
            {
                Id = Guid.NewGuid(),
                cmc_successplantemplatename = "Test Template",
                statecode = cmc_successplantemplateState.Active
            };
        }

        private List PrepareListEntity(SystemUser user, cmc_successplanassignment successplanassignment, Contact contact)
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
                StateCode = ListState.Active,
                cmc_successplanassignment_list = new List<cmc_successplanassignment>() { successplanassignment },
                listcontact_association = new List<Contact>() { contact },

            };
        }
        private Contact PrepareContactEntity(Account account, mshied_academicperiod accademicPeriod)
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Name",
                ParentCustomerId = account.ToEntityReference(),
                Telephone1 = "3232322323",
                cmc_currentretentionscoredate = DateTime.Now,
                mshied_CurrentAcademicPeriodId = accademicPeriod.ToEntityReference(),
               
            };
        }

        private cmc_successplanassignment PrepareSucessPlanEntity(cmc_successplantemplate template)
        {
            return new cmc_successplanassignment()
            {
                Id = Guid.NewGuid(),
                cmc_successplantemplateid = template.ToEntityReference(),
                statecode = cmc_successplanassignmentState.Active,
            };
        }
        private mshied_academicperiod PrepareAcademicPeriodEntity()
        {
            return new mshied_academicperiod()
            {
                Id = Guid.NewGuid(),
                statecode = mshied_academicperiodState.Active,
                mshied_StartDate = DateTime.Now.AddDays(-2),
                mshied_EndDate = DateTime.Now,

            };
        }
        private cmc_successplantodotemplate PrepareToDoTemplateEntity(cmc_successplantemplate template)
        {
            return new cmc_successplantodotemplate()
            {
                Id = Guid.NewGuid(),
                cmc_duedatecalculationtype = cmc_successplantodotemplate_cmc_duedatecalculationtype.Calculated,
                cmc_duedatecalculationfield = cmc_successplantodotemplate_cmc_duedatecalculationfield.StartofAcademicPeriod,
                cmc_successplantemplateid = template.ToEntityReference()
            };
        }
    }
}
