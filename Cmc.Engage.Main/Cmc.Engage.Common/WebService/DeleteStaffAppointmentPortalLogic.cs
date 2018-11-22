using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Communication;
using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public class DeleteStaffAppointmentPortalLogic : PortalWebServiceLogicBase
    {
        private ILogger _trace;
        private IAppointmentService _deleteLogic;
        private IOrganizationService _orgService;
        public DeleteStaffAppointmentPortalLogic(ILogger trace, IAppointmentService deleteLogic, IOrganizationService orgService)
        {
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _trace = trace ?? throw new ArgumentException(nameof(trace));
            _deleteLogic = deleteLogic ?? throw new ArgumentException(nameof(deleteLogic));
        }
        private Input _input;
       
        public class Input
        {
            public Guid AppointmentId { get; set; }
        }

        public override object DoWork(IExecutionContext context, string inputData)
        {           
            var serviceProvider = context.XrmServiceProvider;          
            _trace.Trace(nameof(DeleteStaffAppointmentPortalLogic));
            _trace.Trace($"inputdata: {inputData}");
            _input = GetInput<Input>(inputData);            
            string deletedAppointmentJson = _deleteLogic.DeleteStaffAppointment(_input.AppointmentId);

            return deletedAppointmentJson;
        }
    }
}
