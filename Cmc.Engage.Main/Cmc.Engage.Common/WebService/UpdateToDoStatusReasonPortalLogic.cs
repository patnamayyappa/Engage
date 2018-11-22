using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;


namespace Cmc.Engage.Common
{
    public class UpdateToDoStatusReasonPortalLogic : PortalWebServiceLogicBase
    {

        private readonly ILogger _trace;
        private readonly UpdateToDoStatusReasonLogic _markToDoAsReadLogic;
        private IOrganizationService _orgService;
        public UpdateToDoStatusReasonPortalLogic(ILogger trace, UpdateToDoStatusReasonLogic markToDoAsReadLogic, IOrganizationService orgService)
        {
            _trace = trace ?? throw new ArgumentException(nameof(trace));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _markToDoAsReadLogic = markToDoAsReadLogic ?? throw new ArgumentException(nameof(markToDoAsReadLogic));
        }
        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var input = JsonConvert.DeserializeObject<Input>(inputData);

            if (input == null || input.ToDoId == null || input.Status == null || input.StudentId == null)
            {
                _trace.Trace("Some or all of the input parameters are null, exiting");
                return false;
            }        
            return _markToDoAsReadLogic.UpdateStatusReason(input.ToDoId.Value, input.Status.Value, input.CloseComment, input.StudentId.Value);
        }

        public class Input
        {
            public Guid? ToDoId { get; set; }
            public int? Status { get; set; }
            public string CloseComment { get; set; }
            public Guid? StudentId { get; set; }
        }
    }
}
