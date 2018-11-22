using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmc.Engage.Common.Activities
{
    /// <summary>
    /// Activity for share permissions
    /// </summary>
    public class SharePermissionActivity : ActivityBase
    {

        protected override void Execute(IActivityExecutionContext context)
        {
            var tracer = context.LoggerFactory.GetLogger(this.GetType());
            var sharePermissionService = context.IocScope.Resolve<ISharePermissionService>();
            IWorkflowContext workflowContext = context.ActivityContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.ActivityContext.GetExtension<IOrganizationServiceFactory>();
            tracer.Trace($"Getting primary entity id.");
            var primaryEntityId = workflowContext.PrimaryEntityId;
            tracer.Trace($"Got primary entity id: {primaryEntityId}.");
            tracer.Trace($"Getting primary entity name.");
            var primaryEntityName = workflowContext.PrimaryEntityName;
            tracer.Trace($"Got primary entity name : {primaryEntityName}.");

            if (primaryEntityId == null || primaryEntityName == null) {
                tracer.Error($"Primary entity id or name is null.");
                return;
            }

            tracer.Trace("Reading In Arguments.");
           
            dynamic userIdOrTeamId = null;
            if (UserId.Get(context.ActivityContext) != null)
            {
                userIdOrTeamId = UserId.Get(context.ActivityContext);
                tracer.Trace("User Id is " + userIdOrTeamId.Id);
            }
            else if (TeamId.Get(context.ActivityContext) != null)
            {
                userIdOrTeamId = TeamId.Get(context.ActivityContext);
                tracer.Trace("Team Id is " + userIdOrTeamId.Id);
            }
            else if (userIdOrTeamId == null)
            {
                tracer.Error("User Id and Team Id is null");
                return;
            }
            tracer.Trace("User or Team Id is " + userIdOrTeamId.Id);
            var readAccess = ReadAccess.Get(context.ActivityContext);
            tracer.Trace("Read Access " + readAccess.ToString());
            var writeAccess = WriteAccess.Get(context.ActivityContext);
            tracer.Trace("Write Access " + writeAccess.ToString());
            var appendAccess = AppendAccess.Get(context.ActivityContext);
            tracer.Trace("Append Access " + appendAccess.ToString());
            var shareAccess = ShareAccess.Get(context.ActivityContext);
            tracer.Trace("Share Access " + shareAccess.ToString());
            List<AccessRights> listAccessRights = new List<AccessRights>();
            if (readAccess)
                listAccessRights.Add(AccessRights.ReadAccess);
            if (writeAccess)
                listAccessRights.Add(AccessRights.WriteAccess);
            if (appendAccess)
            {
                listAccessRights.Add(AccessRights.AppendAccess);
                listAccessRights.Add(AccessRights.AppendToAccess);
            }
            if (shareAccess)
                listAccessRights.Add(AccessRights.ShareAccess);
            tracer.Trace($"Assess List Count : {listAccessRights.Count}.");

            sharePermissionService.SharePermission(serviceFactory,userIdOrTeamId, OwnerId.Get(context.ActivityContext), new EntityReference(primaryEntityName, primaryEntityId), listAccessRights);
           
        }
        [ReferenceTarget(SystemUser.EntityLogicalName)]
        [Input("Assigned User")]
        public InArgument<EntityReference> UserId { get; set; }

        [ReferenceTarget(Team.EntityLogicalName)]
        [Input("Team")]
        public InArgument<EntityReference> TeamId { get; set; }

        [RequiredArgument]
        [ReferenceTarget(SystemUser.EntityLogicalName)]
        [Input("Assigning User")]
        public InArgument<EntityReference> OwnerId { get; set; }

        [RequiredArgument]        
        [Input("Read Access")]
        public InArgument<bool> ReadAccess { get; set; }

        [RequiredArgument]
        [Input("Write Access")]        
        public InArgument<bool> WriteAccess { get; set; }

        [RequiredArgument]
        [Input("Share Access")]        
        public InArgument<bool> ShareAccess { get; set; }

        [RequiredArgument]
        [Input("Append Access")]        
        public InArgument<bool> AppendAccess { get; set; }
        
    }
}
