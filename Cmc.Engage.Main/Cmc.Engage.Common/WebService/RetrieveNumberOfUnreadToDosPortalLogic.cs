using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public class RetrieveNumberOfUnreadToDosPortalLogic : PortalWebServiceLogicBase
    {
        private ILogger traceService;
        private readonly RetrieveNumberOfUnreadToDosLogic _retrieveNumberOfUnreadToDosLogic;
        private IOrganizationService _orgService;
        public RetrieveNumberOfUnreadToDosPortalLogic(ILogger trace, RetrieveNumberOfUnreadToDosLogic retrieveNumberOfUnreadToDosLogic, IOrganizationService orgService)
        {
            traceService = trace ?? throw new ArgumentException(nameof(trace));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _retrieveNumberOfUnreadToDosLogic = retrieveNumberOfUnreadToDosLogic ?? throw new ArgumentException(nameof(retrieveNumberOfUnreadToDosLogic));

        }
        public override object DoWork(IExecutionContext context, string inputData)
        {

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            traceService.Trace($"Raw Input {inputData}");
            var input = JsonConvert.DeserializeObject<Input>(inputData);

            if (input.StudentId == null)
            {
                traceService.Trace("Student not found. Return 0");
                return 0;
            }
            return _retrieveNumberOfUnreadToDosLogic.RetrieveNumberOfUnreadToDos(input.StudentId.Value);
        }

        public class Input
        {
            public Guid? StudentId { get; set; }
        }
    }
}
