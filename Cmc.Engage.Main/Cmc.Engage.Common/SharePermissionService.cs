using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.WebServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    /// <summary>
    /// Service for share permission
    /// </summary>
    public class SharePermissionService : ISharePermissionService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _logger;
        private IOrganizationServiceFactory _serviceFactory;
        public SharePermissionService(ILogger logger, IOrganizationService organizationService)
        {
            _logger = logger;
            _orgService = organizationService;
            
        }
        /// <summary>
        /// Method for grant and modify permission 
        /// </summary>
        /// <param name="organizationServiceFactory">Organization service factory</param>
        /// <param name="userIdOrTeamId">User or Team whom getting the permission</param>
        /// <param name="ownerId">User who giving the permission</param>
        /// <param name="targetEntityId">Entity that getting permission</param>
        /// <param name="listAccessRights">Collection of permission type</param>        
        public void SharePermission(IOrganizationServiceFactory organizationServiceFactory, EntityReference userIdOrTeamId,EntityReference ownerId, EntityReference targetEntityId, List<AccessRights> listAccessRights = null)
        {
            _serviceFactory = organizationServiceFactory;
            _orgService = _serviceFactory.CreateOrganizationService(ownerId.Id);
            _logger.Trace($"Entered Into SharePermission ");
            _logger.Trace($"OwnerId {ownerId.Id}");


            var modifyAccessRequest = new ModifyAccessRequest()
            {

                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.None,
                    Principal = userIdOrTeamId
                },
                Target = targetEntityId
            };
            _logger.Trace($"Created request type of ModifyAccessRequest.");
            _orgService.Execute(modifyAccessRequest);
            _logger.Trace($"Modified given permission for user or team :{userIdOrTeamId.Id}.");

            if (listAccessRights != null && listAccessRights.Any())
            {
                _logger.Trace($"Entered Into SharePermission type of grant.");
                AccessRights? accessRights = new AccessRights();
                foreach (AccessRights accessRight in listAccessRights)
                {
                    accessRights |= accessRight;
                }
                _logger.Trace($"Access Rights : {accessRights.Value}");
                var grantAccessRequest = new GrantAccessRequest()
                {
                    PrincipalAccess = new PrincipalAccess
                    {
                        AccessMask = (AccessRights)accessRights,
                        Principal = userIdOrTeamId
                    },
                    Target = targetEntityId
                };
                _logger.Trace($"Created request type of GrantAccessRequest.");                
                _orgService.Execute(grantAccessRequest);
                _logger.Trace($"Granted given permission for user or team :{userIdOrTeamId.Id}.");
            }
           
        }
    }
}

