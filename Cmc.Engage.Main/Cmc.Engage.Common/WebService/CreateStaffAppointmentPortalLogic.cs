using System;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Communication;

namespace Cmc.Engage.Common
{
    public class CreateStaffAppointmentPortalLogic : PortalWebServiceLogicBase
    {
        private ILogger _trace;
        private Input _input;
        private readonly IAppointmentService _createStaffAppointmentActionService;
        private IOrganizationService _orgService;

        public CreateStaffAppointmentPortalLogic(ILogger logger, IAppointmentService createStaffAppointmentActionService, IOrganizationService orgService)
        {
            _trace = logger ?? throw new ArgumentException(nameof(logger));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _createStaffAppointmentActionService = createStaffAppointmentActionService ?? throw new ArgumentException(nameof(createStaffAppointmentActionService));
        }
        public class Input
        {
            public Guid ContactId { get; set; }
            public Guid UserId { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public Guid LocationId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            _trace.Trace(nameof(CreateStaffAppointmentPortalLogic));
            _trace.Trace($"inputdata: {inputData}");
            _input = GetInput<Input>(inputData);
            string createdAppointmentJson = _createStaffAppointmentActionService.CreateStaffAppointment(
                _input.ContactId,
                _input.UserId,
                _input.LocationId,
                _input.StartDate.FromIso8601Date(),
                _input.EndDate.FromIso8601Date(),
                _input.Title,
                _input.Description);

            return createdAppointmentJson;
        }
    }
}
