using System;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common
{
    public class RetrieveContactImageLogic : PortalWebServiceLogicBase
    {
        private ILogger _trace;
        private IOrganizationService _orgService;
        public RetrieveContactImageLogic(ILogger trace, IOrganizationService orgService)
        {
            _trace = trace ?? throw new ArgumentNullException(nameof(trace));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
        }
        public class Input
        {
            public Guid? ContactId { get; set; }
        }

        public class Output
        {
            public string ContactImage { get; set; }
        }
        //private ILogger traceService;
        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            
            //var trace = serviceProvider.GetTracingService();
            _trace.Trace(nameof(RetrieveContactImageLogic));
            _trace.Trace($"inputdata: {inputData}");

            if (string.IsNullOrEmpty(inputData))
            {
                _trace.Trace("No Input Data present.");
                return null;
            }

            var input = GetInput<Input>(inputData);

            if (input.ContactId == null)
            {
                _trace.Trace("Contact Id is null.");
                return null;
            }

            var contact = _orgService.Retrieve(Contact.EntityLogicalName, input.ContactId.Value, new ColumnSet("entityimage")).ToEntity<Contact>();

            return new Output()
            {
                ContactImage = contact.EntityImage != null
                                ? Convert.ToBase64String(contact.EntityImage)
                                : null
            };
        }
    }
}
