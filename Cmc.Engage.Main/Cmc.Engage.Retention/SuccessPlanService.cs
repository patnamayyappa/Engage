using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;


namespace Cmc.Engage.Retention
{
    public class SuccessPlanService: ISuccessplanService
    {
        ILogger _loger;
        IDictionary<Guid, IEnumerable<cmc_scoringfactor>> _factorMap;
        EntityConditionEvaluator _contactEvaluator;
        int _createCount;
        int _updateCount;
        int _skippedCount;
        int _deactivatedCount;
        IDictionary<string, EntityMetadata> _entityMetadataMap;

        private IOrganizationService _orgService;
        private readonly ILanguageService _retrieveMultiLingualValues;

        public SuccessPlanService(ILogger tracer, IOrganizationService orgService, ILanguageService retrieveMultiLingualValues)
        {
            _loger = tracer ?? throw new ArgumentException(nameof(tracer));          
            _orgService = orgService;
            _retrieveMultiLingualValues = retrieveMultiLingualValues;
        }
        private readonly string[] _ignoreSystemAttributes =
        {
            "createdon", "createdby", "createdonbehalfby",
            "modifiedby", "modifiedon", "modifiedonbehalfby",
            "ownerid", "owningbusinessunit", "owningteam", "owninguser",
            "overridencreatedon", "timezoneruleversionnumber",
            "utcconversiontimezonecode", "versionnumber"
        };
        
        private readonly string[] _ignoreCustomAttributes =
        {
            "cmc_successplantemplateid", "cmc_successplantodotemplateid"
        };

        #region Copy Success Plan Template

        public void CopySuccessPlanTemplate(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateOrganizationServiceAsCurrentUser();

            _loger.Trace("Start CopySuccessPlanTemplatePlugin's Logic Plugin.");

            var target = pluginContext.GetInputParameter<Entity>("Target").ToEntity<cmc_successplantemplate>();

            CopySuccessPlanTemplateRecord(target);

            _loger.Trace("End CopySuccessPlanTemplatePlugin's Logic Plugin.");
        }
        private void CopySuccessPlanTemplateRecord(cmc_successplantemplate successPlanTemplate)
        {
            var copyFromSuccessPlanTemplateId = successPlanTemplate.cmc_copyfromsuccessplantemplateid?.Id;

            if (copyFromSuccessPlanTemplateId != null && copyFromSuccessPlanTemplateId != default(Guid))
            {
                _loger.Trace("Duplicating associated cmc_successplantodotemplate records.");
                DuplicateAssociatedSuccessPlanToDoTemplates((Guid)copyFromSuccessPlanTemplateId, successPlanTemplate);
            }
            else
            {
                _loger.Trace("Unable to copy cmc_successplantodotemplate records without valid cmc_copyfromsuccessplantemplate.");
            }
        }

        private void DuplicateAssociatedSuccessPlanToDoTemplates(Guid existingSuccessPlanTemplateId, Entity newSuccessPlanTemplate)
        {
            const string schemaName = "cmc_successplantodotemplate";
            var fetchSuccessPlanToDoTemplatesXml = $@"
                    <fetch>
                        <entity name='cmc_successplantodotemplate'>
                        <filter type='and'>
                            <condition attribute='cmc_successplantemplateid' operator='eq' value='{existingSuccessPlanTemplateId}' />
                        </filter>
                        </entity>
                    </fetch>";

            _loger.Trace($"Attempting to fetch associated {schemaName} records.");
            var fetchSuccessPlanToDoTemplateResults = _orgService.RetrieveMultipleAll(fetchSuccessPlanToDoTemplatesXml);

            if (fetchSuccessPlanToDoTemplateResults != null && fetchSuccessPlanToDoTemplateResults.Entities.Any())
            {
                _loger.Trace($"Found {fetchSuccessPlanToDoTemplateResults.Entities.Count} cmc_successplantodotemplate records to be associated.");

                foreach (var successPlanToDoTemplate in fetchSuccessPlanToDoTemplateResults.Entities)
                {
                    var newSuccessPlanToDoTemplate = new cmc_successplantodotemplate()
                    {
                        cmc_successplantemplateid = newSuccessPlanTemplate.ToEntityReference()
                    };

                    var attributesToCopy = RetrieveUpdateableFields(schemaName);

                    foreach (var attribute in attributesToCopy)
                    {
                        if (successPlanToDoTemplate.Contains(attribute))
                        {
                            newSuccessPlanToDoTemplate[attribute] = successPlanToDoTemplate[attribute];
                        }
                    }

                    var newSuccessPlanToDoTemplateId = _orgService.Create(newSuccessPlanToDoTemplate);
                    _loger.Trace($"Successfully created new cmc_successplantodotemplate with Id {newSuccessPlanToDoTemplateId}.");
                }
            }
            else
            {
                _loger.Trace($"No associated {schemaName} records found.");
            }
        }
        private IEnumerable<string> RetrieveUpdateableFields(string entityName)
        {
            MetadataFilterExpression entityFilter;

            entityFilter = new MetadataFilterExpression(Microsoft.Xrm.Sdk.Query.LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression(
                "LogicalName", MetadataConditionOperator.Equals, entityName));

            var attributeFilter = new MetadataFilterExpression(LogicalOperator.And);
            attributeFilter.Conditions.Add(new MetadataConditionExpression(
                "LogicalName", MetadataConditionOperator.NotIn, _ignoreSystemAttributes));
            attributeFilter.Conditions.Add(new MetadataConditionExpression(
                "LogicalName", MetadataConditionOperator.NotIn, _ignoreCustomAttributes));
            attributeFilter.Conditions.Add(new MetadataConditionExpression(
                "IsValidForCreate", MetadataConditionOperator.Equals, true));

            var response = _orgService.Execute(new RetrieveMetadataChangesRequest()
            {
                Query = new EntityQueryExpression()
                {
                    Criteria = entityFilter,
                    AttributeQuery = new AttributeQueryExpression()
                    {
                        Criteria = attributeFilter,
                        Properties = new MetadataPropertiesExpression("LogicalName")
                    },
                    LabelQuery = new LabelQueryExpression(),
                    Properties = new MetadataPropertiesExpression("Attributes")
                }
            }) as RetrieveMetadataChangesResponse;

            _loger.Trace($"Successfully retrieved updatable fields for {entityName}.");
            return response.EntityMetadata.FirstOrDefault().Attributes.Select(attr => attr.LogicalName);
        }

