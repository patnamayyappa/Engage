using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Communication;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;


namespace Cmc.Engage.Common
{
    public class RetrieveStaffAvailabilityFromOfficeHoursPortalLogic : PortalWebServiceLogicBase
    {
        private IOrganizationService _orgService;
        private readonly ILogger _trace;
        private Input _input;
        private readonly IAppointmentService _retrieveOfficeHoursLogic;
        public RetrieveStaffAvailabilityFromOfficeHoursPortalLogic(ILogger trace, IAppointmentService retrieveOfficeHoursLogic, IOrganizationService orgService)
        {
            _trace = trace ?? throw new ArgumentException(nameof(trace));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _retrieveOfficeHoursLogic = retrieveOfficeHoursLogic ?? throw new ArgumentException(nameof(retrieveOfficeHoursLogic));
        }
        public class Input
        {
            public Guid UserId { get; set; }
            public Guid AccountId { get; set; }
        }

        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            _trace.Trace(nameof(RetrieveStaffAvailabilityFromOfficeHoursPortalLogic));
            _trace.Trace($"inputdata: {inputData}");
            _input = GetInput<Input>(inputData);
            string availabilityJson = _retrieveOfficeHoursLogic.RetrieveStaffAvailability(_input.UserId, _input.AccountId);

            return availabilityJson;
        }
    }
}
