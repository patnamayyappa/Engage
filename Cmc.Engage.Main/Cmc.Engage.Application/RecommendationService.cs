using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using Workflow = Cmc.Engage.Models.Workflow;
using Microsoft.Crm.Sdk.Messages;
using System;

namespace Cmc.Engage.Application
{
    public class RecommendationService : IRecommendationService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _logger;
        public RecommendationService(ILogger tracer, IOrganizationService orgService)
        {
            _logger = tracer;
            _orgService = orgService;
        }

        #region ApplicationRecomendationWorkflowPlugin
        public void SendThankyouEmail(IExecutionContext executionContext)
        {
            _logger.Trace("Starting SendThankyouEmail");
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            cmc_applicationrecommendation postImage = null;
            if (pluginContext.PostEntityImages.Contains("Target"))
            {
                _logger.Trace("Retrieving PostImage");
                postImage = pluginContext.GetPostEntityImage<cmc_applicationrecommendation>("Target");
                if (postImage.cmc_applicationrecommendationId != null)
                {
                    var recRecord = GetRecEmailWf(postImage.cmc_applicationid);
                    
                    if (recRecord != null )
                    {
                        var thankyouWorkflowId = recRecord.cmc_recommendationthankyouworkflow == null ? null : ((AliasedValue)recRecord.Attributes["thankyouWkflw.workflowid"]).Value;
                       
                        if (thankyouWorkflowId != null)
                        {
                            var stateCode = ((OptionSetValue)((AliasedValue)recRecord.Attributes["thankyouWkflw.statecode"]).Value).Value;
                            var statusCode = ((OptionSetValue)((AliasedValue)recRecord.Attributes["thankyouWkflw.statuscode"]).Value).Value;
                            _logger.Trace("Retrieved Workflow ID : " + thankyouWorkflowId.ToString());
                            if ((WorkflowState)stateCode == WorkflowState.Activated && (workflow_statuscode)statusCode == workflow_statuscode.Activated)
                            {
                                _logger.Trace("Retrieved Workflow Status : " + stateCode.ToString() + "--" + statusCode.ToString());
                                _orgService.Execute(new ExecuteWorkflowRequest
                                {
                                    EntityId = postImage.Id,
                                    WorkflowId = new Guid(thankyouWorkflowId.ToString())
                                });
                                _logger.Trace("Workflow Executed");
                            }
                            else
                            {
                                _logger.Trace("Workflow : " + recRecord.cmc_recommendationthankyouworkflow.Name + " is not active.");
                            }

                        }
                        else
                        {
                            _logger.Trace("Workflow not found.");
                        }
                    }

                  
                }
            }
            else
            {
                _logger.Trace("Failed to Retrieve PostImage");
            }

            _logger.Trace("Exiting SendThankyouEmail");
        }

        private cmc_applicationrecommendationdefinition GetRecEmailWf(EntityReference applicationId)
        {
            if (applicationId == null)
            {
                _logger.Trace("Application associated with recommendation is null. No email workflow will be retrieved.");
                return null;
            }
            _logger.Trace("Retrieving Recommendation definition for application: " + applicationId.Id );
            var appRecDefRecord = _orgService.RetrieveMultiple(new FetchExpression($@"
               <fetch version='1.0' output-format='xml-platform' mapping='logical'>
				  <entity name='cmc_applicationrecommendationdefinition' >
                    <attribute name='cmc_recommendationthankyouworkflow' />
					<link-entity name='workflow' from='workflowid' to='cmc_recommendationthankyouworkflow' link-type='outer' alias='thankyouWkflw' >
                        <attribute name='workflowid'/>   
                        <attribute name='statecode'/>
                        <attribute name='statuscode'/>
                    </link-entity>
					<link-entity name='cmc_applicationdefinition' from='cmc_recommendationdefinitionid' to='cmc_applicationrecommendationdefinitionid' link-type='inner' >
					  <link-entity name='cmc_applicationdefinitionversion' from='cmc_applicationdefinitionid' to='cmc_applicationdefinitionid' link-type='inner' >
						<link-entity name='cmc_applicationregistration' from='cmc_applicationdefinitionversionid' to='cmc_applicationdefinitionversionid' link-type='inner' >
						  <link-entity name='cmc_application' from='cmc_applicationregistration' to='cmc_applicationregistrationid' link-type='inner' >
							<filter>
							    <condition attribute='statecode' operator='eq' value='{(int)cmc_applicationState.Active}' />
                                <condition attribute='cmc_applicationid' operator='eq'  value='{applicationId.Id}' /> 
							</filter>
						  </link-entity>
						</link-entity>
					  </link-entity>
					</link-entity>
				  </entity>
				</fetch>")).Entities.Cast<cmc_applicationrecommendationdefinition>().ToList().FirstOrDefault();
            _logger.Trace("Retrieved Recommendation definition");
            return appRecDefRecord;
        }

       
        #endregion
    }
}
