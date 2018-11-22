using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models.Tests;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.InboundInterest.Activity
{
    [TestClass]
    public class RetrieveContactOpenLifecycleTest
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void OpenLifecycle_RetrieveContactStatusAttributeMetadata_Opportunity()
        {
            #region ARRANGE
            var opportunity = PrepareOpportunity();
            var filters = GetFilters(opportunity);
            var xrmFakedContext = new XrmFakedContext();
            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
               opportunity,

            });

            var userLocalizedLabel = new LocalizedLabel("microsoft.xrm.sdk.optionsetvalue", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };

            var statusAttributeMetadata = new StatusAttributeMetadata()
            {
                OptionSet = new OptionSetMetadata()
                {
                    Options = { new OptionMetadata( new Label(userLocalizedLabel,labels),null)

                    }
                }
            };



            var retrieveAttributeResponse = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection
                {
                    { "AttributeMetadata", statusAttributeMetadata}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributeResponse);

            #endregion ARRANGE
            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockLifecycleService = new LifecycleService(xrmFakedContext.GetFakedOrganizationService(), mockLogger.Object);

            var resultLifecycleService = mockLifecycleService.RetrieveContactOpenLifecycle(filters);
            #endregion  ACT

            #region ASSERT
            Assert.IsNotNull(resultLifecycleService.Id);
            #endregion ASERT
        }

        [TestMethod]
        [TestCategory("Activity"), TestCategory("Positive")]
        public void OpenLifecycle_RetrieveContactStateAttributeMetadata_Opportunity()
        {
            #region ARRANGE
            var opportunity = PrepareOpportunity();



            var filters = GetFilters(opportunity);



            var xrmFakedContext = new XrmFakedContext();
            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
               opportunity,

            });


            var userLocalizedLabel = new LocalizedLabel("microsoft.xrm.sdk.optionsetvalue", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };

            var stateAttributeMetadata = new StateAttributeMetadata()
            {
                OptionSet = new OptionSetMetadata()
                {
                    Options = { new OptionMetadata(new Label(userLocalizedLabel, labels), null) }
                }
            };



            var retrieveAttributeResponse = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection
                {
                    { "AttributeMetadata", stateAttributeMetadata}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributeResponse);

            #endregion ARRANGE
            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockLifecycleService = new LifecycleService(xrmFakedContext.GetFakedOrganizationService(), mockLogger.Object);

            var resultLifecycleService = mockLifecycleService.RetrieveContactOpenLifecycle(filters);
            #endregion  ACT

            #region ASSERT
            Assert.IsNotNull(resultLifecycleService.Id);
            #endregion ASERT
        }

        [TestMethod]
        [TestCategory("Activity"), TestCategory("Positive")]
        public void OpenLifecycle_RetrieveContactPicklistAttributeMetadata_Opportunity()
        {
            #region ARRANGE
            var opportunity = PrepareOpportunity();

            var filters = GetFilters(opportunity);

            var xrmFakedContext = new XrmFakedContext();
            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
               opportunity,

            });

            var userLocalizedLabel = new LocalizedLabel("microsoft.xrm.sdk.optionsetvalue", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };
            var picklistAttributeMetadata = new PicklistAttributeMetadata()
            {
                OptionSet = new OptionSetMetadata()
                {
                    Options = { new OptionMetadata(new Label(userLocalizedLabel, labels), null) }
                }
            };



            var retrieveAttributeResponse = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection
                {
                    { "AttributeMetadata", picklistAttributeMetadata}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributeResponse);

            #endregion ARRANGE
            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockLifecycleService = new LifecycleService(xrmFakedContext.GetFakedOrganizationService(), mockLogger.Object);

            var resultLifecycleService = mockLifecycleService.RetrieveContactOpenLifecycle(filters);
            #endregion  ACT

            #region ASSERT
            Assert.IsNotNull(resultLifecycleService.Id);
            #endregion ASERT
        }

        [TestMethod]
        [TestCategory("Activity"), TestCategory("Negative")]
        public void OpenLifecycle_RetrieveContact_Opportunity()
        {
            #region ARRANGE
            var opportunity = PrepareOpportunity();

            var filters = GetFilters(opportunity);

            var xrmFakedContext = new XrmFakedContext();
            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
               opportunity,

            });


            var picklistAttributeMetadata = new PicklistAttributeMetadata()
            {

                DisplayName = new Label()
                {
                    UserLocalizedLabel = new LocalizedLabel("microsoft.xrm.sdk.optionsetvalue", 175490002)
                },
                LogicalName = "firstname",
                OptionSet = new OptionSetMetadata()
                {
                    Options = { new OptionMetadata( new Label() {
                    UserLocalizedLabel = new LocalizedLabel("microsoft.xrm.sdk.optionsetvalue", 175490002) }, null) }
                }
            };



            var retrieveAttributeResponse = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection
                {
                    { "AttributeMetadata", picklistAttributeMetadata}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributeResponse);

            #endregion ARRANGE
            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockLifecycleService = new LifecycleService(xrmFakedContext.GetFakedOrganizationService(), mockLogger.Object);

            filters.LifecycleType = null;

            mockLifecycleService.RetrieveContactOpenLifecycle(filters);

            filters.LifecycleType = (new OptionSetValue(1)).ToString();
            mockLifecycleService.RetrieveContactOpenLifecycle(filters);
            
            #endregion  ACT

            #region ASSERT
            //Assert.IsNotNull(resultLifecycleService.Id);
            #endregion ASERT
        }

        [TestMethod]
        [TestCategory("Activity"), TestCategory("Negative")]
        public void OpenLifecycle_RetrieveContact_Opportunity_OptionSetNotNull()
        {
            #region ARRANGE
            var opportunity = PrepareOpportunity();

            var filters = GetFilters(opportunity);

            var xrmFakedContext = new XrmFakedContext();
            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
               opportunity,

            });
            

            var userLocalizedLabel = new LocalizedLabel("microsoft.xrm.sdk.optionsetvalue", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };

            var statusAttributeMetadata = new StatusAttributeMetadata()
            {
                OptionSet = new OptionSetMetadata()
                {
                    Options = { new OptionMetadata( new Label(userLocalizedLabel,labels),175490002)

                    }
                }
            };



            var retrieveAttributeResponse = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection
                {
                    { "AttributeMetadata", statusAttributeMetadata}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributeResponse);           
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>._)).Returns(new EntityCollection(new List<Entity> { opportunity }));

            #endregion ARRANGE
            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockLifecycleService = new LifecycleService(xrmFakedContext.GetFakedOrganizationService(), mockLogger.Object);
           
            mockLifecycleService.RetrieveContactOpenLifecycle(filters);

            filters.LifecycleType = (175490002).ToString();
            mockLifecycleService.RetrieveContactOpenLifecycle(filters);

            filters.DepositReceivedContainsData =false;
            mockLifecycleService.RetrieveContactOpenLifecycle(filters);
            
            #endregion  ACT

            #region ASSERT
            //Assert.IsNotNull(resultLifecycleService.Id);
            #endregion ASERT
        }

       

        private RetrieveContactOpenLifecycleFilters GetFilters(Entity opportunity)
        {
            return new RetrieveContactOpenLifecycleFilters()
            {
                CampusId = opportunity.GetAttributeValue<EntityReference>("parentaccountid"),
                CompleteAppReceivedContainsData = true,
                ContactId = opportunity.GetAttributeValue<EntityReference>("parentcontactid"),
                DepositReceivedContainsData = true,
                 ExpectedStartDateId = opportunity.GetAttributeValue<EntityReference>("cmc_expstartdateid"),
                LifecycleType = (new OptionSetValue(175490002)).ToString(),
                ProgramId = opportunity.GetAttributeValue<EntityReference>("cmc_prgmid"),
                ProgramLevelId = opportunity.GetAttributeValue<EntityReference>("cmc_prgmlevelid"),
                SourceCampaignId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcecampaignid"),
                SourceCampusId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcecampusid"),
                SourceCategoryId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcecategoryid"),
                SourceMethodId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcemethodid"),
               SourceProgramId = opportunity.GetAttributeValue<EntityReference>("cmc_sourceprgmid"),
                SourceProgramLevelId = opportunity.GetAttributeValue<EntityReference>("cmc_srcprgmlevelid"),
                SourceReferringContactId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcereferringcontactid"),
                SourceReferringOrganizationId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcereferringorganizationid"),
                SourceReferringStaffId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcereferringstaffid"),
                SourceSubCategoryId = opportunity.GetAttributeValue<EntityReference>("cmc_sourcesubcategoryid"),

            };
        }

        private Entity PrepareOpportunity()
        {
          
            return new Entity("opportunity", Guid.NewGuid())
            {
                ["name"] = "Test opportunity",
                ["statecode"] = "Open",
                ["parentaccountid"] = new EntityReference("Account", Guid.NewGuid()),
                ["cmc_completeappreceiveddate"] = DateTime.Now,
                ["parentcontactid"] = new EntityReference("Contact", Guid.NewGuid()),
                ["cmc_depositreceiveddate"] = DateTime.Now,
                ["cmc_expstartdateid"] = new EntityReference("mshied_academicperiod", Guid.NewGuid()),
                ["cmc_lifecycletype"] = new OptionSetValue(175490002),
                ["cmc_prgmid"] = new EntityReference("mshied_program", Guid.NewGuid()),
                ["cmc_prgmlevelid"] = new EntityReference("mshied_programlevel", Guid.NewGuid()),
                ["cmc_sourcecampusid"] = new EntityReference("Account", Guid.NewGuid()),
                ["cmc_sourcecategoryid"] = new EntityReference("cmc_sourcecategory", Guid.NewGuid()),
                ["cmc_sourcemethodid"] = new EntityReference("cmc_sourcemethod", Guid.NewGuid()),
                ["cmc_sourceprgmmid"] = new EntityReference("mshied_program", Guid.NewGuid()),
                ["cmc_srcprgmlevelid"] = new EntityReference("mshied_programlevel", Guid.NewGuid()),
                ["cmc_sourcereferringcontactid"] = new EntityReference("Contact", Guid.NewGuid()),
                ["cmc_sourcereferringorganizationid"] = new EntityReference("Account", Guid.NewGuid()),
                ["cmc_sourcereferringstaffid"] = new EntityReference("SystemUser", Guid.NewGuid()),
                ["cmc_sourcecampaignid"] = new EntityReference("campaign", Guid.NewGuid()),
                ["cmc_sourcesubcategoryid"] = new EntityReference("cmc_sourcesubcategory", Guid.NewGuid()),

            };
        }

    }
}
