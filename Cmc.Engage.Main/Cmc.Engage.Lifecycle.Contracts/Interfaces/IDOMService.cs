using System.Collections.Generic;
using Cmc.Engage.Lifecycle.Messages;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Lifecycle
{
    public interface IDOMService
    {
        void ProcessDomAssignment(string entityLogicalName,IOrganizationService organizationService);
		/// <summary>
		/// gets the entity chain for the attribute schema string
		/// </summary>
		/// <param name="schemaString"></param>
		/// <returns></returns>
		List<EntityRelationship> GetEntityChainForAttributeSchemaString(string schemaString);
	}
}
