using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Lifecycle.Messages;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Lifecycle
{
    
   

    public static class DomAssignmentCommonService
    {
        public static void ValidateAttributeFormat(string attribute, ILanguageService retrieveMultiLingualValues, string errorMessage,
            Func<string, InvalidPluginExecutionException> createErrorHelper = null)
        {
            var isValid = Regex.IsMatch(attribute, "^[^\\.]+(\\.[^\\.]+)+$");
            if (string.IsNullOrEmpty(attribute) || !isValid)
            {
                string error = retrieveMultiLingualValues.Get(errorMessage);
                if (createErrorHelper == null)
                {
                    throw new InvalidPluginExecutionException(error);
                }
                else
                {
                    throw createErrorHelper(error);
                }
            }
        }

        public static void ValidateEntityNameMatchesRunAssignmentFor(string entityName,
            int? runAssignmentForValue, ILanguageService retrieveMultiLingualValues,
            string errorKey, IOrganizationService orgService, ILogger tracer,
            Func<string, InvalidPluginExecutionException> createErrorHelper = null)
        {
            tracer.Trace($"Checking if Run Assignment For Entity {runAssignmentForValue} is for {entityName}");
            
            string validEntityName = RetrieveEntityNameForRunAssignmentForEntity(runAssignmentForValue);

            tracer.Trace($"Entity name should be {validEntityName}");
            if (validEntityName?.Equals(entityName, StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                tracer.Trace("Entity name is valid");
                return;
            }

            string error = string.Format(retrieveMultiLingualValues.Get(
                errorKey), validEntityName);

            if (createErrorHelper == null)
            {
                throw new InvalidPluginExecutionException(error);
            }
            else
            {
                throw createErrorHelper(error);
            }
        }

        public static string RetrieveEntityNameForRunAssignmentForEntity(int? runAssignmentForEntity)
        {
            switch (runAssignmentForEntity ?? -1)
            {
                case (int)cmc_runassignmentforentity.Account:
                    return Account.EntityLogicalName;
                case (int)cmc_runassignmentforentity.Contact:
                    return Contact.EntityLogicalName;
                case (int)cmc_runassignmentforentity.InboundInterest:
                    return Lead.EntityLogicalName;
                case (int)cmc_runassignmentforentity.Lifecycle:
                    return Opportunity.EntityLogicalName;
            }

            return null;
        }

        
        public static AttributeMetadata ValidateAttributeString(string attribute, EntityReference domMasterId, ILanguageService retrieveMultiLingualValues, ILogger tracer, IOrganizationService orgService, Func<string, InvalidPluginExecutionException> createErrorHelper = null)
        {
            tracer.Trace("Splitting Attribute.");

            if (attribute == null)
            {
                tracer.Trace("Attribute is empty.");
                return null;
            }

            var attributeParts = attribute.Split('.');
            var entityName = attributeParts[0];
            var currentEntityLogicalName = entityName;
            string attributePart = "";

            for (var i = 0; i < attributeParts.Length - 1; i++)
            {
                attributePart = attributeParts[i];

                if (i == 0)
                {
                    // For first element, validate entity name
                    ValidateEntityNameMatchesRunAssignmentForEntity(entityName,
                        domMasterId,
                        retrieveMultiLingualValues, tracer, orgService, createErrorHelper);
                }
                else
                {
                    // For any element in the middle, validate value is a relationship
                    currentEntityLogicalName = ValidateRelationshipName(currentEntityLogicalName, attributePart, retrieveMultiLingualValues, tracer, orgService, createErrorHelper);
                }
            }
            attributePart = attributeParts.LastOrDefault();
            return ValidateAttributeName(currentEntityLogicalName, attributePart, retrieveMultiLingualValues, tracer, orgService, createErrorHelper);
        }

        private static void ValidateEntityNameMatchesRunAssignmentForEntity(string entityName, EntityReference domMasterId,
            ILanguageService retrieveMultiLingualValues, ILogger tracer, IOrganizationService orgService,
            Func<string, InvalidPluginExecutionException> createErrorHelper)
        {
            tracer.Trace($"Validate entity {entityName} matches the Run Assignment For Entity Value.");

            int? runAssignmentForValue = null;
            if (domMasterId != null)
            {
                var domMaster = orgService.Retrieve<cmc_dommaster>(domMasterId, new ColumnSet("cmc_runassignmentforentity"));

                runAssignmentForValue = domMaster?.cmc_runassignmentforentity?.Value;
            }

            ValidateEntityNameMatchesRunAssignmentFor(entityName, runAssignmentForValue,
                retrieveMultiLingualValues, "Invalid_Entity_Name_Execution_Order_Attribute",
                orgService, tracer, createErrorHelper);
        }

        private static AttributeMetadata ValidateAttributeName(string entityName, string attributeName,
            ILanguageService retrieveMultiLingualValues, ILogger tracer, IOrganizationService orgService,
            Func<string, InvalidPluginExecutionException> createErrorHelper)
        {
            tracer.Trace($"Validate Attribute {attributeName} belongs to entity {entityName}");
            var attributeMetadata = (orgService.Execute(new RetrieveMetadataChangesRequest()
            {
                Query = new EntityQueryExpression()
                {
                    Criteria = new MetadataFilterExpression()
                    {
                        Conditions =
                        {
                            new MetadataConditionExpression("LogicalName",
                                MetadataConditionOperator.Equals, entityName.ToLower())
                        }
                    },
                    AttributeQuery = new AttributeQueryExpression()
                    {
                        Criteria = new MetadataFilterExpression()
                        {
                            Conditions =
                            {
                                new MetadataConditionExpression("LogicalName",
                                MetadataConditionOperator.Equals, attributeName.ToLower())
                            }
                        },
                        Properties = new MetadataPropertiesExpression("LogicalName","AttributeType","OptionSet", "Targets")
                    },
                    Properties = new MetadataPropertiesExpression("LogicalName", "Attributes")
                }
            }) as RetrieveMetadataChangesResponse)?.EntityMetadata.FirstOrDefault()?.Attributes.FirstOrDefault();

            if (attributeMetadata != null)
            {
                tracer.Trace($"Attribute {attributeName} is for entity {entityName}");
                return attributeMetadata;
            }

            var error = string.Format(retrieveMultiLingualValues.Get(
                "Invalid_Attribute_Execution_Order_Attribute"), attributeName, entityName);

            if (createErrorHelper == null)
            {
                throw new InvalidPluginExecutionException(error);
            }
            else
            {
                throw createErrorHelper(error);
            }
        }

        private static string ValidateRelationshipName(string entityName, string relationshipName,
            ILanguageService retrieveMultiLingualValues, ILogger tracer, IOrganizationService orgService,
            Func<string, InvalidPluginExecutionException> createErrorHelper)
        {
            tracer.Trace($"Validate Relationship {relationshipName} belongs to entity {entityName}");

            var relationships = GetEntityRelationshipForRelationshipName(entityName, relationshipName, retrieveMultiLingualValues, tracer, orgService, createErrorHelper);
            return relationships.LastOrDefault().TargetEntity;
        }

        private static List<EntityRelationship> GetEntityRelationshipForRelationshipName(string entityName, string relationshipName,
            ILanguageService retrieveMultiLingualValues, ILogger tracer, IOrganizationService orgService,
            Func<string, InvalidPluginExecutionException> createErrorHelper)
        {

            tracer.Trace($"Looking for relationship {relationshipName} on entity {entityName}");
            RelationshipMetadataBase relationship = null;
            try
            {
                relationship = (orgService.Execute(new RetrieveRelationshipRequest()
                {
                    Name = relationshipName
                }) as RetrieveRelationshipResponse)?.RelationshipMetadata;
            }
            catch (Exception e)
            {
                tracer.Trace($"Relationship {relationshipName} not found");
            }

            EntityRelationship entityRelationship = new EntityRelationship()
            {
                SourceEntity = entityName
            };

            EntityRelationship intersectRelationship = null;

            string entity1 = "";
            string entity2 = "";
            string entity1Attribute = "";
            string entity2Attribute = "";
            string referencedEntity = "";

            if (relationship?.RelationshipType == RelationshipType.OneToManyRelationship)
            {
                var oneToManyRelationship = (OneToManyRelationshipMetadata)relationship;
                entity1 = oneToManyRelationship.ReferencedEntity;
                entity2 = oneToManyRelationship.ReferencingEntity;
                entity1Attribute = oneToManyRelationship.ReferencedAttribute;
                entity2Attribute = oneToManyRelationship.ReferencingAttribute;

                // Self referencing relationships should use the Referencing attribute as source, so treat that as default
                if (entity2 == entityName)
                {
                    entityRelationship.TargetEntity = entity1;
                    entityRelationship.TargetAttribute = entity1Attribute;
                    entityRelationship.SourceAttribute = entity2Attribute;
                }
                else if (entity1 == entityName)
                {
                    entityRelationship.TargetEntity = entity2;
                    entityRelationship.TargetAttribute = entity2Attribute;
                    entityRelationship.SourceAttribute = entity1Attribute;
                }
            }
            else if (relationship?.RelationshipType == RelationshipType.ManyToManyRelationship)
            {
                var manyToManyRelationship = (ManyToManyRelationshipMetadata)relationship;
                entity1 = manyToManyRelationship.Entity1LogicalName;
                entity2 = manyToManyRelationship.Entity2LogicalName;
                entity1Attribute = MetadataHelper.GetEntityPrimaryKey(orgService, tracer, entity1);
                entity2Attribute = MetadataHelper.GetEntityPrimaryKey(orgService, tracer, entity2);

                intersectRelationship = new EntityRelationship();

                if (entity1 == entityName)
                {
                    entityRelationship.TargetEntity = manyToManyRelationship.IntersectEntityName;
                    entityRelationship.TargetAttribute = manyToManyRelationship.Entity1IntersectAttribute;
                    entityRelationship.SourceAttribute = entity1Attribute;

                    intersectRelationship.TargetEntity = entity2;
                    intersectRelationship.TargetAttribute = entity2Attribute;
                    intersectRelationship.SourceEntity = manyToManyRelationship.IntersectEntityName;
                    intersectRelationship.SourceAttribute = manyToManyRelationship.Entity2IntersectAttribute;

                    // CRM returns invalid metadata for the accountleads relationship, so flip the attributes.
                    if (manyToManyRelationship.IntersectEntityName == "accountleads")
                    {
                        if ((entity1 == Account.EntityLogicalName 
                            && manyToManyRelationship.Entity1IntersectAttribute == "leadid"
                            && manyToManyRelationship.Entity2IntersectAttribute == "accountid")
                            ||
                            (entity1 == Lead.EntityLogicalName
                            && manyToManyRelationship.Entity1IntersectAttribute == "accountid"
                            && manyToManyRelationship.Entity2IntersectAttribute == "leadid"))
                        {
                            entityRelationship.TargetAttribute = manyToManyRelationship.Entity2IntersectAttribute;
                            intersectRelationship.SourceAttribute = manyToManyRelationship.Entity1IntersectAttribute;
                        }
                    }
                }
                else if (entity2 == entityName)
                {
                    entityRelationship.TargetEntity = manyToManyRelationship.IntersectEntityName;
                    entityRelationship.TargetAttribute = manyToManyRelationship.Entity2IntersectAttribute;
                    entityRelationship.SourceAttribute = entity2Attribute;

                    intersectRelationship.TargetEntity = entity1;
                    intersectRelationship.TargetAttribute = entity1Attribute;
                    intersectRelationship.SourceEntity = manyToManyRelationship.IntersectEntityName;
                    intersectRelationship.SourceAttribute = manyToManyRelationship.Entity1IntersectAttribute;

                    // CRM returns invalid metadata for the accountleads relationship, so flip the attributes.
                    if (manyToManyRelationship.IntersectEntityName == "accountleads")
                    {
                        if ((entity2 == Account.EntityLogicalName
                            && manyToManyRelationship.Entity1IntersectAttribute == "accountid"
                            && manyToManyRelationship.Entity2IntersectAttribute == "leadid")
                            ||
                            (entity2 == Lead.EntityLogicalName
                            && manyToManyRelationship.Entity1IntersectAttribute == "leadid"
                            && manyToManyRelationship.Entity2IntersectAttribute == "accountid"))
                        {
                            entityRelationship.TargetAttribute = manyToManyRelationship.Entity1IntersectAttribute;
                            intersectRelationship.SourceAttribute = manyToManyRelationship.Entity2IntersectAttribute;
                        }
                    }
                }
            }
            else
            {
                string error = string.Format(retrieveMultiLingualValues.Get(
                    "Invalid_Relationship_Execution_Order_Attribute"), relationshipName, entityName);

                if (createErrorHelper == null)
                {
                    throw new InvalidPluginExecutionException(error);
                }
                else
                {
                    throw createErrorHelper(error);
                }
            }

            tracer.Trace($"Relationship {relationshipName} is for entity {entityName} referencing entity {referencedEntity}");
            if (intersectRelationship == null)
            {
                return new List<EntityRelationship>() { entityRelationship };

            } else
            {
                return new List<EntityRelationship>() { entityRelationship, intersectRelationship };
            }
        }

        private static List<EntityRelationship> GetEntityHierarchyForRelationshipChain(string baseEntity, List<string> relationshipChain,
            ILanguageService retrieveMultiLingualValues, ILogger tracer, IOrganizationService orgService,
            Func<string, InvalidPluginExecutionException> createErrorHelper)
        {
            List<EntityRelationship> entityChain = new List<EntityRelationship>();
            var currentEntity = baseEntity;
            foreach (var relationship in relationshipChain)
            {
                var relationships = GetEntityRelationshipForRelationshipName(currentEntity, relationship, retrieveMultiLingualValues, tracer, orgService, createErrorHelper);
                currentEntity = relationships.LastOrDefault().TargetEntity;
                entityChain = entityChain.Concat(relationships).ToList();
            }

            return entityChain;
        }

        public static List<EntityRelationship> GetEntityChainForAttributeSchemaString(string attributeString,
            ILanguageService retrieveMultiLingualValues, ILogger tracer, IOrganizationService orgService,
            Func<string, InvalidPluginExecutionException> createErrorHelper = null)
        {
            var attributeParts = attributeString.Split('.');
            var entityName = attributeParts[0];
            var relationshipChain = attributeParts.Skip(1).Take(attributeParts.Length - 2).ToList();
            return GetEntityHierarchyForRelationshipChain(entityName, relationshipChain, retrieveMultiLingualValues, tracer, orgService, createErrorHelper);
        }
    }
}
