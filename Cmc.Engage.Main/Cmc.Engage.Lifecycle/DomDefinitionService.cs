using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    public class DomDefinitionService : IDomDefinitionService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;
        private readonly ILanguageService _retrieveMultiLingualValues;

        public DomDefinitionService(ILogger tracer, ILanguageService retrieveMultiLingualValues, IOrganizationService orgService)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _retrieveMultiLingualValues = retrieveMultiLingualValues ?? throw new ArgumentException(nameof(retrieveMultiLingualValues));      
             _orgService = orgService;
        }

        #region Validate Dom Definition
        public void ValidateDomDefinition(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;

            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _tracer.Trace($"Entered: {nameof(ValidateDomDefinition)}");
            if (!pluginContext.IsValidCall(cmc_domdefinition.EntityLogicalName))
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            }

            _tracer.Trace("Retrieving Target");
            var target = pluginContext.GetTargetEntity<cmc_domdefinition>();

            _tracer.Trace("Retrieving PreImage");
            var preImage = pluginContext.GetPreEntityImage<cmc_domdefinition>("Target");

            ValidateDomDefinition(target, preImage);
        }

        private void ValidateDomDefinition(cmc_domdefinition target, cmc_domdefinition preImage)
        {
            _tracer.Trace("Checking if DOM Master changed.");

            if (target.cmc_dommasterid?.Id == preImage.cmc_dommasterid?.Id)
            {
                _tracer.Trace("DOM Master did not change. Validation will not occur.");
                return;
            }

            if (target.cmc_dommasterid == null)
            {
                _tracer.Trace("DOM Master is null. Validation will not occur.");
                return;
            }

            _tracer.Trace("Querying for the DOM Master");
            var domMaster = _orgService.Retrieve<cmc_dommaster>(target.cmc_dommasterid,
                new ColumnSet("cmc_runassignmentforentity"));

            var entityName = DomAssignmentCommonService.RetrieveEntityNameForRunAssignmentForEntity(
                domMaster.cmc_runassignmentforentity?.Value);
            _tracer.Trace($"Run Assignment For Entity on DOM Master is {entityName}");

            if (string.IsNullOrEmpty(entityName))
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get(
                    "InvalidDOMDefinition_InvalidLogicsMessage"));
            }

            _tracer.Trace("Retrieving if any DOM Definition Logic records would be invalid on the DOM Definition.");
            var invalidRecord = _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch version='1.0' top='1'>
                  <entity name='cmc_domdefinitionlogic'>
                    <attribute name='cmc_domdefinitionlogicid' />
                    <order attribute='cmc_domdefinitionlogicname' descending='false' />
                    <filter type='and'>
                      <condition attribute='cmc_attributeschema' operator='not-like' value='{entityName}.%' />
                      <condition attribute='cmc_domdefinitionid' operator='eq' value='{target.cmc_domdefinitionId}' />
                    </filter>
                  </entity>
                </fetch>")).Entities.Count > 0;

            if (invalidRecord == true)
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get(
                    "InvalidDOMDefinition_InvalidLogicsMessage"));
            }

            _tracer.Trace("All records are valid.");
        }
        #endregion
    }
}
