using System;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public class PortalRetrieveMultiLingualValues: PortalWebServiceLogicBase
    {
        private readonly ILogger traceService;
        private readonly ILanguageService _todosLogic;
        private readonly GetPortalUserLanguageCode _portalLanguageLogic;
        private IOrganizationService _orgService;
        public PortalRetrieveMultiLingualValues(ILogger tracer, ILanguageService todosLogic, GetPortalUserLanguageCode portalLanguageLogic, IOrganizationService orgService)
        {
            traceService = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
            _todosLogic = todosLogic ?? throw new ArgumentNullException(nameof(todosLogic));
            _portalLanguageLogic = portalLanguageLogic ?? throw new ArgumentNullException(nameof(portalLanguageLogic));
        }
        public override object DoWork(IExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var input = JsonConvert.DeserializeObject<PortalRetrieveMultiLingualValuesInput>(inputData);
            var languageCode = _portalLanguageLogic.GetUserLanguageCode(input.ContactId, input.WebsiteId);
            var keys = input.Keys.Split(new[] { "~|~" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            var values = _todosLogic.Get(keys, languageCode);

            if (input.OutputType == OutputType.CSV)
            {
                return String.Join(",", values.Select(v => String.Join(",", v.Key, v.Value)));
            }

            return values;
        }

        public class PortalRetrieveMultiLingualValuesInput
        {
            public Guid? ContactId { get; set; }
            public Guid? WebsiteId { get; set; }
            public String Keys { get; set; }
            public OutputType OutputType { get; set; } = OutputType.JSON;
        }

        public enum OutputType
        {
            JSON,
            CSV
        }


    }
}
