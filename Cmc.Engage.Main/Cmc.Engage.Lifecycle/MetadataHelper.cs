using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Lifecycle
{
    public static class MetadataHelper
    {
        private static Dictionary<string, string[]> attributeTargetEntities = new Dictionary<string, string[]>();
        private static Dictionary<string, string> entityPrimaryKeys = new Dictionary<string, string>();
        private static Dictionary<string, string> entityPrimaryNames = new Dictionary<string, string>();
        private static Dictionary<string, AttributeMetadata> attributeMetadata = new Dictionary<string, AttributeMetadata>();
        private static Dictionary<string, string> attributeDisplayNames = new Dictionary<string, string>();

        public static string[] GetAttributeTargetEntity(IOrganizationService orgService, ILogger log, string entityLogicalName, string attributeName)
        {
            // Check the cache
            var key = entityLogicalName + "." + attributeName;
            if (attributeTargetEntities.ContainsKey(key))
            {
                return attributeTargetEntities[key];
            }

            var req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true,
            };

            try
            {
                var attributeMetadataResponse = ((RetrieveAttributeResponse)orgService.Execute(req));
                var lookupAttributeMetadata = (LookupAttributeMetadata)attributeMetadataResponse?.AttributeMetadata;
                string[] targets = lookupAttributeMetadata?.Targets;

                // Cache the result and return it
                attributeTargetEntities[key] = targets;
                return targets;
            }
            catch (Exception ex)
            {
                log.Error($"Error retrieving attribute metadata: {ex.Message}");
                return null;
            }
        }

        public static string GetEntityPrimaryKey(IOrganizationService orgService, ILogger log, string entityLogicalName)
        {
            // Check the cache
            if (entityPrimaryKeys.ContainsKey(entityLogicalName))
            {
                return entityPrimaryKeys[entityLogicalName];
            }
            var req = new RetrieveEntityRequest()
            {
                LogicalName = entityLogicalName,
                RetrieveAsIfPublished = true,
                EntityFilters = EntityFilters.Entity
            };

            try
            {
                var attributeMetadataResponse = ((RetrieveEntityResponse)orgService.Execute(req));
                var result = attributeMetadataResponse?.EntityMetadata?.PrimaryIdAttribute;
                entityPrimaryKeys[entityLogicalName] = result;
                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error retrieving entity metadata: {ex.Message}");
                return null;
            }
        }

        public static string GetTwoOptionCodeForBoolValue(IOrganizationService orgService, ILogger log, string entityLogicalName, string attribute, string val)
        {
            BooleanAttributeMetadata attr = (BooleanAttributeMetadata)MetadataHelper.GetAttributeMetadata(orgService, log, entityLogicalName, attribute);
            var trueLabel = attr.OptionSet.TrueOption.Label.UserLocalizedLabel.Label;
            var falseLabel = attr.OptionSet.FalseOption.Label.UserLocalizedLabel.Label;

            if (val.ToLower() == trueLabel.ToLower())
            {
                return attr.OptionSet.TrueOption.Value.ToString();
            }
            else if (val.ToLower() == falseLabel.ToLower())
            {
                return attr.OptionSet.FalseOption.Value.ToString();
            }
            else
            {
                log.Error($"Invalid label value {val} for attribute {attr.LogicalName} of type Two Options. Valid labels are {trueLabel} and {falseLabel}");
            }
            return null;
        }

        public static bool DateTimeStringIsDateOnly(string dateTimeString)
        {
            // To determine if the passed in string was a date or datetime, add a time to the end and see if it parses.
            return DateTime.TryParse(dateTimeString + " 00:00", out DateTime parseResult);
        }
                
        public static DateTime? ParseStringDateTimeAndConvertToUTC(string dateTimeString, TimeZoneInfo timeZone, DateTimeBehavior dateTimeBehavior)
        {
            if (dateTimeBehavior != DateTimeBehavior.UserLocal)
            {
                timeZone = TimeZoneInfo.Utc;
            }

            if (!DateTime.TryParse(dateTimeString, out DateTime inputDateTime))
            {
                return null;
            }

            inputDateTime = DateTime.SpecifyKind(inputDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(inputDateTime, timeZone);
        }

        public static ConditionOperator GetConditionOperatorForDateTimeString(string dateTimeString, bool isRange, bool isMinimum = false)
        {
            ConditionOperator dateOperator = ConditionOperator.On;
            ConditionOperator dateTimeOperator = ConditionOperator.Equal;

            if (isRange)
            {
                if (isMinimum)
                {
                    dateOperator = ConditionOperator.OnOrAfter;
                    dateTimeOperator = ConditionOperator.GreaterEqual;
                }
                else
                {
                    dateOperator = ConditionOperator.OnOrBefore;
                    dateTimeOperator = ConditionOperator.LessEqual;
                }
            }

            return MetadataHelper.DateTimeStringIsDateOnly(dateTimeString) ? dateOperator : dateTimeOperator;
        }

        public static string GetEntityPrimaryName(IOrganizationService orgService, ILogger log, string entityLogicalName)
        {
            // Check the cache
            if (entityPrimaryNames.ContainsKey(entityLogicalName))
            {
                return entityPrimaryNames[entityLogicalName];
            }
            var req = new RetrieveEntityRequest()
            {
                LogicalName = entityLogicalName,
                RetrieveAsIfPublished = true,
                EntityFilters = EntityFilters.Entity
            };

            try
            {
                var attributeMetadataResponse = ((RetrieveEntityResponse)orgService.Execute(req));
                var result = attributeMetadataResponse?.EntityMetadata?.PrimaryNameAttribute;
                entityPrimaryNames[entityLogicalName] = result;
                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error retrieving entity metadata: {ex.Message}");
                return null;
            }
        }

        public static bool AttributeTypeIsLookup(string attributeType)
        {
            return attributeType == Constants.LookupAttributeType || attributeType == Constants.OwnerAttributeType || attributeType == Constants.CustomerAttributeType;
        }

        public static AttributeMetadata GetAttributeMetadata(IOrganizationService orgService, ILogger log, string entityLogicalName, string attributeName)
        {
            // Check the cache
            var key = entityLogicalName + "." + attributeName;
            if (attributeMetadata.ContainsKey(key))
            {
                return attributeMetadata[key];
            }

            var req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true,
            };

            try
            {
                var attributeMetadataResponse = ((RetrieveAttributeResponse)orgService.Execute(req));
                attributeMetadata[key] = attributeMetadataResponse?.AttributeMetadata;
                return attributeMetadataResponse?.AttributeMetadata;
            }
            catch (Exception ex)
            {
                log.Error($"Error retrieving attribute metadata: {ex.Message}");
                return null;
            }
        }

        public static string GetAttributeType(IOrganizationService orgService, ILogger log, string entityLogicalName, string attributeName)
        {
            var attributeMetadata = GetAttributeMetadata(orgService, log, entityLogicalName, attributeName);
            return attributeMetadata?.AttributeType?.ToString();
        }

        public static string GetAttributeDateTimeBehavior(IOrganizationService orgService, ILogger log, string entityLogicalName, string attributeName)
        {
            var attributeMetadata = (DateTimeAttributeMetadata)GetAttributeMetadata(orgService, log, entityLogicalName, attributeName);
            return attributeMetadata?.DateTimeBehavior?.Value?.ToString();
        }

        public static int? GetOptionSetValueFromLabel(IOrganizationService orgService, ILogger log, string entityLogicalName, string attribute, string label)
        {
            var attributeMetadata = (EnumAttributeMetadata)GetAttributeMetadata(orgService, log, entityLogicalName, attribute);
            return attributeMetadata?.OptionSet?.Options?.Where(x => x.Label.UserLocalizedLabel.Label.Equals(label)).Select(x => x.Value).FirstOrDefault();
        }

        public static string GetAttributeDisplayName(IOrganizationService orgService, ILogger log, string entityLogicalName, string attributeLogicalName)
        {
            // Check the cache
            var key = entityLogicalName + "." + attributeLogicalName;
            if (attributeDisplayNames.ContainsKey(key))
            {
                return attributeDisplayNames[key];
            }
            var req = new RetrieveEntityRequest()
            {
                LogicalName = entityLogicalName,
                RetrieveAsIfPublished = true,
                EntityFilters = EntityFilters.Attributes
            };

            try
            {
                var attributeMetadataResponse = ((RetrieveEntityResponse)orgService.Execute(req));
                var attributes = attributeMetadataResponse?.EntityMetadata?.Attributes;
                var result = attributes?.Where(x => x.LogicalName.Equals(attributeLogicalName, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.DisplayName.UserLocalizedLabel.Label).FirstOrDefault();
                attributeDisplayNames[key] = result;
                return result;
            }
            catch (Exception ex)
            {
                log.Info($"Error retrieving entity metadata: {ex.Message}");
                return null;
            }
        }
    }
}