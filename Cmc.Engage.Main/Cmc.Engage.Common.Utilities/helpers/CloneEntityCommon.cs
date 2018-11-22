using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common.Utilities
{
    public class CloneEntityCommon
    {
        private static readonly string[] _systemFields =
        {
            "createdon", "createdby", "createdonbehalfby",
            "modifiedby", "modifiedon", "modifiedonbehalfby",
            "ownerid", "owningbusinessunit", "owningteam", "owninguser",
            "overridencreatedon", "timezoneruleversionnumber",
            "utcconversiontimezonecode", "versionnumber"
        };

        public static T CloneEntity<T>(Entity sourceEntity, IEnumerable<string> copyAttributes)
            where T : Entity
        {
            var entityType = typeof(T);
            var logicalNameField = entityType.GetField("EntityLogicalName");

            var logicalName = sourceEntity.LogicalName;
            if (logicalNameField != null)
            {
                logicalName = (string)logicalNameField.GetValue(null);
            }

            var clonedEntity = new Entity(logicalName);
            foreach (var attribute in copyAttributes)
            {
                if (sourceEntity.Contains(attribute))
                {
                    clonedEntity[attribute] = sourceEntity[attribute];
                }
            }

            return clonedEntity.ToEntity<T>();
        }

        public static IEnumerable<string> RetrieveUpdateableFields(string entityName, IEnumerable<string> ignoreFields, IOrganizationService orgService, ILogger tracer)
        {
            tracer?.Trace($"Retrieving Updateable fields for {entityName}");

            var entityFilter = new MetadataFilterExpression(LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression(
                "LogicalName", MetadataConditionOperator.Equals, entityName));

            var attributeFilter = new MetadataFilterExpression(LogicalOperator.And);
            attributeFilter.Conditions.Add(new MetadataConditionExpression(
                "LogicalName", MetadataConditionOperator.NotIn, _systemFields));

            if (ignoreFields?.Count() > 0)
            {
                attributeFilter.Conditions.Add(new MetadataConditionExpression(
                    "LogicalName", MetadataConditionOperator.NotIn, ignoreFields));
            }

            attributeFilter.Conditions.Add(new MetadataConditionExpression(
                "IsValidForCreate", MetadataConditionOperator.Equals, true));

            var response = orgService.Execute(new RetrieveMetadataChangesRequest()
            {
                Query = new EntityQueryExpression()
                {
                    Criteria = entityFilter,
                    AttributeQuery = new AttributeQueryExpression()
                    {
                        Criteria = attributeFilter,
                        Properties = new MetadataPropertiesExpression("LogicalName")
                    },
                    LabelQuery = new LabelQueryExpression(),
                    Properties = new MetadataPropertiesExpression("Attributes")
                }
            }) as RetrieveMetadataChangesResponse;

            tracer?.Trace($"Successfully retrieved updatable fields for {entityName}.");

            return response?.EntityMetadata.FirstOrDefault()?.Attributes.Select(attr => attr.LogicalName);
        }
    }
}
