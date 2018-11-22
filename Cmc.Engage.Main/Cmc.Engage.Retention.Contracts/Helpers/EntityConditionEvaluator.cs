using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Engage.Contracts;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Retention
{
    public class EntityConditionEvaluator
    {
        private IDictionary<string, EntityMetadata> _entityMetadataMap;
        private IDictionary<string, string> _attributeMap;
        public EntityConditionEvaluator(IDictionary<string, EntityMetadata> entityMetadataMap)
        {
            _entityMetadataMap = entityMetadataMap;
            _attributeMap = new Dictionary<string, string>();
        }

        private static string ConvertToFetchValue(string conditionValue, AttributeMetadata metadata, IOrganizationService client)
        {
            switch (metadata.AttributeType)
            {
                case AttributeTypeCode.Boolean:
                    if (((BooleanAttributeMetadata)metadata).OptionSet.TrueOption.Label.UserLocalizedLabel.Label.ToLowerInvariant() == conditionValue.ToLowerInvariant())
                    {
                        return ((BooleanAttributeMetadata)metadata).OptionSet.TrueOption.Value.Value.ToString();
                    }
                    else if (((BooleanAttributeMetadata)metadata).OptionSet.FalseOption.Label.UserLocalizedLabel.Label.ToLowerInvariant() == conditionValue.ToLowerInvariant())
                    {
                        return ((BooleanAttributeMetadata)metadata).OptionSet.FalseOption.Value.Value.ToString();
                    }

                    return conditionValue;

                case AttributeTypeCode.Status:

                    var statusMetadata = (StatusAttributeMetadata)metadata;
                    foreach (var option in statusMetadata.OptionSet.Options)
                    {
                        if (option.Label.UserLocalizedLabel.Label.ToLowerInvariant() == conditionValue.ToLowerInvariant())
                        {
                            return option.Value.Value.ToString();
                        }
                    }

                    return conditionValue;

                case AttributeTypeCode.State:

                    var stateMetadata = (StateAttributeMetadata)metadata;
                    foreach (var option in stateMetadata.OptionSet.Options)
                    {
                        if (option.Label.UserLocalizedLabel.Label.ToLowerInvariant() == conditionValue.ToLowerInvariant())
                        {
                            return option.Value.Value.ToString();
                        }
                    }

                    return conditionValue;

                case AttributeTypeCode.Picklist:
                    var picklistMetadata = (PicklistAttributeMetadata)metadata;
                    foreach (var option in picklistMetadata.OptionSet.Options)
                    {
                        if (option.Label.UserLocalizedLabel.Label.ToLowerInvariant() == conditionValue.ToLowerInvariant())
                        {
                            return option.Value.Value.ToString();
                        }
                    }

                    return conditionValue;

                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Lookup:

                    var lookupMetadata = (LookupAttributeMetadata)metadata;
                    foreach (var target in lookupMetadata.Targets)
                    {
                        var metadataRequest = new RetrieveEntityRequest();
                        metadataRequest.EntityFilters = EntityFilters.Attributes | EntityFilters.Entity;
                        metadataRequest.LogicalName = target;

                        var response = (RetrieveEntityResponse)client.Execute(metadataRequest);
                        var primaryAttribute = response.EntityMetadata.PrimaryNameAttribute;

                        var fetch =
                            $@"<fetch>
                                    <entity name='{response.EntityMetadata.LogicalName}'>
                                        <filter>
                                            <condition operator='eq' attribute='{primaryAttribute}' value='{conditionValue}' />
                                        </filter>
                                    </entity>
                               </fetch>";

                        var results = client.RetrieveMultiple(new FetchExpression(fetch));
                        if (results.Entities != null && results.Entities.Count > 0)
                        {
                            return results.Entities.First().Id.ToString();
                        }
                    }

                    return conditionValue;

                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.DateTime:
                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Money:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.Uniqueidentifier:
                case AttributeTypeCode.BigInt:
                    return conditionValue;

                default:
                    return conditionValue;
            }
        }

        private static object ConvertAttributeValue(object attributeValue, AttributeMetadata metadata)
        {
            switch (metadata.AttributeType)
            {
                case AttributeTypeCode.Boolean:
                    return ((bool)attributeValue)
                        ? ((BooleanAttributeMetadata)metadata).OptionSet.TrueOption.Label.UserLocalizedLabel.Label.ToLowerInvariant()
                        : ((BooleanAttributeMetadata)metadata).OptionSet.FalseOption.Label.UserLocalizedLabel.Label.ToLowerInvariant();

                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Lookup:
                    return ((EntityReference)attributeValue).Name.ToLowerInvariant();

                case AttributeTypeCode.Money:
                    return ((Money)attributeValue).Value;

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    return ((PicklistAttributeMetadata)metadata).OptionSet.Options.First(o => o.Value == ((OptionSetValue)attributeValue).Value).Label.UserLocalizedLabel.Label.ToLowerInvariant();


                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                    return ((string)attributeValue).ToLowerInvariant();

                case AttributeTypeCode.DateTime:
                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.Uniqueidentifier:
                case AttributeTypeCode.BigInt:
                    return attributeValue;

                default:
                    throw new NotImplementedException($"Comparing attributes of type {metadata.AttributeType} is not supported.");
            }
        }
        private static object ParseConditionValue(string conditionValue, AttributeMetadata metadata)
        {
            switch (metadata.AttributeType)
            {
                case AttributeTypeCode.Boolean:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                    return (conditionValue == null) ? null : conditionValue.ToLowerInvariant();

                case AttributeTypeCode.DateTime:
                    return DateTime.Parse(conditionValue);

                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Money:
                    return decimal.Parse(conditionValue);

                case AttributeTypeCode.Double:
                    return double.Parse(conditionValue);

                case AttributeTypeCode.Integer:
                    return int.Parse(conditionValue);

                case AttributeTypeCode.Uniqueidentifier:
                    return Guid.Parse(conditionValue);

                case AttributeTypeCode.BigInt:
                    return long.Parse(conditionValue);

                default:
                    throw new NotImplementedException($"Comparing attributes of type {metadata.AttributeType} is not supported.");
            }
        }

        public IConditionEntity ConvertDisplayValues(string entity, IConditionEntity condition, IOrganizationService client)
        {
            if (String.IsNullOrWhiteSpace(condition.cmc_value))
                return condition;

            AttributeMetadata attributeMetadata;

            var attributePath = condition.ParseAttributeName();
            string attributeName;
            if (!_attributeMap.TryGetValue(attributePath.ToString(), out attributeName))
            {
                attributeName = attributePath.AttributeName;
            }

            EntityMetadata entityMetadata;
            if (attributePath.RelatedEntities.Count == 0)
            {
                entityMetadata = _entityMetadataMap[entity];
            }
            else
            {
                entityMetadata = _entityMetadataMap[attributePath.RelatedEntities.Last().EntityName];
            }
            attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributePath.AttributeName);

            condition.cmc_value = ConvertToFetchValue(condition.cmc_value, attributeMetadata, client);

            return condition;
        }

		public bool IsMatch(Entity entity, IConditionEntity condition)
		{
			var attributePath = condition.ParseAttributeName();
			return IsMatch(entity, condition, attributePath);
		}


		public bool IsMatch(Entity entity, IConditionEntity condition,AttributePath attributePath)
        {
            AttributeMetadata attributeMetadata;
            string attributeName;
            if (!_attributeMap.TryGetValue(attributePath.ToString(), out attributeName))
            {
                attributeName = attributePath.AttributeName;
            }

            EntityMetadata entityMetadata;
            if (attributePath.RelatedEntities.Count == 0)
            {
                entityMetadata = _entityMetadataMap[entity.LogicalName];
            }
            else
            {
                entityMetadata = _entityMetadataMap[attributePath.RelatedEntities.Last().EntityName];
            }
            attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributePath.AttributeName);


            if (entity.Contains(attributeName) && attributeMetadata != null)
            {
                object value = entity[attributeName];

                    if (value is AliasedValue)
                    {
                        value = ((AliasedValue)value).Value;
                    }

                    value = ConvertAttributeValue(value, attributeMetadata);

                    switch ((cmc_conditiontype?)condition.cmc_conditiontype?.Value)
                    {
                        case cmc_conditiontype.Equals:
                            var condValue = ParseConditionValue(condition.cmc_value, attributeMetadata);
                            return object.Equals(condValue, value);

                        case cmc_conditiontype.Range:
                            var condMin = ParseConditionValue(condition.cmc_min, attributeMetadata);
                            var condMax = ParseConditionValue(condition.cmc_max, attributeMetadata);
                            var comp = (IComparable)value;
                            return comp.CompareTo(condMin) >= 0 && comp.CompareTo(condMax) <= 0;

						case cmc_conditiontype.IsNull:
						return value == null;

						case cmc_conditiontype.IsNotNull:
						return value != null;

						case cmc_conditiontype.BeginsWith:
                            if (!String.IsNullOrWhiteSpace(condition.cmc_value))
                            {
                                return value.ToString().StartsWith(condition.cmc_value, StringComparison.InvariantCultureIgnoreCase);
                            }
                            else
                            {
                                var characters = charactersBetween(condition.cmc_min.First(), condition.cmc_max.First());
                                foreach (var character in characters)
                                {
                                    if (value.ToString().StartsWith(character.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        return true;
                                    }
                                }

								return false;
							}
					}
               
            }
            if (condition.cmc_conditiontype.Value == (int)cmc_conditiontype.Equals && condition.cmc_value == null)
            {
                return true;
            }

            return false;
        }

        public void AddAttributeAlias(string attributePath, string attributeAlias)
        {
            _attributeMap[attributePath] = attributeAlias;
        }

        private char[] charactersBetween(char start, char end)
        {
            return Enumerable.Range(start, end - start + 1).Select(c => (char)c).ToArray();
        }
    }
}
