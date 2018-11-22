using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public class RetrieveStudentSuccessNetworkPortalLogic : PortalWebServiceLogicBase
    {
        private IOrganizationService _orgService;
        private ILogger _trace;

        public RetrieveStudentSuccessNetworkPortalLogic(ILogger trace, IOrganizationService orgService)
        {
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _trace = trace ?? throw new ArgumentException(nameof(trace));
        }
        public class Input
        {
            public Guid? StudentId { get; set; }
        }

        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            _trace.Trace(nameof(RetrieveStudentSuccessNetworkPortalLogic));
            _trace.Trace($"inputdata: {inputData}");
            var input = GetInput<Input>(inputData);

            if (!input.StudentId.HasValue)
            {
                return new StudentSuccessNetworkStaff[0];
            }

            return Get(input.StudentId.Value);
        }
        public Dictionary<Guid, StudentSuccessNetworkStaff> Get(Guid studentId)
        {
            var fetch = $@"<fetch>
					<entity name='cmc_successnetwork'>
						<attribute name='cmc_staffroleid'/>
						<attribute name='cmc_staffmemberid'/>						
						<link-entity name='systemuser' from='systemuserid' to='cmc_staffmemberid' alias='systemuser'>
							<attribute name='entityimage'/>
                            <attribute name='lastname' />
						</link-entity>
						<filter>
							<condition attribute='cmc_studentid' operator='eq' value='{studentId}'/>
							<condition attribute='cmc_staffmemberid' operator='not-null'/>
							<condition attribute='statecode' operator='eq' value='0'/>
						</filter>
					</entity>
				</fetch>";
            return _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.ToDictionary(
                x => x.Id,
                x => new StudentSuccessNetworkStaff(x.ToEntity<cmc_successnetwork>()));
        }

        public class StudentSuccessNetworkStaff
        {
            public Guid? StaffMemberId { get; private set; }
            public string StaffMemberName { get; set; }
            public string StaffMemberLastName { get; set; }
            public string StaffRoleName { get; private set; }
            public string StaffPictureBase64 { get; private set; }

            public StudentSuccessNetworkStaff(cmc_successnetwork successNetwork)
            {
                StaffMemberId = successNetwork.cmc_staffmemberid?.Id;
                StaffMemberName = successNetwork.cmc_staffmemberid?.Name;
                StaffMemberLastName = successNetwork.Contains("systemuser.lastname") ? ((AliasedValue)successNetwork["systemuser.lastname"])?.Value as string : null;
                StaffRoleName = successNetwork.cmc_staffroleid?.Name;
                StaffPictureBase64 = successNetwork.Contains("systemuser.entityimage") ? Convert.ToBase64String(((byte[])((AliasedValue)successNetwork["systemuser.entityimage"]).Value)) : null;
            }
        }
    }
}
