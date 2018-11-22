using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public class CustomAttributePickerUIService : ICustomAttributePickerUIService
    {
        private readonly IOrganizationService _orgService;
        private readonly ILogger _tracer;
        private const int _maxMetadataQueryFilters = 300;

        private Dictionary<string, string> _attributeNameDisplayNames = new Dictionary<string, string>();
        private Dictionary<string, EntityMetadata> _entityMetadata = new Dictionary<string, EntityMetadata>();
        private Dictionary<string, string> _attributeLocalLabels = new Dictionary<string, string>();
        private Dictionary<string, OneToManyRelationshipMetadata> _oneToManyRelationshipMetadata = new Dictionary<string, OneToManyRelationshipMetadata>();
        private Dictionary<string, ManyToManyRelationshipMetadata> _manyToManyRelationshipMetadata = new Dictionary<string, ManyToManyRelationshipMetadata>();

        private class RelatedEntityMetadata
        {
            public EntityMetadata Entity { get; set; }
            public string LocalizedRelationshipDisplayName { get; set; }
        }

        public CustomAttributePickerUIService(ILogger tracer, IOrganizationService orgService)
        {
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _tracer = tracer ?? throw new ArgumentException(nameof(tracer));
        }

        #region Build Attribute Display

        public void RetrieveLocalizedAttributeNamesToDisplay(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var userId = pluginContext.UserId as Guid?;

            var attributeNamesString = pluginContext.InputParameters["AttributeNames"] as string;
            _tracer.Trace(
                $"{nameof(CustomAttributePickerUIService)} : {nameof(RetrieveLocalizedAttributeNamesToDisplay)} => Attribute Name is {attributeNamesString}");

            if (string.IsNullOrWhiteSpace(attributeNamesString))
            {
                pluginContext.OutputParameters["AttributeDisplayNamesJson"] = "{}";
                return;
            }

            var attributeNamesList = attributeNamesString.Split(',').ToList();
            var result = GetLocalizedDisplayNames(attributeNamesList);
            var orderedResult = result.OrderBy(x => x.Value);

            pluginContext.OutputParameters["AttributeDisplayNamesJson"] = JsonConvert.SerializeObject(orderedResult);
        }

        private Dictionary<string, string> GetLocalizedDisplayNames(List<string> attributeNames)
        {
            var localizedAttributeDisplayNames = new Dictionary<string, string>();

            foreach(var attributeName in attributeNames)
            {
                var displayName = GetDisplayValue(attributeName);
                if (!localizedAttributeDisplayNames.ContainsKey(attributeName))
                {
                    localizedAttributeDisplayNames.Add(attributeName, displayName);
                }
            }
            // Return an object with each key being the passed in logical name and the value being the display name created by the plugin
            return localizedAttributeDisplayNames;
        }

        private string GetDisplayValue(string attributeName)
        {
            var localDisplayValue = RetrieveLocalizedDisplayName(attributeName) ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(localDisplayValue))
            {
                _tracer.Trace($"{nameof(CustomAttributePickerUIService)} : {nameof(GetDisplayValue)} => Unable to resolve all attributes for Attribute Name: {attributeName}, value associated with {attributeName} is empty.");
            }

            return localDisplayValue;
        }

        private string RetrieveLocalizedDisplayName(string attributeNameKey)
        {
            // Check the cache
            if (_attributeNameDisplayNames.ContainsKey(attributeNameKey))
            {
                _tracer.Trace($"Key: {attributeNameKey} =>  Value: {_attributeNameDisplayNames[attributeNameKey]}");
                return _attributeNameDisplayNames[attributeNameKey];
            }

            try
            {
                var localizedAttributes = RetrieveLocalizedDisplayNameHelper(attributeNameKey);
                var resultStr = localizedAttributes != null && localizedAttributes.Length > 0 
                    ? string.Join(" > ", localizedAttributes) : "";

                // Cache the result and return it
                _attributeNameDisplayNames.Add(attributeNameKey, resultStr);
                _tracer.Trace($"Key: {attributeNameKey} => Value: {resultStr}");
                return resultStr;
            }
            catch (Exception ex)
            {
                _tracer.Error($"Error retrieving localized logical display name: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        private string[] RetrieveLocalizedDisplayNameHelper(string key)
        {
            var schemaNames = key.Split('.');
            var lastEntityAddedToDictionary = GetEntityMetadata(schemaNames[0]);

            var localizedNames = new string[schemaNames.Length];
            localizedNames[0] = GetEntityLocalLabel(schemaNames[0]);
            _tracer.Trace($"Entity: {schemaNames[0]} => {localizedNames[0]}");

            if (localizedNames[0] != null && schemaNames.Length > 1)
            {
                string attribute;
                RelatedEntityMetadata relatedEntityMetadata;
                int lastIndex = schemaNames.Length - 1;

                for (int i = 1; i < lastIndex; i++)
                {
                    relatedEntityMetadata = GetRelationshipMetadata(lastEntityAddedToDictionary, schemaNames[i]);
                    if (relatedEntityMetadata == null)
                    {
                        _tracer.Trace($"Returning null; Unable to resolve {schemaNames[i]} as an attribute or relationship on entity {lastEntityAddedToDictionary.LogicalName}");
                        return null;
                    }

                    lastEntityAddedToDictionary = relatedEntityMetadata.Entity;
                    localizedNames[i] = relatedEntityMetadata.LocalizedRelationshipDisplayName;
                    _tracer.Trace($"Relationship: {schemaNames[i]} => {localizedNames[i]}");
                }

                // Last schema name should map to an attribute
                attribute = GetAttributeLocalLabelFromMetadata(lastEntityAddedToDictionary.LogicalName, schemaNames[lastIndex]);

                if (attribute == null)
                {
                    return null;
                }
                localizedNames[lastIndex] = attribute;
                _tracer.Trace($"Attribute: {schemaNames[lastIndex]} => {localizedNames[lastIndex]}");
            }
            return localizedNames;
        }

        #endregion

        #region Entity Metadata Helpers

        private EntityMetadata GetEntityMetadata(string entityKey)
        {
            // Check the cache
            if (_entityMetadata.ContainsKey(entityKey))
            {
                return _entityMetadata[entityKey];
            }

            var req = new RetrieveEntityRequest()
            {
                LogicalName = entityKey,
                RetrieveAsIfPublished = true,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes
            };

            try
            {
                var entityMetadataResponse = ((RetrieveEntityResponse)_orgService.Execute(req));
                var result = entityMetadataResponse?.EntityMetadata;

                // Cache the result and return it
                _entityMetadata.Add(entityKey, result);
                return result;
            }
            catch (Exception ex)
            {
                _tracer.Trace($"Returning null; Error retrieving entity metadata for {entityKey}: {ex.Message}");
                return null;
            }
        }

        private string GetEntityLocalizedPluralDisplayName(EntityMetadata entity)
        {
            return entity.DisplayCollectionName.UserLocalizedLabel?.Label
                ?? entity.DisplayCollectionName.LocalizedLabels.FirstOrDefault()?.Label;
        }

        private string GetEntityLocalLabel(string key)
        {
            return _entityMetadata[key].DisplayName.UserLocalizedLabel?.Label
                ?? _entityMetadata[key].DisplayName.LocalizedLabels.FirstOrDefault()?.Label;
        }

        #endregion

        #region Attribute Metadata Helpers

        private string GetAttributeLocalLabelFromMetadata(string entityLogicalName, string attributeKey)
        {
            // Check the cache, key handles attribute key duplicates across multiple entities
            var key = entityLogicalName + "." + attributeKey;
            if (_attributeLocalLabels.ContainsKey(key))
            {
                return _attributeLocalLabels[key];
            }

            var req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeKey, 
                RetrieveAsIfPublished = true
            };

            try
            {
                var attributeMetadataResponse = ((RetrieveAttributeResponse)_orgService.Execute(req));
                var result = attributeMetadataResponse?.AttributeMetadata;

                // Cache the result and return the Local Label
                var localLabel = result.DisplayName.UserLocalizedLabel?.Label ?? result.DisplayName.LocalizedLabels.FirstOrDefault()?.Label;
                _attributeLocalLabels.Add(key, localLabel);
                return localLabel;
            }
            catch (Exception ex)
            {
                _tracer.Trace($"Returning null; Error retrieving attribute metadata for {attributeKey}: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Relationship Metadata Helpers

        private RelatedEntityMetadata GetRelationshipMetadata(EntityMetadata entity, string relationshipKey)
        {
            var relatedEntityMetadata = new RelatedEntityMetadata();

            // Check the cache
            if (_oneToManyRelationshipMetadata.ContainsKey(relationshipKey))
            {
                return GetOneToManyRelationshipMetadataHelper(relationshipKey, _oneToManyRelationshipMetadata[relationshipKey], entity);
            }
            else if (_manyToManyRelationshipMetadata.ContainsKey(relationshipKey))
            {
                return GetManyToManyRelationshipMetadataHelper(relationshipKey, _manyToManyRelationshipMetadata[relationshipKey], entity);
            }

            var req = new RetrieveRelationshipRequest()
            {
                Name = relationshipKey,
                RetrieveAsIfPublished = true
            };

            try
            {
                var relationshipMetadataResponse = ((RetrieveRelationshipResponse)_orgService.Execute(req));

                if (relationshipMetadataResponse?.RelationshipMetadata.RelationshipType == RelationshipType.OneToManyRelationship)
                {
                    // Cache the result
                    var result = (OneToManyRelationshipMetadata)relationshipMetadataResponse?.RelationshipMetadata;
                    _oneToManyRelationshipMetadata.Add(relationshipKey, (OneToManyRelationshipMetadata)relationshipMetadataResponse?.RelationshipMetadata);
                    return GetOneToManyRelationshipMetadataHelper(relationshipKey, result, entity);
                }
                else
                {
                    // Cache the result
                    var result = (ManyToManyRelationshipMetadata)relationshipMetadataResponse?.RelationshipMetadata;
                    _manyToManyRelationshipMetadata.Add(relationshipKey, result);
                    return GetManyToManyRelationshipMetadataHelper(relationshipKey, result, entity);
                }
            }
            catch (Exception ex)
            {
                _tracer.Trace($"Returning null; Error retrieving relationship metadata for {relationshipKey}: {ex.Message}");
                return null;
            }
        }

        private RelatedEntityMetadata GetOneToManyRelationshipMetadataHelper(string relationshipKey, OneToManyRelationshipMetadata relationship, EntityMetadata entity)
        {
            var relatedEntityMetadata = new RelatedEntityMetadata();

            if (IsManyToOneRelationship(relationship, entity.LogicalName))
            {
                _tracer.Trace($"N:1 Relationship: {relationshipKey}");
                relatedEntityMetadata.Entity = GetEntityMetadata(relationship.ReferencingEntity);
                relatedEntityMetadata.LocalizedRelationshipDisplayName = GetEntityLocalizedPluralDisplayName(relatedEntityMetadata.Entity);
            }
            else
            {
                _tracer.Trace($"1:N Relationship: {relationshipKey}");
                relatedEntityMetadata.Entity = GetEntityMetadata(relationship.ReferencedEntity);
                relatedEntityMetadata.LocalizedRelationshipDisplayName =
                    GetAttributeLocalLabelFromMetadata(entity.LogicalName, relationship.ReferencingAttribute);
            }
            return relatedEntityMetadata;
        }

        private RelatedEntityMetadata GetManyToManyRelationshipMetadataHelper(string relationshipKey, ManyToManyRelationshipMetadata relationship, EntityMetadata entity)
        {
            var relatedEntityMetadata = new RelatedEntityMetadata();

            _tracer.Trace($"N:N Relationship: {relationshipKey}");
            relatedEntityMetadata.Entity = GetOtherEntityOnManyToManyRelationship(entity.LogicalName, relationship);
            relatedEntityMetadata.LocalizedRelationshipDisplayName = GetEntityLocalizedPluralDisplayName(relatedEntityMetadata.Entity);
            return relatedEntityMetadata;
        }

        private EntityMetadata GetOtherEntityOnManyToManyRelationship(string currentEntityName, ManyToManyRelationshipMetadata relationship)
        {
            var otherEntityName = currentEntityName == relationship.Entity1LogicalName 
                ? relationship.Entity2LogicalName 
                : relationship.Entity1LogicalName;

            _tracer.Trace($"Current Entity: {currentEntityName}; Other Entity: {otherEntityName}");
            return GetEntityMetadata(otherEntityName);
        }

        private bool IsManyToOneRelationship(OneToManyRelationshipMetadata relationship, string entityName)
        {
            // TODO: Edge case where entity is also ReferencingEntity
            return relationship.ReferencedEntity == entityName;
        }

        #endregion


        public void RetrieveAttributesRelationshipsForEntities(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _tracer.Trace($"Entered plugin: {nameof(RetrieveAttributesRelationshipsForEntities)}");

            var entityNamesStr = pluginContext.InputParameters["EntityLogicalNames"] as string;
            var entityNamesArr = entityNamesStr.Split(',');

            var attributesRelationshipsMetadata = SetMetadataForMetadataJson(entityNamesArr);
            pluginContext.OutputParameters["AttributeJson"] = GetMetadataJson(attributesRelationshipsMetadata);
        }

        public void RetrieveUserLookupsForEntity(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _tracer.Trace($"Entered plugin: {nameof(RetrieveUserLookupsForEntity)}");

            var entityName = pluginContext.InputParameters["EntityLogicalName"] as string;

            pluginContext.OutputParameters["AttributeJson"] = RetrieveUserLookupsForEntityMetadataJson(entityName);
        }

        private string RetrieveUserLookupsForEntityMetadataJson(string entity)
        {
            var results = new Dictionary<String, EntityInfo>();
            var entityInfo = new EntityInfo();

            MetadataFilterExpression EntityFilter = new MetadataFilterExpression(LogicalOperator.And);
            EntityFilter.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entity));

            MetadataPropertiesExpression EntityProperties = new MetadataPropertiesExpression()
            {
                AllProperties = false
            };
            EntityProperties.PropertyNames.AddRange(new string[] { "Attributes", "SchemaName", "DisplayName" });
            LabelQueryExpression labelQuery = new LabelQueryExpression();
            labelQuery.FilterLanguages.Add(GetUserLanguageCode());

            MetadataPropertiesExpression AttributeProperties = new MetadataPropertiesExpression() { AllProperties = false };
            AttributeProperties.PropertyNames.Add("SchemaName");
            AttributeProperties.PropertyNames.Add("DisplayName"); ;
            AttributeProperties.PropertyNames.Add("EntityLogicalName");
            AttributeProperties.PropertyNames.Add("Targets");
            AttributeProperties.PropertyNames.Add("IsValidForUpdate");

            MetadataFilterExpression AttributeTypeFilter = new MetadataFilterExpression(LogicalOperator.Or);
            AttributeTypeFilter.Conditions.Add(new MetadataConditionExpression("AttributeType", MetadataConditionOperator.Equals, AttributeTypeCode.Lookup));
            AttributeTypeFilter.Conditions.Add(new MetadataConditionExpression("AttributeType", MetadataConditionOperator.Equals, AttributeTypeCode.Owner));

            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Properties = EntityProperties,
                LabelQuery = labelQuery,
                Criteria = EntityFilter,
                AttributeQuery = new AttributeQueryExpression()
                {
                    Properties = AttributeProperties,
                    Criteria = AttributeTypeFilter
                },
            };

            RetrieveMetadataChangesResponse metadataChangesResponse = GetMetadataChanges(entityQueryExpression);
            foreach (var entityMetadata in metadataChangesResponse.EntityMetadata)
            {
                foreach (var attributeMetadata in entityMetadata.Attributes)
                {
                    if (attributeMetadata is LookupAttributeMetadata)
                    {
                        var lookupAttribute = (LookupAttributeMetadata)attributeMetadata;
                        if (lookupAttribute.Targets.Contains("systemuser") && (lookupAttribute.IsValidForUpdate ?? false))
                        {
							entityInfo.Attributes.Add(lookupAttribute.LogicalName, new AttributeInfo
							{
								AttributeDisplayName= lookupAttribute.DisplayName.UserLocalizedLabel.Label,
								AttributeType= lookupAttribute.AttributeType

							});
                        }
                    }
                }

                results.Add(entityMetadata.DisplayName.UserLocalizedLabel.Label, entityInfo);
            }

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Retrieve entity metadata individually
        /// Build RetrieveMetadataChangesRequest to retrieve all the display names for all the 1:N referencing attributes
        /// </summary>
        /// <param name="entities"></param>
        private ReferenceData SetMetadataForMetadataJson(string[] entities)
        {
            var referenceData = new ReferenceData();

            // Retrieve entity metadata and build entity filter by schema name
            Dictionary<string, EntityReferenceData> entityReferenceDataByEntity = new Dictionary<string, EntityReferenceData>();
            foreach (var entity in entities)
            {
                var em = RetrieveEntity(entity);
                referenceData.EntityMetadataCollection.Add(em);

                foreach (var attributeMetadata in em.Attributes)
                {
                    if (attributeMetadata.DisplayName != null && attributeMetadata.DisplayName.UserLocalizedLabel != null)
                        AddToDictionary(referenceData.AttributeNameMappings, attributeMetadata.SchemaName, attributeMetadata.DisplayName.UserLocalizedLabel.Label);
                }

                // Build filter for each 1:N referncing attribute to retrieve the display name
                foreach (var attribute in em.OneToManyRelationships)
                {
                    var entityReferenceData = AddEntityMetadataFilter(entityReferenceDataByEntity, attribute.ReferencingEntity);
                    entityReferenceData.AddAttributeMetadataFilter(entityReferenceData, attribute.ReferencingAttribute);
                    
                }
                foreach (var attribute in em.ManyToOneRelationships)
                {
                    var entityReferenceData = AddEntityMetadataFilter(entityReferenceDataByEntity, attribute.ReferencedEntity);
                    entityReferenceData.AddAttributeMetadataFilter(entityReferenceData, attribute.ReferencedAttribute);
                }
                foreach (var attribute in em.ManyToManyRelationships)
                {
                    if (attribute.Entity1LogicalName != entity)
                    {
                        AddEntityMetadataFilter(entityReferenceDataByEntity, attribute.Entity1LogicalName);
                    }
                    else
                    {
                        AddEntityMetadataFilter(entityReferenceDataByEntity, attribute.Entity2LogicalName);
                    }
                }

                AddEntityMetadataFilter(entityReferenceDataByEntity, em.SchemaName);
            }

            MetadataFilterExpression entityFilter = new MetadataFilterExpression(LogicalOperator.Or);
            MetadataFilterExpression schemaFilter = new MetadataFilterExpression(LogicalOperator.Or);
            int conditions = 0;
            foreach (string entity in entityReferenceDataByEntity.Keys)
            {
                var entityReferenceData = entityReferenceDataByEntity[entity];
                if (conditions + entityReferenceData.Conditions > _maxMetadataQueryFilters)
                {
                    RetrieveMetadataAndAddToReferenceData(entityFilter, schemaFilter, referenceData);
                    entityFilter = new MetadataFilterExpression(LogicalOperator.Or);
                    schemaFilter = new MetadataFilterExpression(LogicalOperator.Or);
                    conditions = 0;
                }

                entityFilter.Conditions.Add(entityReferenceData.EntitySchemaFilter);

                // Handle a single entity having more than 300 conditions by itself
                if (conditions == 0 && conditions + entityReferenceData.Conditions > _maxMetadataQueryFilters)
                {
                    var batchSize = _maxMetadataQueryFilters - 1;
                    var total = entityReferenceData.SchemaFilterByName.Count;
                    var processedConditions = 0;
                    while (total > 0)
                    {
                        if (schemaFilter.Conditions.Count > 0)
                        {
                            RetrieveMetadataAndAddToReferenceData(entityFilter, schemaFilter, referenceData);
                            schemaFilter = new MetadataFilterExpression(LogicalOperator.Or);
                            conditions = 0;
                        }

                        var batch = entityReferenceData.SchemaFilterByName.Values.Skip(processedConditions).Take(batchSize);
                        schemaFilter.Conditions.AddRange(batch);
                        processedConditions += batch.Count();
                        total -= batch.Count();
                        conditions += batch.Count();
                    }
                    continue;
                }

                schemaFilter.Conditions.AddRange(entityReferenceData.SchemaFilterByName.Values);
                conditions += entityReferenceData.Conditions;
            }

            if (conditions > 0)
            {
                RetrieveMetadataAndAddToReferenceData(entityFilter, schemaFilter, referenceData);
            }

            return referenceData;
        }

        private void RetrieveMetadataAndAddToReferenceData(MetadataFilterExpression entityFilter, MetadataFilterExpression attributeFilter, ReferenceData referenceData)
        {
            // Only return Entity Attributes and SchemaNames for user's language
            MetadataPropertiesExpression EntityProperties = new MetadataPropertiesExpression()
            {
                AllProperties = false
            };
            EntityProperties.PropertyNames.AddRange(new string[] { "Attributes", "SchemaName", "DisplayName", "DisplayCollectionName" });
            LabelQueryExpression labelQuery = new LabelQueryExpression();
            labelQuery.FilterLanguages.Add(GetUserLanguageCode());

            // Only return Attributes Schema and Display names
            MetadataPropertiesExpression AttributeProperties = new MetadataPropertiesExpression() { AllProperties = false };
            AttributeProperties.PropertyNames.Add("SchemaName");
            AttributeProperties.PropertyNames.Add("DisplayName");
            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Properties = EntityProperties,
                LabelQuery = labelQuery,
                Criteria = entityFilter,
                AttributeQuery = new AttributeQueryExpression()
                {
                    Criteria = attributeFilter,
                    Properties = AttributeProperties
                },
            };

            RetrieveMetadataChangesResponse metadataChangesResponse = GetMetadataChanges(entityQueryExpression);
            foreach (var entityMetadata in metadataChangesResponse.EntityMetadata)
            {
                foreach (var attributeMetadata in entityMetadata.Attributes)
                {
                    if (attributeMetadata.DisplayName != null && attributeMetadata.DisplayName.UserLocalizedLabel != null)
                        AddToDictionary(referenceData.AttributeNameMappings, attributeMetadata.SchemaName, attributeMetadata.DisplayName.UserLocalizedLabel.Label);
                }

                if (entityMetadata.DisplayCollectionName != null && entityMetadata.DisplayCollectionName.UserLocalizedLabel != null)
                    AddToDictionary(referenceData.EntityPluralNameMappings, entityMetadata.SchemaName, entityMetadata.DisplayCollectionName.UserLocalizedLabel.Label);
            }
        }

        private string GetMetadataJson(ReferenceData referenceData)
        {
            var results = new Dictionary<String, EntityInfo>();

            foreach (var entityMetadata in referenceData.EntityMetadataCollection)
            {
                var entityInfo = new EntityInfo();
                var filteredAttributes = entityMetadata.Attributes.Where(a => a.DisplayName != null && a.DisplayName.UserLocalizedLabel != null).ToList();
                foreach (var attribute in filteredAttributes)
                {
					entityInfo.Attributes.Add(attribute.LogicalName, new AttributeInfo
					{
						AttributeDisplayName = attribute.DisplayName.UserLocalizedLabel.Label,
						AttributeType = attribute.AttributeType

					});
				}
                foreach (var relationship in entityMetadata.OneToManyRelationships)
                {
                    if (entityInfo.Relationships.ContainsKey(relationship.SchemaName))
                        continue;

                    var relationshipInfo = new RelationshipInfo();
                    relationshipInfo.LookupField = relationship.ReferencingAttribute;
                    relationshipInfo.LookupFieldDisplayName = GetDisplayNameFromMapping(referenceData.AttributeNameMappings, relationship.ReferencingAttribute);
                    relationshipInfo.PrimaryEntity = relationship.ReferencedEntity;
                    relationshipInfo.PrimaryEntityDisplayName = GetDisplayNameFromMapping(referenceData.EntityPluralNameMappings, relationship.ReferencedEntity);
                    relationshipInfo.RelatedEntity = relationship.ReferencingEntity;
                    relationshipInfo.RelatedEntityDisplayName = GetDisplayNameFromMapping(referenceData.EntityPluralNameMappings, relationship.ReferencingEntity);
                    relationshipInfo.Type = "OneToManyRelationship";

                    entityInfo.Relationships.Add(relationship.SchemaName, relationshipInfo);
                }
                foreach (var relationship in entityMetadata.ManyToOneRelationships)
                {
                    if (entityInfo.Relationships.ContainsKey(relationship.SchemaName))
                        continue;

                    var relationshipInfo = new RelationshipInfo();
                    // Use the Referencing Attribute for both 1:N and N:1 relationships so the field name is always displayed
                    relationshipInfo.LookupField = relationship.ReferencingAttribute;
                    relationshipInfo.LookupFieldDisplayName = GetDisplayNameFromMapping(referenceData.AttributeNameMappings, relationship.ReferencingAttribute);
                    relationshipInfo.PrimaryEntity = relationship.ReferencedEntity;
                    relationshipInfo.PrimaryEntityDisplayName = GetDisplayNameFromMapping(referenceData.EntityPluralNameMappings, relationship.ReferencedEntity);
                    relationshipInfo.RelatedEntity = relationship.ReferencingEntity;
                    relationshipInfo.RelatedEntityDisplayName = GetDisplayNameFromMapping(referenceData.EntityPluralNameMappings, relationship.ReferencingEntity);
                    relationshipInfo.Type = "ManyToOneRelationship";

                    entityInfo.Relationships.Add(relationship.SchemaName, relationshipInfo);
                }
                foreach (var relationship in entityMetadata.ManyToManyRelationships)
                {
                    if (entityInfo.Relationships.ContainsKey(relationship.SchemaName))
                        continue;

                    var relationshipInfo = new RelationshipInfo();
                    var entity1DisplayName = GetDisplayNameFromMapping(referenceData.EntityPluralNameMappings, relationship.Entity1LogicalName);
                    var entity2DisplayName = GetDisplayNameFromMapping(referenceData.EntityPluralNameMappings, relationship.Entity2LogicalName);
                    if (relationship.Entity1LogicalName == entityMetadata.LogicalName)
                    {
                        relationshipInfo.PrimaryEntity = relationship.Entity1LogicalName;
                        relationshipInfo.PrimaryEntityDisplayName = entity1DisplayName;
                        relationshipInfo.RelatedEntity = relationship.Entity2LogicalName;
                        relationshipInfo.RelatedEntityDisplayName = entity2DisplayName;
                    }
                    else
                    {
                        relationshipInfo.PrimaryEntity = relationship.Entity2LogicalName;
                        relationshipInfo.PrimaryEntityDisplayName = entity2DisplayName;
                        relationshipInfo.RelatedEntity = relationship.Entity1LogicalName;
                        relationshipInfo.RelatedEntityDisplayName = entity1DisplayName;
                    }

                    relationshipInfo.Type = "ManyToManyRelationship";

                    entityInfo.Relationships.Add(relationship.SchemaName, relationshipInfo);
                }

                results.Add(entityMetadata.DisplayName.UserLocalizedLabel.Label, entityInfo);
            }

            return JsonConvert.SerializeObject(results);
        }

        private void AddToDictionary(Dictionary<string, string> mappings, string key, string value)
        {
            if (!mappings.ContainsKey(key.ToLower()))
                mappings.Add(key.ToLower(), value);
        }

        private EntityReferenceData AddEntityMetadataFilter(Dictionary<string, EntityReferenceData> entityReferenceDataByEntity, string entity)
        {
            if (entityReferenceDataByEntity.ContainsKey(entity) == false)
            {
                entityReferenceDataByEntity.Add(entity, new EntityReferenceData(entity));
            }

            return entityReferenceDataByEntity[entity];
        }

        private EntityMetadata RetrieveEntity(string entity)
        {
            var rer = new RetrieveEntityRequest();
            rer.EntityFilters = EntityFilters.Attributes | EntityFilters.Relationships;
            rer.LogicalName = entity;

            return ((RetrieveEntityResponse)_orgService.Execute(rer)).EntityMetadata;
        }

        private RetrieveMetadataChangesResponse GetMetadataChanges(
           EntityQueryExpression entityQueryExpression)
        {
            RetrieveMetadataChangesRequest retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
            {
                Query = entityQueryExpression,
                DeletedMetadataFilters = DeletedMetadataFilters.Default
            };

            return (RetrieveMetadataChangesResponse)_orgService.Execute(retrieveMetadataChangesRequest);
        }

        private int GetUserLanguageCode()
        {
            var fetch =
                @"<fetch count='1'>
                    <entity name='usersettings' > 
	                    <attribute name='localeid' />	
	                    <filter type='and'>	 
		                    <condition attribute='systemuserid' operator='eq-userid' /> 
			            </filter>			   
					</entity>					
				  </fetch>";

            return _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.First().GetAttributeValue<int>("localeid");
        }

        private string GetDisplayNameFromMapping(Dictionary<string, string> mapping, string schemaName)
        {
            if (mapping.ContainsKey(schemaName.ToLower()))
                return mapping[schemaName];

            return String.Empty;
        }
    }

    public class ReferenceData
    {
        public ReferenceData()
        {
            this.EntityMetadataCollection = new List<EntityMetadata>();
            this.EntityPluralNameMappings = new Dictionary<string, string>();
            this.AttributeNameMappings = new Dictionary<string, string>();
        }
        public List<EntityMetadata> EntityMetadataCollection { get; set; }
        public Dictionary<string, string> EntityPluralNameMappings { get; set; }
        public Dictionary<string, string> AttributeNameMappings { get; set; }
    }

    public class EntityReferenceData
    {
        public EntityReferenceData(string entity)
        {
            this.EntitySchemaFilter = new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entity);
            this.SchemaFilterByName = new Dictionary<string, MetadataConditionExpression>();
        }
        public MetadataConditionExpression EntitySchemaFilter { get; set; }
        public Dictionary<string, MetadataConditionExpression> SchemaFilterByName { get; set; }
        public int Conditions
        {
            get
            {
                return 1 + SchemaFilterByName.Keys.Count;
            }
        }

        public void AddAttributeMetadataFilter(EntityReferenceData entityReferenceData, string attribute)
        {
            if (entityReferenceData.SchemaFilterByName.ContainsKey(attribute))
            {
                return;
            }

            entityReferenceData.SchemaFilterByName.Add(attribute, new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, attribute));
        }
    }

    public class EntityInfo
    {
        [JsonProperty(PropertyName = "attributes")]
        public IDictionary<String, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>();
        [JsonProperty(PropertyName = "relationships")]
        public IDictionary<String, RelationshipInfo> Relationships = new Dictionary<string, RelationshipInfo>();
    }

	public class AttributeInfo
	{
		[JsonProperty(PropertyName = "attributeDisplayName")]
		public string AttributeDisplayName { get; set; }

		[JsonProperty(PropertyName = "attributeType")]
		public AttributeTypeCode? AttributeType { get; set; }
		

	}
    public class RelationshipInfo
    {
        [JsonProperty(PropertyName = "type")]
        public String Type;
        [JsonProperty(PropertyName = "primaryEntity")]
        public String PrimaryEntity;
        [JsonProperty(PropertyName = "primaryEntityDisplayName")]
        public String PrimaryEntityDisplayName;
        [JsonProperty(PropertyName = "relatedEntity")]
        public String RelatedEntity;
        [JsonProperty(PropertyName = "relatedEntityDisplayName")]
        public String RelatedEntityDisplayName;
        [JsonProperty(PropertyName = "lookupField")]
        public String LookupField;
        [JsonProperty(PropertyName = "lookupFieldDisplayName")]
        public String LookupFieldDisplayName;
    }
}
