using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Common.Tests.CustomAttributePickerUI.Plugin
{
	[TestClass]
	public class RetrieveAttributesRelationshipsForEntitiesTest : XrmUnitTestBase
	{
	    [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
		public void RetrieveAttributesRelationshipsForEntities()
		{
			#region ARRANGE
			var entity = PreparingEntity();
			var successplan = SuccessPlan();
			var xrmFakedContext = new XrmFakedContext();

			xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Models.Contact));

			xrmFakedContext.Initialize(new List<Entity>()
			{
				entity,
				successplan
			});

			RelationshipMetadataBase relationshipMetadataBase = new OneToManyRelationshipMetadata()
			{
				MetadataId = Guid.NewGuid(),
				ReferencedEntity = "contact",
				ReferencedAttribute = "contactid",
				ReferencingEntity = "cmc_successplan",
				ReferencingAttribute = "cmc_assignedtoid",
				SchemaName = "cmc_contact_successplan_assignedtoid"
			};

			var userSetting = new Entity("usersettings", Guid.NewGuid()) { Attributes = new AttributeCollection() { new KeyValuePair<string, object>("localeid", 1033) } };
			var entityCollection = new EntityCollection();
			entityCollection.Entities.Add(userSetting);

			var entityMetadata = new EntityMetadata()
			{
				LogicalName = "contact",
				DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frcontact", 1036) },
				SchemaName = "contact",
				MetadataId = (Guid)relationshipMetadataBase.MetadataId,

			};
			var oneToManyRelationships = new OneToManyRelationshipMetadata[1];
			oneToManyRelationships[0] = new OneToManyRelationshipMetadata()
			{
				ReferencedAttribute = "cmc_assignedtoid",
				ReferencingAttribute = "contactid",
				ReferencedEntity = "cmc_successplan",
				ReferencingEntity = "contact",
				SchemaName = "cmc_contact_successplan_assignedtoid"
			};
			var manyToOneRelationships = new OneToManyRelationshipMetadata[1];
			manyToOneRelationships[0] = new OneToManyRelationshipMetadata()
			{
				ReferencedAttribute = "cmc_assignedtoid",
				ReferencingAttribute = "contactid",
				ReferencedEntity = "cmc_successplan",
				ReferencingEntity = "contact",
				SchemaName = "cmc_contact_successplan_assignedtoid"
			};
			var manyToManyRelationships = new ManyToManyRelationshipMetadata[1];
			manyToManyRelationships[0] = new ManyToManyRelationshipMetadata()
			{
				Entity1LogicalName = "contact",
				Entity2LogicalName = "cmc_tripactivity",
				Entity1IntersectAttribute = "cmc_tripactivity_contact",
				Entity2IntersectAttribute = "cmc_tripactivity_contact",
				SchemaName = "cmc_tripactivity_contact"
			};
			entityMetadata.GetType().GetProperty("OneToManyRelationships").SetValue(entityMetadata, oneToManyRelationships, null);
			entityMetadata.GetType().GetProperty("ManyToOneRelationships").SetValue(entityMetadata, manyToOneRelationships, null);
			entityMetadata.GetType().GetProperty("ManyToManyRelationships").SetValue(entityMetadata, manyToManyRelationships, null);

			entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
				new UniqueIdentifierAttributeMetadata("contactid")
				{
					DisplayName = new Label()
					{
						UserLocalizedLabel = new LocalizedLabel("frcontactid",1036)
					},
					LogicalName ="contactid"
				},
				new StringAttributeMetadata("LastName")
				{
					DisplayName = new Label()
					{
						UserLocalizedLabel = new LocalizedLabel("frLastName",1036)
					},
					LogicalName ="LastName"
				},
				new StringAttributeMetadata("FirstName")
				{
					DisplayName = new Label()
					{
						UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036)
					},
					LogicalName ="FirstName"
				},
			});
			xrmFakedContext.AddRelationship("cmc_contact_successplan_assignedtoid", new XrmFakedRelationship()
			{
				RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
				Entity1Attribute = "contactid",
				Entity1LogicalName = "contact",
				Entity2Attribute = "cmc_assignedtoid",
				Entity2LogicalName = "cmc_successplan"
			});

			xrmFakedContext.SetEntityMetadata(entityMetadata);

			var mockServiceProvider = InitializeMockService(xrmFakedContext, entity, Operation.cmc_retrieveattributesandrelationshipsforentities);
			var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
			#endregion

			#region ACT
			var mockLogger = new Mock<ILogger>();
			var orgService = new Mock<IOrganizationService>();

			var response = new RetrieveEntityResponse()
			{
				Results = new ParameterCollection
						{
							{ "EntityMetadata", xrmFakedContext.GetEntityMetadataByName("contact") }
						}
			};
			var metaDataCollection = new EntityMetadataCollection();
			metaDataCollection.Add(xrmFakedContext.GetEntityMetadataByName("contact"));
			var RetrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
			{
				Results = new ParameterCollection
						{
							{ "EntityMetadata", metaDataCollection }
						}
			};
			orgService.SetupSequence(x => x.Execute(It.IsAny<OrganizationRequest>())).Returns(response).Returns(RetrieveMetadataChangesResponse);
			orgService.Setup(x => x.RetrieveMultiple(It.IsAny<FetchExpression>())).Returns(entityCollection);

			AddInputParameters(mockServiceProvider, "EntityLogicalNames", "contact");// "contact.ManyToMany.FirstName");

			var customAttributePickerUIService = new CustomAttributePickerUIService(mockLogger.Object, orgService.Object);

			customAttributePickerUIService.RetrieveAttributesRelationshipsForEntities(mockExecutionContext.Object);
            #endregion

            #region Assert

		    var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

		    var result = mockPluginExecutionContext.OutputParameters["AttributeJson"];

		    Assert.IsNotNull(result);

            #endregion

        }

        private Models.Contact PreparingEntity()
		{
			var entity = new Models.Contact()
			{
				Id = Guid.NewGuid(),
				LastName = "Test Lastname",
				FirstName = "Test"
			};
			return entity;
		}

		private cmc_successplan SuccessPlan()
		{
			var sp = new cmc_successplan()
			{
				Id = Guid.NewGuid(),
				cmc_successplanname = "Test success plan"
			};
			return sp;
		}
	}
}
