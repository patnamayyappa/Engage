using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Core.Xrm.ServerExtension.Core;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Common
{
    public class MarkToDoAsReadPortalLogic : PortalWebServiceLogicBase
    {
        private readonly ILogger _trace;
        private readonly MarkToDoAsReadLogic _markToDoAsReadLogic;
        private IOrganizationService _orgService;
        public MarkToDoAsReadPortalLogic(ILogger trace, MarkToDoAsReadLogic markToDoAsReadLogic, IOrganizationService orgService)
        {
            _trace = trace ?? throw new ArgumentNullException(nameof(trace));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _markToDoAsReadLogic = markToDoAsReadLogic ?? throw new ArgumentNullException(nameof(markToDoAsReadLogic));

        }
        public override object DoWork(IExecutionContext context, string inputData)
        {

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            var input = JsonConvert.DeserializeObject<Input>(inputData);
            List<Guid?> toDoIds = input.ToDoIds.Split(new[] { "~|~" }, StringSplitOptions.RemoveEmptyEntries).Select(x => ParseInputId(x.Trim())).ToList();

            if (toDoIds.Count == 0)
            {
                _trace.Trace("No ToDo Ids found in input, exiting");
                return false;
            }
            else if (input.StudentId == null)
            {
                _trace.Trace("Student Id not found in input, exiting");
                return false;
            }          
            _markToDoAsReadLogic.MarkToDosAsRead(toDoIds, input.StudentId.Value);

            return true;
        }

        private Guid? ParseInputId(string id)
        {
            Guid parsedId;
            return Guid.TryParse(id, out parsedId) ? parsedId : (Guid?)null;
        }

        public class Input
        {
            public String ToDoIds { get; set; }
            public Guid? StudentId { get; set; }
        }
    }
}
