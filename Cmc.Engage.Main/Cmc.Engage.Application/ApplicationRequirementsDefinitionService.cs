using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Application.Contracts.Interfaces;
using Cmc.Engage.Common;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Application
{
    public class ApplicationRequirementsDefinitionService : IApplicationRequirementsDefinitionService
    {
        private readonly ILanguageService _languageService;
        private readonly ILogger _logger;
        private readonly IOrganizationService _orgService;

        public ApplicationRequirementsDefinitionService(ILogger logger, IOrganizationService orgService,
            ILanguageService languageService)
        {
            _logger = logger;
            _orgService = orgService;
            _languageService = languageService;
        }

        public void CreateUpdateApplicationRequirementsDefinitionDetail(IExecutionContext executionContext)
        {
            _logger.Trace("Starting ApplicationRequirementsDefinitionDetailPlugin");
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var entity = pluginContext.GetInputParameter<Entity>("Target")
                .ToEntity<cmc_applicationrequirementdefinitiondetail>();

            if (entity.cmc_requirementtype.Value != (int) cmc_applicationrequirementtype.OfficialTranscript
                && entity.cmc_requirementtype.Value != (int) cmc_applicationrequirementtype.UnofficialTranscript
                && entity.cmc_requirementtype.Value != (int) cmc_applicationrequirementtype.Recommendation)
                return;

                var extra = _orgService.Retrieve(entity.ToEntityReference(),
                    new ColumnSet("cmc_applicationrequirementdefinition"));
                entity.cmc_applicationrequirementdefinition = (extra as cmc_applicationrequirementdefinitiondetail)
                    .cmc_applicationrequirementdefinition;

            var details = _orgService.RetrieveMultiple(new FetchExpression(
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
<entity name='cmc_applicationrequirementdefinitiondetail'>
<attribute name='cmc_name'/>
<attribute name='cmc_requirementtype'/>
<filter type='and'>
<condition attribute='statecode' operator='eq' value='{(int) cmc_applicationrequirementdefinitiondetailState.Active}' />
<condition attribute='cmc_applicationrequirementdefinition' operator='eq' uitype='cmc_applicationrequirementdefinition' value='{entity.cmc_applicationrequirementdefinition.Id}' />
</filter>
</entity></fetch>"));
            foreach (var detail in details.Entities)
            {
                var detailEntity = detail.ToEntity<cmc_applicationrequirementdefinitiondetail>();
                if (detailEntity.cmc_requirementtype.Value == entity.cmc_requirementtype.Value &&
                    detailEntity.Id != entity.Id)
                    throw new InvalidPluginExecutionException(
                        string.Format(_languageService.Get("ApplicationRequirementDetailDuplicateError"),
                            detailEntity.cmc_name)
                    );
            }
        }
    }
}