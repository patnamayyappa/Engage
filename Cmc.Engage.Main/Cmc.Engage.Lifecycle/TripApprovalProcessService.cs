using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    /// <summary>
    /// Service  for Trip Approval Process
    /// </summary>
    public class TripApprovalProcessService : ITripApprovalProcessService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _logger;
        private readonly IConfigurationService _configurationDetails;
        private const string stageCreateTrip = "d397ee00-1b33-4474-b689-daf68ea41db4";
        private const string stageReviewBeforeApproval = "ab919e2f-ad3f-4bcf-bb36-c805c36608ce";
        private const string stageSubmitForReview = "b09f2827-2f6e-42e0-88fb-36f29e3f3730";
        private const string stageReviewed = "ac029f02-2082-462f-875e-53003915917c";

        /// <summary>
        /// Constructor method for TripService 
        /// </summary>
        /// <param name="organizationService">organization service</param>
        /// <param name="logger">logger</param>
        public TripApprovalProcessService(ILogger logger, IOrganizationService organizationService, IConfigurationService configurationService)
        {
            _logger = logger;
            _orgService = organizationService;
            _configurationDetails = configurationService;
        }

        /// <summary>
        /// Execution of Trip Approval Process
        /// </summary>
        /// <param name="context">Execution context </param>
        public void TripApprovalProcessExecute(IExecutionContext context)
        {
            _logger.Info($"Entered into OnMoveNextStageSubmitForApproval");
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var targeTripApprovalProcess = pluginContext.GetTargetEntity<cmc_tripapprovalprocess>();
            _logger.Info($"Got target entity");
            OnMoveNextStage(targeTripApprovalProcess);
        }
        /// <summary>
        /// On Change Active Stage to Submit for approval
        /// </summary>
        /// <param name="targetTripApprovalProcess">Target Entity</param>
        public void OnMoveNextStage(cmc_tripapprovalprocess targetTripApprovalProcess)
        {
            _logger.Info($"Entered into OnMoveNextStage");
            if (targetTripApprovalProcess.ActiveStageId == null)
            {
                _logger.Info($"No active Stage to move ");
                return;
            }

            _logger.Info($"Active Stage Id : {targetTripApprovalProcess.ActiveStageId.Id}");
            _logger.Info($"TraversedPath : {targetTripApprovalProcess.TraversedPath}");
            
            if ((targetTripApprovalProcess.ActiveStageId.Id == new Guid(stageSubmitForReview)))
            {
                _logger.Info($"Active Stage Is SubmitForReview.");
                _logger.Info($"Getting workflow Configuration");
                cmc_configuration configuration = _configurationDetails.GetActiveConfiguration();
                _logger.Info($"Getting workflow Id");
                if (!configuration.Contains("cmc_tripapprovalassignworkflow") || configuration.cmc_tripapprovalassignworkflow == null)
                {
                    _logger.Error($"Workflow not founded in Configuration cmc_tripapprovalassignworkflow");
                    return;
                }
                Guid workFlowId = configuration.cmc_tripapprovalassignworkflow.Id;
                Guid tripId = targetTripApprovalProcess.bpf_cmc_tripid.Id;                
                _logger.Info($"Preparing  request for Execute Workflow");

                _logger.Info($"Creating trip entity for update trip status.");
                cmc_trip cmcTrip = new cmc_trip() {
                Id = tripId,
                cmc_Status=cmc_trip_cmc_status.SubmittedForReview
                };
                _logger.Info($"Updating trip entity for update trip status as SubmittedForReview.");
                _orgService.Update(cmcTrip);
                _logger.Info($"Updated trip entity for update trip status as SubmittedForReview.");
                ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
                {
                    WorkflowId = workFlowId,
                    EntityId = tripId
                };
                _logger.Info($"Executing Workflow : {configuration.cmc_tripapprovalassignworkflow.Name}");
                _orgService.Execute(request);
                _logger.Info($"Executed Workflow : {configuration.cmc_tripapprovalassignworkflow.Name}");
            }
            else if ((targetTripApprovalProcess.ActiveStageId.Id == new Guid(stageReviewed)))
            {
                _logger.Info($"Active Stage Is Reviewed.");
                _logger.Info($"Getting workflow Configuration");
                cmc_configuration configuration = _configurationDetails.GetActiveConfiguration();
                _logger.Info($"Getting workflow Id");
                if (!configuration.Contains("cmc_tripapprovalassignworkflow") || configuration.cmc_tripapprovalassignworkflow == null)
                {
                    _logger.Error($"Workflow not founded in Configuration cmc_tripapprovalassignworkflow");
                    return;
                }
                Guid workFlowId = configuration.cmc_tripapprovalassignworkflow.Id;
                Guid tripId = targetTripApprovalProcess.bpf_cmc_tripid.Id;
                _logger.Info($"Preparing  request for Execute Workflow");
                
                ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
                {
                    WorkflowId = workFlowId,
                    EntityId = tripId
                };
                _logger.Info($"Executing Workflow : {configuration.cmc_tripapprovalassignworkflow.Name}");
                _orgService.Execute(request);
                _logger.Info($"Executed Workflow : {configuration.cmc_tripapprovalassignworkflow.Name}");
            }

        }
    }
}


