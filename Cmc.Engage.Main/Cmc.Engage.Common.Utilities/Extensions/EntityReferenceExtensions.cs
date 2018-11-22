using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Cmc.Engage.Common.Utilities
{
    public static class EntityReferenceExtensions
    {
        public static string GetName(this EntityReference entityReference, IOrganizationService orgService, string primaryAttribute)
        {
            if (!String.IsNullOrWhiteSpace(entityReference.Name))
            {
                return entityReference.Name;
            }
            return orgService.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(primaryAttribute)).GetAttributeValue<String>(primaryAttribute);
        }
    }
}
