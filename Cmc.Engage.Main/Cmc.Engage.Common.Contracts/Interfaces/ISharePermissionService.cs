using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Cmc.Engage.Common
{
    public interface ISharePermissionService
    {
        /// <summary>
        /// Share Permisson for trip approval 
        /// </summary>
        /// <param name="userIdOrTeamId">User id or team id </param>
        /// <param name="tripId">Trip Id</param>
        /// <param name="action">share or unshare</param>
        void SharePermission(IOrganizationServiceFactory organizationServiceFactory, EntityReference userIdOrTeamId, EntityReference ownerId,EntityReference targetEntityId, List<AccessRights> listAccessRights = null);
    }
}
