using System;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Common
{
    public class RetrieveStaffDetailsPortalLogic : PortalWebServiceLogicBase
    {
        private readonly ILogger _trace;
        private readonly RetrieveStaffDetailsLogic _retrieveStaffDetailsService;
        public class Input
        {
            public Guid? StudentId { get; set; }
            public Guid? SuccessNetworkId { get; set; }
        }
        public RetrieveStaffDetailsPortalLogic(ILogger trace, RetrieveStaffDetailsLogic retrieveStaffDetailsService)
        {
            _trace = trace ?? throw new ArgumentException(nameof(trace));
            _retrieveStaffDetailsService = retrieveStaffDetailsService ?? throw new ArgumentException(nameof(retrieveStaffDetailsService));

        }

        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            _trace.Trace(nameof(RetrieveStaffDetailsPortalLogic));
            _trace.Trace($"inputdata: {inputData}");
            var input = GetInput<Input>(inputData);

            if (!input.StudentId.HasValue || !input.SuccessNetworkId.HasValue)
            {
                return null;
            }
            return _retrieveStaffDetailsService.GetHours(input.StudentId.Value, input.SuccessNetworkId.Value);
        }
    }
}
