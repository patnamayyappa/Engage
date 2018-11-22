using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Moq;
using Newtonsoft.Json;

namespace Cmc.Engage.Common.Tests.CustomAttributePickerUI.Plugin
{
    [TestClass]
    public class BuildAttributeDisplayTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void BuildAttributeDisplay_RetrieveLocalizedAttributeNamesToDisplay()
        {
            #region ARRANGE

            var entity = PreparingEntity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                entity
            });

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
            };

            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("contactid"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frcontactid",1036) }, LogicalName="contactid" },
                new StringAttributeMetadata("LastName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frLastName",1036) }, LogicalName="LastName" },
                new StringAttributeMetadata("FirstName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frFirstName",1036) }, LogicalName="FirstName" },
            });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, entity, Operation.cmc_buildattributedisplay);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();


            var response = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadata }
                        }
            };
            AddInputParameters(mockServiceProvider, "AttributeNames", "contact.LastName,contact.FirstName");//entity.KeyAttributes);

            var customAttributePickerUiService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            customAttributePickerUiService.RetrieveLocalizedAttributeNamesToDisplay(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var outputstring = Convert.ToString(mockPluginExecutionContext?.OutputParameters["AttributeDisplayNamesJson"]);
            var outputParameter = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(outputstring);
            Assert.AreEqual(outputParameter.FirstOrDefault(r => r.Key == "contact.LastName").Value, "frcontact > frLastName");

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void BuildAttributeDisplay_RetrieveLocalizedAttributeNamesToDisplay_OneToMany()
        {
            #region ARRANGE

            var entity = PreparingEntity();
            var academicEntity = PreparingAcademicEntity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                entity,
                academicEntity
            });

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
            };

            var entityMetadata2 = new EntityMetadata()
            {
                LogicalName = "mshied_academicperiod",               
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("fracademic", 1036) },
            };

            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });

            entityMetadata2.SetAttributeCollection(new List<AttributeMetadata>() {            
            new UniqueIdentifierAttributeMetadata("mshied_academicperiod") { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("mshied_academicperiodid", 1036) }, LogicalName = "mshied_academicperiodid" },
            });

            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("mshied_currentacademicperiodid") { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcmc_currentacademicperiodid", 1036) }, LogicalName = "mshied_currentacademicperiodid" },
            });
            
            List<EntityMetadata> entityMetadatas = new List<EntityMetadata>();
            entityMetadatas.Add(entityMetadata);
            entityMetadatas.Add(entityMetadata2);
            xrmFakedContext.InitializeMetadata(entityMetadatas);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, entity, Operation.cmc_buildattributedisplay);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();          
            AddInputParameters(mockServiceProvider, "AttributeNames", "contact.mshied_academicperiod_contact_CurrentAcademicPeriod.mshied_academicperiodid");//entity.KeyAttributes);
            var customAttributePickerUiService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            customAttributePickerUiService.RetrieveLocalizedAttributeNamesToDisplay(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var outputstring = Convert.ToString(mockPluginExecutionContext?.OutputParameters["AttributeDisplayNamesJson"]);
            var outputParameter = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(outputstring);
            Assert.AreEqual(outputParameter.FirstOrDefault(r => r.Key == "contact.mshied_academicperiod_contact_CurrentAcademicPeriod.mshied_academicperiodid").Value, "frcontact > frcmc_currentacademicperiodid > mshied_academicperiodid");
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void BuildAttributeDisplay_RetrieveLocalizedAttributeNamesToDisplay_ManyToOne()
        {
            #region ARRANGE

            var entity = PreparingEntity();
            var successEntity = PreparingSuccessPlanEntity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                entity,
                successEntity
            });

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
                DisplayCollectionName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
            };

            var entityMetadata2 = new EntityMetadata()
            {
                LogicalName = "cmc_successplan",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frsuccessplan", 1036) },
                DisplayCollectionName = new Label() { UserLocalizedLabel = new LocalizedLabel("frsuccessplan", 1036) },
            };
        
            xrmFakedContext.AddRelationship("cmc_contact_successplan_assignedtoid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "cmc_successplan",
                Entity1Attribute = "cmc_assignedtoid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            entityMetadata2.SetAttributeCollection(new List<AttributeMetadata>() {
            new UniqueIdentifierAttributeMetadata("cmc_assignedtoid") { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("cmc_assignedtoid", 1036) }, LogicalName = "cmc_assignedtoid" },
            new StringAttributeMetadata("cmc_successplanname"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frcmc_successplanname",1036) }, LogicalName="cmc_successplanname" },
            });

            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("contactid"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frcontactid",1036) }, LogicalName="contactid" },
            });

            List<EntityMetadata> entityMetadatas = new List<EntityMetadata>();
            entityMetadatas.Add(entityMetadata);
            entityMetadatas.Add(entityMetadata2);           
            xrmFakedContext.InitializeMetadata(entityMetadatas);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, entity, Operation.cmc_buildattributedisplay);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "AttributeNames", "contact.cmc_contact_successplan_assignedtoid.cmc_successplanname");//entity.KeyAttributes);
            var customAttributePickerUiService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            customAttributePickerUiService.RetrieveLocalizedAttributeNamesToDisplay(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var outputstring = Convert.ToString(mockPluginExecutionContext?.OutputParameters["AttributeDisplayNamesJson"]);
            var outputParameter = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(outputstring);
            Assert.AreEqual(outputParameter.FirstOrDefault(r => r.Key == "contact.cmc_contact_successplan_assignedtoid.cmc_successplanname").Value, "frcontact > frsuccessplan > frcmc_successplanname");
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void BuildAttributeDisplay_RetrieveLocalizedAttributeNamesToDisplay_ManyToMany()
        {
            #region ARRANGE

            var successEntity = PreparingSuccessPlanAssignment();
            var ListEntity = PreparingList();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                successEntity,
                ListEntity
            });

           var entityMetadata = new EntityMetadata()
            {
                LogicalName = "List",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frList", 1036) },
                DisplayCollectionName = new Label() { UserLocalizedLabel = new LocalizedLabel("frList", 1036) },
            };
         
            var entityMetadata2 = new EntityMetadata()
            {
                LogicalName = "cmc_successplanassignment",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frsuccessplanassingment", 1036) },
                DisplayCollectionName = new Label() { UserLocalizedLabel = new LocalizedLabel("frsuccessplanassingment", 1036) },
            };
          
            xrmFakedContext.AddRelationship("cmc_successplanassignment_list", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = "cmc_successplanassignment",
                Entity1Attribute = "cmc_successplanassignmentid",
                Entity2LogicalName = "list",
                Entity2Attribute = "listid",
                IntersectEntity = "cmc_successplanassignment_list"//"cmc_successplanassignmentid"
            });

            entityMetadata2.SetAttributeCollection(new List<AttributeMetadata>() {
            new UniqueIdentifierAttributeMetadata("cmc_successplanassignmentid") { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("cmc_successplanassignmentid", 1036) }, LogicalName = "cmc_successplanassignmentid" },
            new StringAttributeMetadata("cmc_successplanassignmentname"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frcmc_successplanassignmentname",1036) }, LogicalName="cmc_successplanassignmentname" },
            });

           entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("listid"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frlistid",1036) }, LogicalName="listid" },
                new StringAttributeMetadata("listname"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frlistname",1036) }, LogicalName="listname" },

            });

            List<EntityMetadata> entityMetadatas = new List<EntityMetadata>();
            entityMetadatas.Add(entityMetadata);
            entityMetadatas.Add(entityMetadata2);
            xrmFakedContext.InitializeMetadata(entityMetadatas);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, ListEntity, Operation.cmc_buildattributedisplay);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "AttributeNames", "List.cmc_successplanassignment_list.cmc_successplanassignmentname");
            var customAttributePickerUiService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            customAttributePickerUiService.RetrieveLocalizedAttributeNamesToDisplay(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var outputstring = Convert.ToString(mockPluginExecutionContext?.OutputParameters["AttributeDisplayNamesJson"]);
            var outputParameter = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(outputstring);
            Assert.AreEqual(outputParameter.FirstOrDefault(r => r.Key == "List.cmc_successplanassignment_list.cmc_successplanassignmentname").Value, "frList > frsuccessplanassingment > frcmc_successplanassignmentname");
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void BuildAttributeDisplay_RetrieveLocalizedAttributeNamesToDisplay_AttributeNames_IsNull()
        {
            #region ARRANGE

            var entity = PreparingEntity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                entity
            });

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
            };

            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("contactid"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frcontactid",1036) }, LogicalName="contactid" },
                new StringAttributeMetadata("LastName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frLastName",1036) }, LogicalName="LastName" },
                new StringAttributeMetadata("FirstName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frFirstName",1036) }, LogicalName="FirstName" },
            });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, entity, Operation.cmc_buildattributedisplay);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();


            var response = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadata }
                        }
            };
            AddInputParameters(mockServiceProvider, "AttributeNames", "");//entity.KeyAttributes);

            var customAttributePickerUiService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            customAttributePickerUiService.RetrieveLocalizedAttributeNamesToDisplay(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var outputstring = mockPluginExecutionContext?.OutputParameters["AttributeDisplayNamesJson"];
            //var outputParameter = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(outputstring);
            Assert.AreEqual(outputstring,"{}");

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void BuildAttributeDisplay_RetrieveLocalizedAttributeNamesToDisplay_LocalDisplayValue_IsNull()
        {
            #region ARRANGE

            var entity = PreparingEntity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                entity
            });

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
            };

            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("contactid"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frcontactid",1036) }, LogicalName="contactid" },
                new StringAttributeMetadata("LastName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frLastName",1036) }, LogicalName="LastName" },
                new StringAttributeMetadata("FirstName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frFirstName",1036) }, LogicalName="FirstName" },
            });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, entity, Operation.cmc_buildattributedisplay);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();


            var response = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadata }
                        }
            };
            AddInputParameters(mockServiceProvider, "AttributeNames", "contact. .");//entity.KeyAttributes);

            var customAttributePickerUiService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            customAttributePickerUiService.RetrieveLocalizedAttributeNamesToDisplay(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var outputstring = Convert.ToString(mockPluginExecutionContext?.OutputParameters["AttributeDisplayNamesJson"]);
            var outputParameter = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(outputstring);
            Assert.AreEqual(outputParameter.FirstOrDefault(r => r.Key == "contact. .").Value, "");

            #endregion
        }

        [ExpectedException(typeof(Exception))]
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void BuildAttributeDisplay_RetrieveLocalizedAttributeNamesToDisplay_AttributeNames_IsNotInContext()
        {
            #region ARRANGE

            var entity = PreparingEntity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                entity
            });

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact",
                DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
            };

            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("contactid"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frcontactid",1036) }, LogicalName="contactid" },
                new StringAttributeMetadata("LastName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frLastName",1036) }, LogicalName="LastName" },
                new StringAttributeMetadata("FirstName"){ DisplayName= new Label(){UserLocalizedLabel= new LocalizedLabel("frFirstName",1036) }, LogicalName="FirstName" },
            });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, entity, Operation.cmc_buildattributedisplay);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();


            var response = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadata }
                        }
            };
            AddInputParameters(mockServiceProvider, "AttributeNames", "DemoTestName");//entity.KeyAttributes);

            var customAttributePickerUiService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            customAttributePickerUiService.RetrieveLocalizedAttributeNamesToDisplay(mockExecutionContext.Object);

            #endregion

        }

        #region DataPrepartion
        private Models.Contact PreparingEntity()
        {
            var entity = new Models.Contact()
            {
                Id = Guid.NewGuid(),
                LastName = "Test Lastname",
                FirstName = "Test Firstname",
            };

            return entity;
        }

        private Models.List PreparingList()
        {
            var entity = new Models.List()
            {
                ListId= Guid.NewGuid(),
                ListName = "Test Lastname",
            };

            return entity;
        }
        private Models.cmc_successplan PreparingSuccessPlanEntity()
        {
            var success = new Models.cmc_successplan()
            {
                Id = Guid.NewGuid(),
                cmc_successplanname= "TestSuccessplanname"
            };
            return success;
        }

       
        private Models.cmc_successplanassignment PreparingSuccessPlanAssignment()
        {
            var success = new Models.cmc_successplanassignment()
            {
                Id = Guid.NewGuid(),
                cmc_successplanassignmentname = "TestSuccessPlanAssignment"
            };
            return success;
        }
        private mshied_academicperiod PreparingAcademicEntity()
        {
            var academic = new mshied_academicperiod()
            {
                Id = Guid.NewGuid()
               
            };
            return academic;
        }

        private Models.Contact PreparingContactEntity( )
        {
            var entity = new Models.Contact()
            {
                Id = Guid.NewGuid(),
                LastName = "Test Lastname",
                FirstName = "Test Firstname",
                mshied_academicperiod_contact_CurrentAcademicPeriod = new Models.mshied_academicperiod()
            };

            return entity;
        }

        #endregion

    }
}