        #endregion

        #region Set Success Plan Status

        public void SetSuccessPlanStatus(Core.Xrm.ServerExtension.Core.IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            //_orgService = serviceProvider.CreateOrganizationServiceAsCurrentUser();
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            if (!pluginContext.IsValidCall("cmc_todo"))
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            var postImage = pluginContext.GetPostEntityImage<cmc_todo>("PostImage");
            UpdateSuccessPlan(postImage);
        }

        private void UpdateSuccessPlan(cmc_todo todo)
        {
            _loger.Trace("Update SuccessPlan method called");
            if (todo.cmc_successplanid == null)
            {
                _loger.Trace("Todo has no Success Plan.");
                return;
            }

            UpdateSuccessPlanStatus(todo);
        }

        private void UpdateSuccessPlanStatus(cmc_todo todo)
        {
            _loger.Trace("Update Success Plan Status method called");

            var listOfAssosiatedTodos = GeTAllTodosForSuccessPlan(todo.cmc_successplanid.Id);

            if (listOfAssosiatedTodos == null || !listOfAssosiatedTodos.Any()) return;

            var currentSuccessPlanstatuscode = GetCurrentSuccessplanStatuscode(listOfAssosiatedTodos.FirstOrDefault());
            if (currentSuccessPlanstatuscode == null) return;


            if (listOfAssosiatedTodos.ToList().Any(a => a.statuscode == cmc_todo_statuscode.Incomplete || a.statuscode == cmc_todo_statuscode.MarkedasComplete))
            {
                if (currentSuccessPlanstatuscode.Equals(cmc_successplan_statuscode.Active)) return;
                _loger.Trace("Success Plan Status Updated to Active");
                _orgService.Update(new cmc_successplan
                {
                    cmc_successplanId = todo.cmc_successplanid.Id,
                    statuscode = cmc_successplan_statuscode.Active,
                    statecode = cmc_successplanState.Active
                });
            }
            else if (listOfAssosiatedTodos.All(a =>
                     a.statuscode == cmc_todo_statuscode.Waived || a.statuscode == cmc_todo_statuscode.Canceled))
            {
                if (currentSuccessPlanstatuscode.Equals(cmc_successplan_statuscode.Canceled)) return;
                _loger.Trace("Success Plan Status Updated to Canceled");
                _orgService.Update(new cmc_successplan
                {
                    cmc_successplanId = todo.cmc_successplanid.Id,
                    statuscode = cmc_successplan_statuscode.Canceled,
                    statecode = cmc_successplanState.Inactive
                });
            }
            else
            {
                if (currentSuccessPlanstatuscode.Equals(cmc_successplan_statuscode.Completed)) return;
                _loger.Trace("Success Plan Status Updated to Completed");
                _orgService.Update(new cmc_successplan
                {
                    cmc_successplanId = todo.cmc_successplanid.Id,
                    statuscode = cmc_successplan_statuscode.Completed,
                    statecode = cmc_successplanState.Inactive
                });
            }
        }

        private cmc_successplan_statuscode? GetCurrentSuccessplanStatuscode(cmc_todo todo)
        {
            _loger.Trace("Get Successplan Statuscode method called");
            var statuscode = todo?.GetAliasedAttributeValue<OptionSetValue>("sp.statuscode");
            if (statuscode != null)
            {
                return (cmc_successplan_statuscode)statuscode.Value;
            }
            return null;
        }

        private List<cmc_todo> GeTAllTodosForSuccessPlan(Guid successplanid)
        {
            _loger.Trace("Get Todos For SuccessPlan method called");
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' > 
                            <entity name='cmc_todo' >
                                <attribute name='cmc_successplanid' />
                                <attribute name='statuscode' />
                                <filter type='and' >
                                    <condition attribute='cmc_successplanid' operator='eq' value='{successplanid}' />                                            
                                </filter>
                                <link-entity name = 'cmc_successplan' from ='cmc_successplanid' to ='cmc_successplanid' alias = 'sp' > 
                                <attribute name='statuscode' />
                                </link-entity>
                            </entity>
                         </fetch>";
            var todoList = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            var data = todoList.Entities.Count <= 0 ? null : todoList.Entities.Cast<cmc_todo>().ToList();
            return data;
        }

        #endregion
        
        
    }
}
