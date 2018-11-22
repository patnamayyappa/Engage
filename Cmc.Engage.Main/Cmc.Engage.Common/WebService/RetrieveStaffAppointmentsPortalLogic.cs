using System;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;


namespace Cmc.Engage.Common
{
    public class RetrieveStaffAppointmentsPortalLogic : PortalWebServiceLogicBase
    {
        private IOrganizationService _orgService;
        private ILogger _trace;
        private Input _input;
        public RetrieveStaffAppointmentsPortalLogic(ILogger trace, IOrganizationService orgService)
        {
            _trace = trace ?? throw new ArgumentException(nameof(trace));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
        }
        public class Input
        {
            public Guid ContactId { get; set; }
            public Guid UserId { get; set; }
        }

        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
           
            _trace.Trace(nameof(RetrieveStaffAppointmentsPortalLogic));
            _trace.Trace($"inputdata: {inputData}");
            _input = GetInput<Input>(inputData);

            var actionName = "cmc_RetrieveStaffAppointments";
            _trace.Trace($"Action name: {actionName}");
            var processAction = new OrganizationRequest(actionName);
            processAction.Parameters.Add("ContactId", _input.ContactId.ToString());
            processAction.Parameters.Add("UserId", _input.UserId.ToString());

            var result = _orgService.Execute(processAction);

            return result["StaffAppointmentsJson"];
        }
    }
}
