using System;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;

using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    public class DomDefinitionExecutionOrderService : IDomDefinitionExecutionOrderService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;
        private readonly ILanguageService _retrieveMultiLingualValues;

        public DomDefinitionExecutionOrderService(ILogger tracer, ILanguageService retrieveMultiLingualValues, IOrganizationService orgService)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _retrieveMultiLingualValues = retrieveMultiLingualValues ?? throw new ArgumentException(nameof(retrieveMultiLingualValues));        
            _orgService = orgService;
        }

        #region Validate Dom Definition ExecutionOrder

        public void ValidateDomDefinitionExecutionOrder(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _tracer.Trace($"Start {nameof(ValidateDomDefinitionExecutionOrder)}");
            if (!pluginContext.IsValidCall(cmc_domdefinitionexecutionorder.EntityLogicalName))
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            }

            var target = pluginContext.GetTargetEntity<cmc_domdefinitionexecutionorder>();

            cmc_domdefinitionexecutionorder preImage = null;
            if (pluginContext.PreEntityImages.Contains("Target"))
            {
                preImage = pluginContext.GetPreEntityImage<cmc_domdefinitionexecutionorder>("Target");
            }

            ValidateDomDefinitionExecutionOrder(target, preImage, context);

        }

        private void ValidateDomDefinitionExecutionOrder(cmc_domdefinitionexecutionorder executionOrder,
           cmc_domdefinitionexecutionorder preImage, IExecutionContext context)
        {
            _tracer.Trace("Validating DOM Definition Execution Order Attribute - Schema is formatted as [Entity Name].[Attribute Name].");

            var attribute = executionOrder.GetValueOrFallback<string>(preImage, "cmc_attributeschema");

            // Perform all validation regardless of which fields updated
            DomAssignmentCommonService.ValidateAttributeFormat(attribute, _retrieveMultiLingualValues, "Invalid_Format_Execution_Order_Attribute");

            DomAssignmentCommonService.ValidateAttributeString(attribute,
                executionOrder.GetValueOrFallback<EntityReference>(preImage, "cmc_dommasterid"),
                _retrieveMultiLingualValues, _tracer, _orgService);
        }

        #endregion

    }
}
