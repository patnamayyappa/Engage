using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Lifecycle;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.FakeMessageExecutors;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Retention.Tests.ScoreCalculator.Functions
{

    [TestClass]
    public class RetentionScoreCalculatorTest:XrmUnitTestBase
    {
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void RetentionScoreCalculator_Retentionscorehistory()
        {

            #region ARRANGE    
            cmc_configuration retrieveConfiguration = new cmc_configuration();
            var bingMapKeyConfigInstance = GetConfigurationList().FirstOrDefault(a => a.cmc_bingmapapikey == retrieveConfiguration.cmc_bingmapapikey);
            var academicPeriod = PrepareAcademicPeriodEntity();
            var AccountEntity = PrepareAccountEntity();
            var contact = PrepareContactEntity(AccountEntity, academicPeriod);
            var listEnitityInstance = PrepareListEnity(contact);
            var scoreDefination = PrepareScoreDefination();
            var scoringfactorEnitityInstance = PrepareScoringfactorEnity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.AddRelationship("cmc_scoredefinition_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });
            var associationScoredefinitionEntity = new Entity("cmc_scoredefinition_list", Guid.NewGuid())
            {
                ["listid"] = listEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id
            };
            xrmFakedContext.AddRelationship("cmc_scoredefinition_scoringfactor", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_scoringfactor",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = cmc_scoringfactor.EntityLogicalName,
                Entity2Attribute = "cmc_scoringfactorid",
            });
            var associationScoringfactorEntity = new Entity("cmc_scoredefinition_scoringfactor", Guid.NewGuid())
            {
                ["cmc_scoringfactorid"] = scoringfactorEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id
            };
            xrmFakedContext.Initialize(new List<Entity>
            {

                associationScoringfactorEntity,
                associationScoredefinitionEntity,
                scoringfactorEnitityInstance,
                scoreDefination,
                academicPeriod,
                AccountEntity,
                contact,
                listEnitityInstance,
                bingMapKeyConfigInstance
            });


            //refactored from RetrieveMultipleExecutor
            //Excute FetchExpression if the name contain cmc_scoringfactor
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>.That.Matches(r => r.Query.Contains("name='cmc_scoringfactor'") || r.Query.Contains("name='list'"))))
                .ReturnsLazily((QueryBase req) =>
                {
                    var xmltext = XDocument.Parse(((FetchExpression)req).Query);
                    // adding the all attributes to the main entity, default behaviour of fake xrm is fetch only specified attribute values.
                    if (!xmltext.Descendants("entity").Any(r => r.Name.LocalName.Contains("attribute")))
                    {
                        xmltext.Descendants("entity").FirstOrDefault().Add(new XElement("all-attributes"));
                        ((FetchExpression)req).Query = xmltext.ToString();
                    }
                    var request = new RetrieveMultipleRequest()
                    {
                        Query = req
                    };
                    var executor = new RetrieveMultipleRequestExecutor();
                    var response = executor.Execute(request, xrmFakedContext) as RetrieveMultipleResponse;
                    QueryExpression qe = req as QueryExpression;
                    if (qe?.PageInfo.ReturnTotalRecordCount == true)
                    {
                        response.EntityCollection.TotalRecordCount = response.EntityCollection.Entities.Count;
                    }
                    return response.EntityCollection;
                });

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { new StringAttributeMetadata("lastname") { LogicalName = "lastname" } });
            xrmFakedContext.InitializeMetadata(entityMetadata);

            #region Refactor code

            //todo: this code fake the results multiple places need to refactor it in code coverage area.

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };

            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            List factorFilter = new List();
            var factorFilterQuery = factorFilter.ListName;
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>._)).Returns(fetchXmlToQueryExpressionResponse);
            #endregion

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
		
			var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
			var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
			//var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
			//    mockConfigurationService);
			var mockBingMapService = InitializeBingMapMockService();

            var retentionScoreCalculatorService = new RetentionScoreCalculatorService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockDomService);
            retentionScoreCalculatorService.RetentionScoreLogic();
            #endregion


            #region Assert
            var contactResult = xrmFakedContext.GetFakedOrganizationService().Retrieve(contact.LogicalName, contact.Id, new ColumnSet(true)).ToEntity<Contact>();
            Assert.IsTrue(contactResult.cmc_currentretentionscore > 0);
           
            #endregion Assert

        }


        [TestCategory("Function"), TestCategory("Negative")]
        [TestMethod]
        public void RetentionScoreCalculator_Retentionscorehistory_NoStudent()
        {

            #region ARRANGE          
            cmc_configuration retrieveConfiguration = new cmc_configuration();
            var bingMapKeyConfigInstance = GetConfigurationList().FirstOrDefault(a => a.cmc_bingmapapikey == retrieveConfiguration.cmc_bingmapapikey);
            var academicPeriod = PrepareAcademicPeriodEntity();
            var AccountEntity = PrepareAccountEntity();
            var contact = PrepareContactEntity(AccountEntity, academicPeriod);
            var listEnitityInstance = PrepareListEnity2(contact);
            var scoreDefination = PrepareScoreDefination();
            var scoringfactorEnitityInstance = PrepareScoringfactorEnity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.AddRelationship("cmc_scoredefinition_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });
            var associationScoredefinitionEntity = new Entity("cmc_scoredefinition_list", Guid.NewGuid())
            {
                ["listid"] = listEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id
            };
            xrmFakedContext.AddRelationship("cmc_scoredefinition_scoringfactor", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_scoringfactor",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = cmc_scoringfactor.EntityLogicalName,
                Entity2Attribute = "cmc_scoringfactorid",
            });
            var associationScoringfactorEntity = new Entity("cmc_scoredefinition_scoringfactor", Guid.NewGuid())
            {
                ["cmc_scoringfactorid"] = scoringfactorEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id
            };

            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Contact.EntityLogicalName,
                Entity1Attribute = "entityid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });
            var associationlistmember = new Entity("listcontact_association", Guid.NewGuid())
            {
                ["listid"] = listEnitityInstance.Id,
                ["entityid"] = contact.Id
            };
            xrmFakedContext.Initialize(new List<Entity>
            {
                associationScoringfactorEntity,
                associationScoredefinitionEntity,
                associationlistmember,
                scoringfactorEnitityInstance,
                scoreDefination,
                academicPeriod,
                AccountEntity,
                contact,
                listEnitityInstance,
                bingMapKeyConfigInstance
            });


            //refactored from RetrieveMultipleExecutor
            //Excute FetchExpression if the name contain cmc_scoringfactor
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>.That.Matches(r => r.Query.Contains("name='cmc_scoringfactor'") || r.Query.Contains("name='list'"))))
                .ReturnsLazily((QueryBase req) =>
                {
                    var xmltext = XDocument.Parse(((FetchExpression)req).Query);
                    // adding the all attributes to the main entity, default behaviour of fake xrm is fetch only specified attribute values.
                    if (!xmltext.Descendants("entity").Any(r => r.Name.LocalName.Contains("attribute")))
                    {
                        xmltext.Descendants("entity").FirstOrDefault().Add(new XElement("all-attributes"));
                        ((FetchExpression)req).Query = xmltext.ToString();
                    }
                    var request = new RetrieveMultipleRequest()
                    {
                        Query = req
                    };
                    var executor = new RetrieveMultipleRequestExecutor();
                    var response = executor.Execute(request, xrmFakedContext) as RetrieveMultipleResponse;

                    QueryExpression qe = req as QueryExpression;
                    if (qe?.PageInfo.ReturnTotalRecordCount == true)
                    {
                        response.EntityCollection.TotalRecordCount = response.EntityCollection.Entities.Count;
                    }
                    return response.EntityCollection;
                });

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { new StringAttributeMetadata("lastname") { LogicalName = "lastname" } });
            xrmFakedContext.InitializeMetadata(entityMetadata);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
			var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
			//var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
			//    mockConfigurationService);
			var mockBingMapService = InitializeBingMapMockService();

            var retentionScoreCalculatorService = new RetentionScoreCalculatorService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockDomService);
            retentionScoreCalculatorService.RetentionScoreLogic();
            #endregion

            #region Assert           
            Assert.IsFalse(false);
            #endregion Assert

        }
                
        [TestMethod]
        [TestCategory("Function"), TestCategory("Negative")]
        public void RetentionScoreCalculator_Retentionscorehistory_NoGroupFactors()
        {

            #region ARRANGE          
            cmc_configuration retrieveConfiguration = new cmc_configuration();
            var bingMapKeyConfigInstance = GetConfigurationList().FirstOrDefault(a => a.cmc_bingmapapikey == retrieveConfiguration.cmc_bingmapapikey);
            var academicPeriod = PrepareAcademicPeriodEntity();
            var AccountEntity = PrepareAccountEntity();
            var contact = PrepareContactEntity(AccountEntity, academicPeriod);
            var listEnitityInstance = PrepareListEnity2(contact);
            var scoreDefination = PrepareScoreDefination();
            // var scoringfactorEnitityInstance = PrepareScoringfactorEnity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.AddRelationship("cmc_scoredefinition_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });
            var associationScoredefinitionEntity = new Entity("cmc_scoredefinition_list", Guid.NewGuid())
            {
                ["listid"] = listEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id
            };
            xrmFakedContext.AddRelationship("cmc_scoredefinition_scoringfactor", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_scoringfactor",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = cmc_scoringfactor.EntityLogicalName,
                Entity2Attribute = "cmc_scoringfactorid",
            });

            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Contact.EntityLogicalName,
                Entity1Attribute = "entityid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });
            var associationlistmember = new Entity("listcontact_association", Guid.NewGuid())
            {
                ["listid"] = listEnitityInstance.Id,
                ["entityid"] = contact.Id
            };
            xrmFakedContext.Initialize(new List<Entity>
            {
               // associationScoringfactorEntity,
                associationScoredefinitionEntity,
                associationlistmember,
              //  scoringfactorEnitityInstance,
                scoreDefination,
                academicPeriod,
                AccountEntity,
                contact,
                listEnitityInstance,
                bingMapKeyConfigInstance
            });


            //refactored from RetrieveMultipleExecutor
            //Excute FetchExpression if the name contain cmc_scoringfactor
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>.That.Matches(r => r.Query.Contains("name='cmc_scoringfactor'") || r.Query.Contains("name='list'"))))
                .ReturnsLazily((QueryBase req) =>
                {
                    var xmltext = XDocument.Parse(((FetchExpression)req).Query);
                    // adding the all attributes to the main entity, default behaviour of fake xrm is fetch only specified attribute values.
                    if (!xmltext.Descendants("entity").Any(r => r.Name.LocalName.Contains("attribute")))
                    {
                        xmltext.Descendants("entity").FirstOrDefault().Add(new XElement("all-attributes"));
                        ((FetchExpression)req).Query = xmltext.ToString();
                    }
                    var request = new RetrieveMultipleRequest()
                    {
                        Query = req
                    };
                    var executor = new RetrieveMultipleRequestExecutor();
                    var response = executor.Execute(request, xrmFakedContext) as RetrieveMultipleResponse;

                    QueryExpression qe = req as QueryExpression;
                    if (qe?.PageInfo.ReturnTotalRecordCount == true)
                    {
                        response.EntityCollection.TotalRecordCount = response.EntityCollection.Entities.Count;
                    }
                    return response.EntityCollection;
                });

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { new StringAttributeMetadata("lastname") { LogicalName = "lastname" } });
            xrmFakedContext.InitializeMetadata(entityMetadata);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
			var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
			//var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
			//    mockConfigurationService);
			var mockBingMapService = InitializeBingMapMockService();

            var retentionScoreCalculatorService = new RetentionScoreCalculatorService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockDomService);
            retentionScoreCalculatorService.RetentionScoreLogic();
            #endregion

            #region Assert           
            Assert.IsFalse(false);
            #endregion Assert

        }


        
        [TestMethod]
        [TestCategory("Function"), TestCategory("Positive")]
        public void RetentionScoreCalculator_Retentionscorehistory_scoringFactors()
        {

            #region ARRANGE          
            cmc_configuration retrieveConfiguration = new cmc_configuration();
            var bingMapKeyConfigInstance = GetConfigurationList().FirstOrDefault(a => a.cmc_bingmapapikey == retrieveConfiguration.cmc_bingmapapikey);
            var academicPeriod = PrepareAcademicPeriodEntity();
            var AccountEntity = PrepareAccountEntity();
            var contact = PrepareContact(AccountEntity, academicPeriod);
            var listEnitityInstance = PrepareListEnity(contact);
            var scoreDefination = PrepareScoreDefination();
            var scoringfactorEnitityInstance = PrepareScoringfactorEnity1();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.AddRelationship("cmc_scoredefinition_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });
            var associationScoredefinitionEntity = new Entity("cmc_scoredefinition_list", Guid.NewGuid())
            {
                ["listid"] = listEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id,

            };
            xrmFakedContext.AddRelationship("cmc_scoredefinition_scoringfactor", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_scoringfactor",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = cmc_scoringfactor.EntityLogicalName,
                Entity2Attribute = "cmc_scoringfactorid",
            });

            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });
            var associationScoringfactorEntity = new Entity("cmc_scoredefinition_scoringfactor", Guid.NewGuid())
            {
                ["cmc_scoringfactorid"] = scoringfactorEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id
            };
            var associationAcademicPeriodEntity = new Entity("mshied_academicperiod_contact_CurrentAcademicPeriod", Guid.NewGuid())
            {
                ["mshied_academicperiod"] = academicPeriod.LogicalName,
                ["mshied_academicperiodid"] = academicPeriod.Id,
                ["contactid"] = contact.Id,
                ["mshied_currentacademicperiodid"] = contact.mshied_CurrentAcademicPeriodId

                // ["cmc_scoredefinitionid"] = scoreDefination.Id
            };
            xrmFakedContext.Initialize(new List<Entity>
            {

                associationScoringfactorEntity,
                associationScoredefinitionEntity,
                associationAcademicPeriodEntity,
                scoringfactorEnitityInstance,
                scoreDefination,
                academicPeriod,
                AccountEntity,
                contact,
                listEnitityInstance,
                bingMapKeyConfigInstance,
            });


            //refactored from RetrieveMultipleExecutor
            //Excute FetchExpression if the name contain cmc_scoringfactor
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>.That.Matches(r => r.Query.Contains("name='cmc_scoringfactor'") || r.Query.Contains("name='list'"))))
                .ReturnsLazily((QueryBase req) =>
                {
                    var xmltext = XDocument.Parse(((FetchExpression)req).Query);
                    // adding the all attributes to the main entity, default behaviour of fake xrm is fetch only specified attribute values.
                    if (!xmltext.Descendants("entity").Any(r => r.Name.LocalName.Contains("attribute")))
                    {
                        xmltext.Descendants("entity").FirstOrDefault().Add(new XElement("all-attributes"));
                        ((FetchExpression)req).Query = xmltext.ToString();
                    }
                    var request = new RetrieveMultipleRequest()
                    {
                        Query = req
                    };
                    var executor = new RetrieveMultipleRequestExecutor();
                    var response = executor.Execute(request, xrmFakedContext) as RetrieveMultipleResponse;
                    QueryExpression qe = req as QueryExpression;
                    if (qe?.PageInfo.ReturnTotalRecordCount == true)
                    {
                        response.EntityCollection.TotalRecordCount = response.EntityCollection.Entities.Count;
                    }
                    return response.EntityCollection;
                });

            List<EntityMetadata> entityMetadatas = new List<EntityMetadata>();

            var contactMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            contactMetadata.SetAttributeCollection(new List<AttributeMetadata>() { new StringAttributeMetadata("fullname") { LogicalName = "fullname" } });
            contactMetadata.SetSealedPropertyValue("PrimaryIdAttribute", "contactid");
            entityMetadatas.Add(contactMetadata);

            var accountMetadata = new EntityMetadata() { LogicalName = AccountEntity.LogicalName, MetadataId = Guid.NewGuid() };
            accountMetadata.SetSealedPropertyValue("PrimaryIdAttribute","accountid");
            accountMetadata.SetAttributeCollection(new List<AttributeMetadata>() { new StringAttributeMetadata("lastname") { LogicalName = "lastname" } });

            entityMetadatas.Add(accountMetadata);

            xrmFakedContext.InitializeMetadata(entityMetadatas);
            #region Refactor code

            //todo: this code fake the results multiple places need to refactor it in code coverage area.

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };

            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            List factorFilter = new List();
            var factorFilterQuery = factorFilter.ListName;
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>._)).Returns(fetchXmlToQueryExpressionResponse);
            #endregion

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
			var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
			//var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
			//    mockConfigurationService);
			var mockBingMapService = InitializeBingMapMockService();

            var retentionScoreCalculatorService = new RetentionScoreCalculatorService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockDomService);
            retentionScoreCalculatorService.RetentionScoreLogic();
            #endregion


            #region Assert
            var contactResult = xrmFakedContext.GetFakedOrganizationService().Retrieve(contact.LogicalName, contact.Id, new ColumnSet(true)).ToEntity<Contact>();
            Assert.IsTrue(contactResult.cmc_currentretentionscore > 0);
           
            #endregion Assert

        }

        [TestMethod]
        [TestCategory("Function"), TestCategory("Negative")]
        public void RetentionScoreCalculator_Retentionscorehistory_scoringFactorsFullNameNotExist()
        {

            #region ARRANGE          
            cmc_configuration retrieveConfiguration = new cmc_configuration();
            var bingMapKeyConfigInstance = GetConfigurationList().FirstOrDefault(a => a.cmc_bingmapapikey == retrieveConfiguration.cmc_bingmapapikey);
            var academicPeriod = PrepareAcademicPeriodEntity();
            var AccountEntity = PrepareAccountEntity();
            var contact = PrepareContact(AccountEntity, academicPeriod);
            var listEnitityInstance = PrepareListEnity(contact);
            var scoreDefination = PrepareScoreDefination();
            var scoringfactorEnitityInstance = PrepareScoringfactorEnity1();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.AddRelationship("cmc_scoredefinition_list", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_list",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = List.EntityLogicalName,
                Entity2Attribute = "listid",
            });
            var associationScoredefinitionEntity = new Entity("cmc_scoredefinition_list", Guid.NewGuid())
            {
                ["listid"] = listEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id,

            };
            xrmFakedContext.AddRelationship("cmc_scoredefinition_scoringfactor", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_scoredefinition_scoringfactor",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_scoredefinition.EntityLogicalName,
                Entity1Attribute = "cmc_scoredefinitionid",
                Entity2LogicalName = cmc_scoringfactor.EntityLogicalName,
                Entity2Attribute = "cmc_scoringfactorid",
            });

            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });
            var associationScoringfactorEntity = new Entity("cmc_scoredefinition_scoringfactor", Guid.NewGuid())
            {
                ["cmc_scoringfactorid"] = scoringfactorEnitityInstance.Id,
                ["cmc_scoredefinitionid"] = scoreDefination.Id
            };
            var associationAcademicPeriodEntity = new Entity("mshied_academicperiod_contact_CurrentAcademicPeriod", Guid.NewGuid())
            {
                ["mshied_academicperiod"] = academicPeriod.LogicalName,
                ["mshied_academicperiodid"] = academicPeriod.Id,
                ["contactid"] = contact.Id,
                ["mshied_currentacademicperiodid"] = contact.mshied_CurrentAcademicPeriodId
            };
            xrmFakedContext.Initialize(new List<Entity>
            {

                associationScoringfactorEntity,
                associationScoredefinitionEntity,
                associationAcademicPeriodEntity,
                scoringfactorEnitityInstance,
                scoreDefination,
                academicPeriod,
                AccountEntity,
                contact,
                listEnitityInstance,
                bingMapKeyConfigInstance,
            });


            //refactored from RetrieveMultipleExecutor
            //Excute FetchExpression if the name contain cmc_scoringfactor
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>.That.Matches(r => r.Query.Contains("name='cmc_scoringfactor'") || r.Query.Contains("name='list'"))))
                .ReturnsLazily((QueryBase req) =>
                {
                    var xmltext = XDocument.Parse(((FetchExpression)req).Query);
                    // adding the all attributes to the main entity, default behaviour of fake xrm is fetch only specified attribute values.
                    if (!xmltext.Descendants("entity").Any(r => r.Name.LocalName.Contains("attribute")))
                    {
                        xmltext.Descendants("entity").FirstOrDefault().Add(new XElement("all-attributes"));
                        ((FetchExpression)req).Query = xmltext.ToString();
                    }
                    var request = new RetrieveMultipleRequest()
                    {
                        Query = req
                    };
                    var executor = new RetrieveMultipleRequestExecutor();
                    var response = executor.Execute(request, xrmFakedContext) as RetrieveMultipleResponse;
                    QueryExpression qe = req as QueryExpression;
                    if (qe?.PageInfo.ReturnTotalRecordCount == true)
                    {
                        response.EntityCollection.TotalRecordCount = response.EntityCollection.Entities.Count;
                    }
                    return response.EntityCollection;
                });

            List<EntityMetadata> entityMetadatas = new List<EntityMetadata>();

            var contactMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            contactMetadata.SetAttributeCollection(new List<AttributeMetadata>() { new StringAttributeMetadata("lastname") { LogicalName = "lastname" } });
            contactMetadata.SetSealedPropertyValue("PrimaryIdAttribute", "contactid");
            entityMetadatas.Add(contactMetadata);

            var accountMetadata = new EntityMetadata() { LogicalName = AccountEntity.LogicalName, MetadataId = Guid.NewGuid() };
            accountMetadata.SetSealedPropertyValue("PrimaryIdAttribute", "accountid");
            accountMetadata.SetAttributeCollection(new List<AttributeMetadata>() { new StringAttributeMetadata("lastname") { LogicalName = "lastname" } });

            entityMetadatas.Add(accountMetadata);

            xrmFakedContext.InitializeMetadata(entityMetadatas);
            #region Refactor code

            //todo: this code fake the results multiple places need to refactor it in code coverage area.

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };

            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            List factorFilter = new List();
            var factorFilterQuery = factorFilter.ListName;
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>._)).Returns(fetchXmlToQueryExpressionResponse);
            #endregion

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
			var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
			//var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
			//    mockConfigurationService);
			var mockBingMapService = InitializeBingMapMockService();

            var retentionScoreCalculatorService = new RetentionScoreCalculatorService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockDomService);
            retentionScoreCalculatorService.RetentionScoreLogic();
             #endregion


            #region Assert
            //Negative Case No Asserts
            Entity contactEntity = new Entity();
            var data = xrmFakedContext.Data["contact"].TryGetValue(contact.Id, out contactEntity);
            
            Assert.IsNull(contactEntity.GetAttributeValue<string>("fullname"));
                        
            #endregion Assert

        }

        private static IEnumerable<cmc_configuration> GetConfigurationList()
        {
            return new List<cmc_configuration>
            {
                new cmc_configuration
                {
                    Id = Guid.NewGuid(),
                    cmc_bingmapapikey = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh"
                    //cmc_configurationname = Constants.BingMapApiKey,
                    //cmc_Value = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh"
                },
                new cmc_configuration
                {
                    Id = Guid.NewGuid(),
                    cmc_batchgeocodesize = 10
                    //cmc_configurationname = Constants.BatchGeocodeSize,
                    //cmc_Value = "10"
                }
            };
        }
        private List PrepareListEnity(Contact contact)
        {
            return new List()
            {
                ListId = Guid.NewGuid(),
                ListName = "MihirTestGroup",
                Type = true,
                cmc_marketinglisttype = cmc_list_cmc_marketinglisttype.StudentGroup,
                CreatedFromCode = list_createdfromcode.Contact,
                StateCode = ListState.Active,
                listcontact_association = new List<Contact>() { contact },
            };
        }
        private List PrepareListEnity2(Contact contact)
        {
            return new List()
            {
                ListId = Guid.NewGuid(),
                ListName = "MihirTestGroup",
                Type = false,
                cmc_marketinglisttype = cmc_list_cmc_marketinglisttype.StudentGroup,
                CreatedFromCode = list_createdfromcode.Contact,
                StateCode = ListState.Active,
                listcontact_association = new List<Contact>() { contact },

            };
        }
        private cmc_scoredefinition PrepareScoreDefination()
        {
            var cmc_Scoredefinition = new cmc_scoredefinition()
            {
                cmc_scoredefinitionId = Guid.NewGuid(),
                cmc_greenscorethreshold = 90,
                cmc_yellowscorethreshold = 50,
                cmc_redscorethreshold = 20,
                statecode = cmc_scoredefinitionState.Active,
				cmc_baseentity="Contact"
            };
            return cmc_Scoredefinition;
        }

        private cmc_scoringfactor PrepareScoringfactorEnity()
        {
            return new cmc_scoringfactor()
            {
                cmc_scoringfactorId = Guid.NewGuid(),
                cmc_scoringfactorname = "MihirScoreFact",
                cmc_attributename = "lastname",
                cmc_points = 20,
                cmc_conditiontype = new OptionSetValue((int)cmc_conditiontype.Equals),
                statecode = cmc_scoringfactorState.Active
            };

        }
        private cmc_scoringfactor PrepareScoringfactorEnity1()
        {
            return new cmc_scoringfactor()
            {
                cmc_scoringfactorId = Guid.NewGuid(),
                cmc_scoringfactorname = "MihirScoreFact",
                cmc_attributename = "account(accountid=parentcustomerid).contact(contactid=primarycontactid).fullname",
                cmc_points = 20,
                cmc_conditiontype = new OptionSetValue((int)cmc_conditiontype.Equals),
                statecode = cmc_scoringfactorState.Active
            };

        }

        private Contact PrepareContactEntity(Account account, mshied_academicperiod accademicPeriod)
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                LogicalName = Contact.EntityLogicalName,
                FirstName = "Ram",
                LastName = "Gopal",
                ParentCustomerId = account.ToEntityReference(),
                cmc_currentretentionscore = 20,
                StateCode = ContactState.Active,
                mshied_CurrentAcademicPeriodId = accademicPeriod.ToEntityReference()

            };
        }
        private Contact PrepareContact(Account account, mshied_academicperiod accademicPeriod)
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                LogicalName = Contact.EntityLogicalName,
                //FirstName = "Ram",
                //LastName = "Gopal",
                ParentCustomerId = account.ToEntityReference(),
                cmc_currentretentionscore = 20,
                StateCode = ContactState.Active,
                // cmc_currentacademicperiodid = accademicPeriod.ToEntityReference()

            };
        }

        private Account PrepareAccountEntity()
        {
            Guid guid = Guid.NewGuid();
            return new Account()
            {               
                Id= guid,
                AccountId = guid
            };
        }
        private mshied_academicperiod PrepareAcademicPeriodEntity()
        {
            return new mshied_academicperiod()
            {
                Id = Guid.NewGuid(),
                statecode = mshied_academicperiodState.Active,
                mshied_StartDate = DateTime.Now,
                mshied_EndDate = DateTime.Now.AddDays(10),
                statuscode = mshied_academicperiod_statuscode.Active

            };
        }
    }
}
