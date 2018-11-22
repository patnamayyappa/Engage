using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Lifecycle.Messages;
using Cmc.Engage.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.WebServiceClient;
using MoreLinq;

namespace Cmc.Engage.Lifecycle
{
    public class DOMService : IDOMService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;
        private readonly ILanguageService _retrieveMultiLingualValues;
        public const int DomMasterRetrieveCount = 5000;
        private readonly IConfigurationService _retrieveConfigurationDetails;

        public DOMService(ILogger tracer, ILanguageService retrieveMultiLingualValues, IOrganizationService orgService, IConfigurationService retrieveConfigurationDetails)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _retrieveMultiLingualValues = retrieveMultiLingualValues ?? throw new ArgumentException(nameof(retrieveMultiLingualValues));
            _orgService = orgService ?? throw new ArgumentNullException(nameof(orgService));
            _retrieveConfigurationDetails = retrieveConfigurationDetails ?? throw new ArgumentNullException(nameof(retrieveConfigurationDetails));
        }

        #region Dom Azure Function

        public void ProcessDomAssignment(string EntityLogicalName,IOrganizationService organizationService)
        {

            var config = SetupConfiguration(EntityLogicalName);

            LogInfo($"Entered: {nameof(ProcessDomAssignment)}", config);

            try
            {
                ValidateLogicInput(config);
            }
            catch (Exception e)
            {
                LogError($"{config.EntityLogicalName}: Validation error with: {e.Message}", config, e);
                return;
            }


            List<Entity> domMasters = RetrieveDomMasterRecordsForEntityType(config);

            while (domMasters.Count() > 0)
            {
                UpdateListMembersPendingAssignmentCache(domMasters.ToList(), config, organizationService);
                ProcessDomMasters(domMasters, config, organizationService);
                domMasters = RetrieveDomMasterRecordsForEntityType(config);
            }
        }

        private DOMEngineConfig SetupConfiguration(string entityLogicalName)
        {
            var config = new DOMEngineConfig();

            config.UserLocalTime = DateHelper.RetrieveLocalTimeFromUtc(_orgService, config.UtcNow);
            config.EntityLogicalName = entityLogicalName;

            Entity postDomAssignmentConfig = _retrieveConfigurationDetails.GetActiveConfiguration();

            LogInfo($"Extracting value from Post DOM Assignment Configuration record", config);
            bool? configValue = postDomAssignmentConfig.GetAttributeValue<bool>("cmc_postdomassignment");
            LogInfo($"Config value: {configValue}", config);

            config.CreatePosts = configValue.HasValue && configValue.Equals(true);            

            config.DomMasterRetrievePage = 1;
            config.DomMasterRetrievePagingCookie = "";

            return config;
        }

        private void ProcessDomMasters(List<Entity> domMasters, DOMEngineConfig config, IOrganizationService organizationService)
        {
            if (domMasters.Count == 0)
            {
                LogInfo($"No DOM Masters related to {config.EntityLogicalName} marketing lists to process, exiting", config);
                return;
            }

            LogInfo("Finding unique DOM Masters and attribute names from query results", config);
            List<Entity> uniqueDomMasters = domMasters.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            var batchSize = 5;
            var domMasterBatch = uniqueDomMasters.Take(batchSize);
            var batch = 1;

            while (domMasterBatch.Count() > 0)
            {
                List<Guid> uniqueDomMasterIds = domMasterBatch.Select(x => x.Id).ToList();
                List<Entity> executionOrders = RetrieveExecutionOrdersRelatedToDomMasters(uniqueDomMasterIds, config);

                List<string> executionOrderAttributeNames = executionOrders.Select(x => x.GetAttributeValue<string>("cmc_attributeschema")).Distinct().ToList();
                List<Entity> definitionLogics = RetrieveDomDefinitionLogicsRelatedToDomMasters(uniqueDomMasterIds, executionOrderAttributeNames, config);

                foreach (Entity domMaster in domMasterBatch)
                {
                    if (!HasPendingAssignmentListMembers(domMaster.Id, config))
                    {
                        LogWarn($"DOM Master with id {domMaster.Id} has no pending assignment list members.", config);
                        continue;
                    }

                    var runAssignmentEntity = domMaster.GetAttributeValue<OptionSetValue>("cmc_runassignmentforentity").Value;
                    var listCreatedFromCode = domMaster.GetAliasedAttributeValue<OptionSetValue>("domMasterList.createdfromcode").Value;

                    if (runAssignmentEntity == (int)cmc_runassignmentforentity.Lifecycle)
                    {
                        if (listCreatedFromCode != (int)list_createdfromcode.Contact)
                        {
                            LogWarn($"Incorrect configuration for Lifecycle DOM Master. Marketing List must be Contact.", config);
                            continue;
                        }
                    }
                    LogInfo($"Processing DOM master with Id: {domMaster.Id}", config);

                    ProcessExecutionOrders(domMaster, definitionLogics
                            .Where(x => x.GetAliasedAttributeValue<EntityReference>("domDefinition.cmc_dommasterid").Id == domMaster.Id)
                            .ToList()
                        , executionOrders
                            .Where(x => x.GetAttributeValue<EntityReference>("cmc_dommasterid").Id == domMaster.Id)
                            .ToList(), config, organizationService);
                }

                domMasterBatch = uniqueDomMasters.Skip(batch * batchSize).Take(batchSize);
                batch += 1;
            }
        }

        private void ProcessExecutionOrders(Entity domMaster, List<Entity> definitionLogics, List<Entity> executionOrders, DOMEngineConfig config,
            IOrganizationService organizationService)
        {

            if (executionOrders.Count() == 0)
            {
                LogWarn($"No execution orders for DOM Master, skipping.", config);
                return;
            }

            int? attributeNameToBeAssignedValue = domMaster.GetAttributeValue<OptionSetValue>("cmc_attributenametobeassigned")?.Value;
            string attributeNameToBeAssigned = "ownerid";

            if (attributeNameToBeAssignedValue == (int)cmc_attributenametobeassigned.OtherUserLookup)
            {
                string otherUserAttribute = domMaster.GetAttributeValue<string>("cmc_otheruserlookup").ExtractActualDomAttribute();

                if (string.IsNullOrEmpty(otherUserAttribute))
                {
                    LogWarn($"Attribute name to be assigned is set to OtherUserLookup, but other user lookup text field is null, skipping Dom Master with Id: {domMaster.Id}", config);
                    return;
                }

                attributeNameToBeAssigned = otherUserAttribute;
            }

            LogInfo($"{executionOrders.Count} DOM Execution Orders to process for Master", config);

            foreach (Entity executionOrder in executionOrders)
            {
                ProcessExecutionOrder(attributeNameToBeAssigned, domMaster, definitionLogics, executionOrder, config, organizationService);
                if (!HasPendingAssignmentListMembers(domMaster.Id, config))
                {
                    break;
                }
            }

            LogInfo("Looping through all Dom Master list members with status of Pending Assignment again to update status and set fall back user", config);

            List<Entity> pendingAssignmentMembers = config.DomMasterListMembersPendingAssignment[domMaster.Id]
                .Select(listMemberId => new Entity()
                {
                    Id = listMemberId,
                    LogicalName = config.EntityLogicalName,
                }).ToList();

            EntityReference fallbackUserId = domMaster.GetAttributeValue<EntityReference>("cmc_fallbackuserid");

            if (pendingAssignmentMembers == null)
            {
                LogInfo("No pending assignment list members found that meet the conditions specified on the DOM Definition Logic", config);
                return;
            }

            AssignListMembersToUser(attributeNameToBeAssigned, null, domMaster, pendingAssignmentMembers, fallbackUserId, config);
        }

        private void ProcessExecutionOrder(string attributeName, Entity domMaster, List<Entity> definitionLogics, Entity executionOrder, DOMEngineConfig config,
            IOrganizationService organizationService)
        {
            LogInfo($"Processing Execution Order with ID: {executionOrder.Id}", config);

            var isDynamicList = domMaster.GetAliasedAttributeValue<bool>("domMasterList.type");

            List<Entity> logicsMatchingOrderAttribute = GetLogicThatMatchesOrderAttributeAndDomMaster(definitionLogics, executionOrder, domMaster.Id, config);

            if (logicsMatchingOrderAttribute == null)
            {
                return;
            }

            foreach (Entity definitionLogic in logicsMatchingOrderAttribute)
            {
                ProcessDefinitionLogic(attributeName, domMaster, definitionLogic, config, organizationService);
                if (!HasPendingAssignmentListMembers(domMaster.Id, config))
                {
                    break;
                }
            }
            return;
        }

        private void ProcessDefinitionLogic(string attributeName, Entity domMaster, Entity definitionLogic, DOMEngineConfig config, IOrganizationService organizationService)
        {
            LogInfo($"Processing DOM Defintion Logic with ID: {definitionLogic.Id}", config);
            List<Entity> listMembers = RetrievePendingAssignmentListMembers(domMaster, definitionLogic, true, config, organizationService);

            if (listMembers == null)
            {
                LogInfo("No pending assignment list members found that meet the conditions specified on the DOM Definition Logic", config);
                return;
            }

            EntityReference assignedUser = definitionLogic.GetAliasedAttributeValue<EntityReference>("domDefinition.cmc_domdefinitionforid");
            string attributeMatchedOn = definitionLogic.GetAttributeValue<string>("cmc_attribute");

            AssignListMembersToUser(attributeName, attributeMatchedOn, domMaster, listMembers, assignedUser, config);
            return;
        }

        private void AssignListMembersToUser(string assignAttribute, string matchedAttribute, Entity domMaster, List<Entity> listMembers, EntityReference assignedUser,
            DOMEngineConfig config)
        {
            var requests = new List<OrganizationRequest>();

            if (matchedAttribute == null)
            {
                LogInfo($"{listMembers?.Count} members found that still have a status of Pending Assignement. Updating status to Assignment Complete", config);
            }
            foreach (Entity member in listMembers)
            {
                Guid memberId = RetrieveMemberId(member);

                Entity updateRecord = new Entity(config.EntityLogicalName) { Id = memberId };
                updateRecord[assignAttribute] = assignedUser;
                updateRecord["cmc_domstatus"] = new OptionSetValue((int)cmc_domstatus.AssignmentComplete);
                LogInfo($"Assigning record to user with Id: {assignedUser?.Id}", config);

                requests.Add(new UpdateRequest()
                {
                    Target = updateRecord
                });

            }

            var response = BatchExecute(requests, config);

            for (var i = 0; i < response.Count(); i++)
            {
                var responseSucceeded = response[i];
                var memberId = RetrieveMemberId(listMembers[i]);

                if (responseSucceeded)
                {
                    RemovePendingAssignmentListMemberFromCache(memberId, config);
                }
                else
                {
                    LogError($"Error assigning record with id = {memberId}", config);
                }
            }

            CreatePosts(assignedUser, response, listMembers, assignAttribute, matchedAttribute, config);
        }

        private List<bool> BatchExecute(List<OrganizationRequest> orgRequests,
            DOMEngineConfig config)
        {
            var batchSize = 100;
            var responseSuccessLookup = new List<bool>(new bool[orgRequests.Count()]);

            var currentBatch = orgRequests.Take(batchSize).ToList();
            int loop = 0;

            while (currentBatch.Count() > 0)
            {
                var requests = new OrganizationRequestCollection();
                requests.AddRange(currentBatch);

                ExecuteMultipleRequest executeMultipleRequest = new ExecuteMultipleRequest();
                executeMultipleRequest.Requests = requests;
                executeMultipleRequest.Settings = new ExecuteMultipleSettings();
                executeMultipleRequest.Settings.ContinueOnError = true;
                executeMultipleRequest.Settings.ReturnResponses = true;

                ExecuteMultipleResponse responses = null;

                try
                {
                    responses = (ExecuteMultipleResponse)_orgService.Execute(executeMultipleRequest);
                }
                catch (Exception e)
                {
                    LogError($"Error making request: {e.Message}", config, e);
                    return null;
                }
                foreach (var r in responses.Responses)
                {
                    var requestSucceeded = (r.Fault == null);
                    responseSuccessLookup[orgRequests.IndexOf(currentBatch[r.RequestIndex])] = requestSucceeded;
                    if (!requestSucceeded)
                    {
                        LogError($"Error making request: {r.Fault}", config);
                    }
                }
                loop++;
                currentBatch = orgRequests.Skip(batchSize * loop).Take(batchSize).ToList();
            }

            return responseSuccessLookup;
        }

        private void ValidateLogicInput(DOMEngineConfig config)
        {
            LogInfo($"Entered: {nameof(ValidateLogicInput)}", config);
            LogInfo($"Entity logical name: {config.EntityLogicalName}", config);

            if (!Constants.ValidEntityLogicalNames.Any(logicalName => config.EntityLogicalName.Equals(logicalName, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new Exception(ErrorMessages.InvalidEntityTypeErrorMessage);
            }
        }
        
        private List<Entity> RetrieveDomMasterRecordsForEntityType(DOMEngineConfig config)
        {
            LogInfo($"Entered: {nameof(RetrieveDomMasterRecordsForEntityType)}", config);

            LogInfo($"Retrieving DOM Masters related to marketing list of type: {config.EntityLogicalName}", config);

            int entityOptionSetValue;

            switch (config.EntityLogicalName)
            {
                case Account.EntityLogicalName:
                    entityOptionSetValue = (int)cmc_runassignmentforentity.Account;
                    break;
                case Contact.EntityLogicalName:
                    entityOptionSetValue = (int)cmc_runassignmentforentity.Contact;
                    break;
                case Opportunity.EntityLogicalName:
                    entityOptionSetValue = (int)cmc_runassignmentforentity.Lifecycle;
                    break;
                case Lead.EntityLogicalName:
                    entityOptionSetValue = (int)cmc_runassignmentforentity.InboundInterest;
                    break;
                default:
                    LogInfo($@"Invalid entity {config.EntityLogicalName}", config);
                    return null;
            }
            try
            {
                var response = _orgService.RetrieveMultiple(new FetchExpression($@"
                    <fetch distinct='true' count='{DomMasterRetrieveCount}' page='{config.DomMasterRetrievePage}' paging-cookie='{config.DomMasterRetrievePagingCookie}'>
                        <entity name='cmc_dommaster'>
                            <attribute name='cmc_dommastername'/>
                            <attribute name='cmc_attributenametobeassigned'/>
                            <attribute name='cmc_otheruserlookup'/>
                            <attribute name='cmc_fallbackuserid'/>
                            <attribute name='cmc_marketinglistid'/>
                            <attribute name='cmc_runassignmentforentity' />
                            <attribute name='cmc_dommasterid' />
                            <filter type='and'>
                                <condition attribute='statecode' operator='eq' value='0' />
                                <condition attribute='cmc_runassignmentforentity' operator='eq' value='{entityOptionSetValue}' />
                            </filter>
                            <link-entity name='list' from='listid' to='cmc_marketinglistid' link-type='inner' alias='domMasterList' >
                                <attribute name='type' />
                                <attribute name='query' />
                                <attribute name='ownerid' />
                                <attribute name='createdfromcode' />
                            </link-entity>
                            <link-entity name='cmc_domdefinitionexecutionorder' from='cmc_dommasterid' to='cmc_dommasterid' link-type='inner'>
                                <filter type='and'>
                                    <condition attribute='statecode' operator='eq' value='0' />
                                </filter>
                            </link-entity>
                        </entity>
                    </fetch>"));

                config.DomMasterRetrievePagingCookie = HttpUtility.HtmlEncode(response.PagingCookie);
                config.DomMasterRetrievePage += 1;

                return response.Entities.ToList();
            }
            catch (Exception e)
            {
                LogError($"Error retrieving DOM Masters: {e.Message}", config, e);
                return new List<Entity>();
            }
        }

        private List<Entity> RetrieveExecutionOrdersRelatedToDomMasters(List<Guid> domMasterIds,
            DOMEngineConfig config)
        {
            LogInfo($"Entered: {nameof(RetrieveExecutionOrdersRelatedToDomMasters)}", config);

            LogInfo("Formatting fetch In conditions for unique DOM Masters and attribute names", config);
            string domMasterInFetchCondition = String.Join("</value><value>", domMasterIds);

            LogInfo("Retrieving DOM Execution Orders related to DOM Masters", config);

            try
            {

                var response = _orgService.RetrieveMultipleAll($@"
                    <fetch>
                        <entity name='cmc_domdefinitionexecutionorder'>
                            <attribute name='cmc_domdefinitionexecutionorderid'/>
                            <attribute name='cmc_attributeschema'/>
                            <attribute name='cmc_order'/>
                            <attribute name='cmc_dommasterid'/>
                            <order attribute='cmc_order' />
                            <filter type='and'>
                                <condition attribute='statecode' operator='eq' value='0' />
                                <condition attribute='cmc_dommasterid' operator='in'><value>{domMasterInFetchCondition}</value></condition>
                            </filter>
                        </entity>
                    </fetch>").Entities.ToList();
                return response;
            }
            catch (Exception e)
            {
                LogError($"Error retrieving ExecutionOrders related to DomMasters : {e.Message}", config, e);
                return new List<Entity>();
            }
        }

        private List<Entity> RetrieveDomDefinitionLogicsRelatedToDomMasters(List<Guid> domMasterIds, List<string> allAttributeNames,
            DOMEngineConfig config)
        {
            LogInfo($"Entered: {nameof(RetrieveDomDefinitionLogicsRelatedToDomMasters)}", config);

            LogInfo("Formatting fetch In conditions for unique DOM Masters and attribute names", config);

            string domMasterInFetchCondition = String.Join("</value><value>", domMasterIds);
            string attributeNamesInFetchCondition = String.Join("</value><value>", allAttributeNames);

            LogInfo("Retrieving DOM Definition Logics related to DOM Masters that also have an attribute name equal to an attribute name in one of the retrieved Execution Orders", config);

            try
            {

                var response = _orgService.RetrieveMultipleAll($@"
                    <fetch>
                        <entity name='cmc_domdefinitionlogic'>
                            <attribute name='cmc_attributeschema'/>
                            <attribute name='cmc_attribute'/>
                            <attribute name='cmc_conditiontype'/>
                            <attribute name='cmc_value'/>
                            <attribute name='cmc_minimum'/>
                            <attribute name='cmc_maximum'/>
                            <filter type='and'>
                                <condition attribute='statecode' operator='eq' value='0' />
                                <condition attribute='cmc_conditiontype' operator='not-null' />
                                <condition attribute='cmc_attributeschema' operator='in'><value>{attributeNamesInFetchCondition}</value></condition>
                            </filter>               
                            <link-entity name='cmc_domdefinition' from='cmc_domdefinitionid' to='cmc_domdefinitionid' link-type='inner' alias='domDefinition' >
                                <attribute name='cmc_dommasterid'/>
                                <attribute name='cmc_domdefinitionforid'/>
                                <filter type='and'>
                                    <condition attribute='statecode' operator='eq' value='0' />
                                    <condition attribute='cmc_dommasterid' operator='in'><value>{domMasterInFetchCondition}</value></condition>
                                </filter>
                            </link-entity>
                            <link-entity name='usersettings' from='systemuserid' to='ownerid' alias='usersettingsalias' link-type='outer'>
                                <link-entity name='timezonedefinition' to='timezonecode' from='timezonecode' alias='timezonedefinitionalias' link-type='outer'>
                                    <attribute name='standardname' />
                                </link-entity>
                            </link-entity>
                        </entity>
                    </fetch>").Entities.ToList();
                return response;
            }
            catch (Exception e)
            {
                LogError($"Error retrieving DomDefinitionLogics related to DomMasters : {e.Message}", config, e);
                return null;
            }
        }

        private List<Entity> GetLogicThatMatchesOrderAttributeAndDomMaster(List<Entity> definitionLogics, Entity aliasedExecutionOrder, Guid domMasterId,
            DOMEngineConfig config)
        {
            string orderAttribute = aliasedExecutionOrder.GetAttributeValue<string>("cmc_attributeschema");

            if (string.IsNullOrEmpty(orderAttribute))
            {
                LogWarn("Order Attribute is null", config);
                return null;
            }

            var matchingLogics = definitionLogics.Where(logic =>
            {
                string logicAttribute = logic.GetAttributeValue<string>("cmc_attributeschema");
                return orderAttribute.Equals(logicAttribute, StringComparison.InvariantCultureIgnoreCase);
            }).ToList();

            if (matchingLogics.Count == 0)
            {
                Guid executionOrderId = aliasedExecutionOrder.GetAliasedAttributeValue<Guid>("domDefinitionExecutionOrder.cmc_domdefinitionexecutionorderid");
                LogWarn($"Could not find matching DOM Execution Logic record for Execution Order with ID: {executionOrderId} with attribute {orderAttribute} for DOM Master with ID: {domMasterId}", config);
                return null;
            }

            return matchingLogics;
        }

        private List<Entity> RetrievePendingAssignmentListMembers(Entity domMaster, Entity domDefinitionLogic, bool useConditions, DOMEngineConfig config, IOrganizationService organizationService)
        {
            QueryExpression query = null;
            Guid impersonatingUserId = domMaster.GetAliasedAttributeValue<EntityReference>("domMasterList.ownerid").Id;

            if (useConditions)
            {
                string attributeSchema = domDefinitionLogic.GetAttributeValue<string>("cmc_attributeschema");
                string attribute = attributeSchema?.ExtractActualDomAttribute();
                List<EntityRelationship> relatedEntities = DomAssignmentCommonService.GetEntityChainForAttributeSchemaString(attributeSchema, _retrieveMultiLingualValues, _tracer, _orgService);

                if (string.IsNullOrEmpty(attribute))
                {
                    LogError($"Invalid attribute name for DOM Definition Logic with Id: {domDefinitionLogic.Id}, skipping retrieve of list members", config);
                    return null;
                }

                // Get the domDefinitionLogic owner's timezone
                var timezoneCode = domDefinitionLogic.GetAliasedAttributeValue<string>("timezonedefinitionalias.standardname");
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
                try
                {
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneCode);
                }
                catch (TimeZoneNotFoundException e)
                {
                    LogError($@"Error: Could not get user time zone: {e.Message}", config, e);
                }

                var domDefinitionFor = domDefinitionLogic.GetAliasedAttributeValue<EntityReference>("domDefinition.cmc_domdefinitionforid");
                query = BuildMarketingListQuery(domMaster, attribute, relatedEntities, domDefinitionLogic, domDefinitionFor, timeZoneInfo, config);
            }
            else
            {
                query = GetBaseQuery(domMaster, config);
            }
            try
            {
                SetCallerId(impersonatingUserId, organizationService);
                var response = organizationService.RetrieveMultipleAll(query).Entities.ToList();
                SetCallerId(Guid.Empty, organizationService);
                return response;
            }
            catch (Exception e)
            {
                LogError($"Error retrieving pending assigment list members: {e.Message}", config, e);
                return null;
            }
        }


		public List<EntityRelationship> GetEntityChainForAttributeSchemaString(string schemaString)
		{

			return DomAssignmentCommonService.GetEntityChainForAttributeSchemaString(schemaString, _retrieveMultiLingualValues, _tracer, _orgService);
		}
		private void UpdateListMembersPendingAssignmentCache(List<Entity> domMasters, DOMEngineConfig config, IOrganizationService organizationService)
        {
            config.DomMasterListMembersPendingAssignment = new Dictionary<Guid, List<Guid>>();

            foreach (var domMaster in domMasters)
            {
                var pendingListMembers = RetrievePendingAssignmentListMembers(domMaster, null, false, config, organizationService)
                    .Select(member => member.Id);
                config.DomMasterListMembersPendingAssignment[domMaster.Id] = pendingListMembers.ToList();
            }
        }

        private void RemovePendingAssignmentListMemberFromCache(Guid listMemberId, DOMEngineConfig config)
        {
            foreach (var domMasterId in config.DomMasterListMembersPendingAssignment.Keys)
            {
                config.DomMasterListMembersPendingAssignment[domMasterId].Remove(listMemberId);
            }
        }

        private bool HasPendingAssignmentListMembers(Guid domMasterId, DOMEngineConfig config)
        {
            if (config.DomMasterListMembersPendingAssignment.ContainsKey(domMasterId))
            {
                return config.DomMasterListMembersPendingAssignment[domMasterId].Count > 0;
            }

            return false;
        }
        /// <summary>
        /// The Organization Service instance retrieved from the DI is a static instance and shared across all Azure functions.
        /// In cases where we would need to impersonate the user, we need to perform the operations using a new instance of Organization Service,
        /// so that it does not impact the other Azure functions.
        /// The newly created Organization Service can be used to set the Caller Id to the user to whom to impersonate. 
        /// </summary>
        /// <param name="callerId"></param>
        /// <param name="organizationService"></param>
        private void SetCallerId(Guid callerId, IOrganizationService organizationService)
        {
            switch (organizationService)
            {
                case OrganizationServiceProxy _:
                    ((OrganizationServiceProxy)organizationService).CallerId = callerId;
                    break;
                case OrganizationWebProxyClient _:
                    ((OrganizationWebProxyClient)organizationService).CallerId = callerId;
                    break;
            }
        }

        private QueryExpression BuildMarketingListQuery(Entity domMaster, string attribute,
            List<EntityRelationship> relatedEntities, Entity domDefinitionLogic, EntityReference domDefinitionForUser, TimeZoneInfo timeZone,
            DOMEngineConfig config)
        {

            QueryExpression query = GetBaseQuery(domMaster, config);

            var linkEntitiesBase = query.LinkEntities;
            var linkCriteriaBase = query.Criteria;

            int conditionType = domDefinitionLogic.GetAttributeValue<OptionSetValue>("cmc_conditiontype").Value;
            var uncastedValue = domDefinitionLogic.GetAttributeValue<string>("cmc_value");

            string parentEntityFetchCondition = string.Empty;

            List<LinkEntity> linkEntities = new List<LinkEntity>();
            List<ConditionExpression> conditionExpressions = new List<ConditionExpression>();
            List<FilterExpression> queryBaseFilterExpressions = new List<FilterExpression>();

            string attributeType = null;
            var attributeEntityName = config.EntityLogicalName;
            try
            {
                if (relatedEntities.Count() > 0)
                {
                    attributeEntityName = relatedEntities.Last().TargetEntity;
                }
                attributeType = MetadataHelper.GetAttributeType(_orgService, _tracer, attributeEntityName, attribute);
            }
            catch (Exception e)
            {
                LogError($"Error fetching metadata: {e.Message}", config, e);
            }

            switch (conditionType)
            {
                case (int)cmc_domconditiontype.BeginsWith:
                    if (MetadataHelper.AttributeTypeIsLookup(attributeType))
                    {
                        var lookupExpression = GetLinkEntityAndConditionExpressionForLookupCondition(attributeEntityName, 
                            attribute, (string)uncastedValue, ConditionOperator.Like, config);

                        // Since the lookup could have been to more than one entity, add a new link for each.
                        linkEntities.AddRange(lookupExpression.LinkEntities);

                        // Filter to make sure that at least one of the linked entities actually has a value
                        queryBaseFilterExpressions.Add(lookupExpression.FilterExpression);
                    }
                    else
                    {
                        conditionExpressions.Add(GetConditionExpressionForAttribute(attributeEntityName, 
                            attribute, attributeType, (string)uncastedValue, ConditionOperator.Like, timeZone, config));
                    }
                    break;
                case (int)cmc_domconditiontype.Equals:
                    if (MetadataHelper.AttributeTypeIsLookup(attributeType))
                    {
                        var lookupExpression = GetLinkEntityAndConditionExpressionForLookupCondition(attributeEntityName, attribute, (string)uncastedValue, ConditionOperator.Equal, config);
                        linkEntities.AddRange(lookupExpression.LinkEntities);
                        queryBaseFilterExpressions.Add(lookupExpression.FilterExpression);
                    }
                    else
                    {
                        conditionExpressions.Add(GetConditionExpressionForAttribute(attributeEntityName, attribute, attributeType, (string)uncastedValue, ConditionOperator.Equal, timeZone, config));
                    }
                    break;

                case (int)cmc_domconditiontype.IsNull:
                    conditionExpressions.Add(GetConditionExpressionForAttribute(attributeEntityName, attribute, attributeType, (string)uncastedValue, ConditionOperator.Null, timeZone, config));
                    break;

                case (int)cmc_domconditiontype.IsNotNull:
                    conditionExpressions.Add(GetConditionExpressionForAttribute(attributeEntityName, attribute, attributeType, (string)uncastedValue, ConditionOperator.NotNull, timeZone, config));
                    break;

                case (int)cmc_domconditiontype.Range:
                    string min = domDefinitionLogic.GetAttributeValue<string>("cmc_minimum");
                    string max = domDefinitionLogic.GetAttributeValue<string>("cmc_maximum");
                    string dateTimeBehavior = "";

                    ConditionOperator minOperator = ConditionOperator.GreaterEqual;
                    ConditionOperator maxOperator = ConditionOperator.LessEqual;

                    object minValue = min;
                    object maxValue = max;

                    if (attributeType == Constants.DateTimeAttributeType)
                    {
                        dateTimeBehavior = MetadataHelper.GetAttributeDateTimeBehavior(_orgService, _tracer, attributeEntityName, attribute);

                        DateTime? minUtcDateTime = MetadataHelper.ParseStringDateTimeAndConvertToUTC((string)min, timeZone, dateTimeBehavior);
                        DateTime? maxUtcDateTime = MetadataHelper.ParseStringDateTimeAndConvertToUTC((string)max, timeZone, dateTimeBehavior);

                        if (minUtcDateTime == null || maxUtcDateTime == null)
                        {
                            return null;
                        }
                        else
                        {
                            minValue = minUtcDateTime;
                            maxValue = maxUtcDateTime;
                            minOperator = MetadataHelper.GetConditionOperatorForDateTimeString(min, true, true);
                            maxOperator = MetadataHelper.GetConditionOperatorForDateTimeString(max, true, false);
                        }
                    }

                    conditionExpressions.Add(new ConditionExpression(attribute, minOperator, minValue));
                    conditionExpressions.Add(new ConditionExpression(attribute, maxOperator, maxValue));

                    break;

                case (int)cmc_domconditiontype.RecordOwner:

                    string lookupTargetEntity = null;
                    string targetEntityPrimaryKey = null;

                    try
                    {
                        var targets = MetadataHelper.GetAttributeTargetEntity(_orgService, _tracer, attributeEntityName, attribute);
                        lookupTargetEntity = targets != null && targets.Length > 0 ? targets[0] : null;

                        if (string.IsNullOrEmpty(lookupTargetEntity))
                        {
                            return null;
                        }

                        targetEntityPrimaryKey = MetadataHelper.GetEntityPrimaryKey(_orgService, _tracer, lookupTargetEntity);

                        if (string.IsNullOrEmpty(targetEntityPrimaryKey))
                        {
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        LogError($"Error fetching metadata: {e.Message}", config, e);
                    }

                    LinkEntity link = new LinkEntity(attributeEntityName, lookupTargetEntity, attribute, targetEntityPrimaryKey, JoinOperator.Inner);
                    link.EntityAlias = "lookupAttributeRecord";
                    link.Columns.AddColumn(targetEntityPrimaryKey);
                    link.LinkCriteria.AddCondition("ownerid", ConditionOperator.Equal, domDefinitionForUser?.Id);
                    linkEntities.Add(link);
                    break;

                default:
                    break;
            }

            if (query == null || (conditionExpressions.Count() == 0 && linkEntities.Count() == 0))
            {
                return null;
            }

            foreach (var filterExpression in queryBaseFilterExpressions)
            {
                query.Criteria.AddFilter(filterExpression);
            }

            foreach (var relationship in relatedEntities)
            {
                var linkEntity = new LinkEntity()
                {
                    LinkToEntityName = relationship.TargetEntity,
                    LinkFromEntityName = relationship.SourceEntity,
                    LinkToAttributeName = relationship.TargetAttribute,
                    LinkFromAttributeName = relationship.SourceAttribute
                };

                linkEntitiesBase.Add(linkEntity);
                linkEntitiesBase = linkEntity.LinkEntities;
                linkCriteriaBase = linkEntity.LinkCriteria;
            }

            foreach (var condition in conditionExpressions)
            {
                linkCriteriaBase.AddCondition(condition);
            }

            foreach (var linkEntity in linkEntities)
            {
                // Only add it to the base because the link entities use aliases for the conditions
                linkEntitiesBase.Add(linkEntity);
            }

            return query;
        }

        private QueryExpression GetBaseQuery(Entity domMaster, DOMEngineConfig config)
        {

            var listMemberTypeAndLookupField = RetrieveListMemberEntityTypeAndLookupField(config);
            string listMemberEntityType = listMemberTypeAndLookupField.ListMemberEntityType;
            string listMemberLookupField = listMemberTypeAndLookupField.ListMemberLookupField;

            QueryExpression query = null;

            string fetch = domMaster.GetAliasedAttributeValue<string>("domMasterList.query");
            Guid marketingListId = domMaster.GetAttributeValue<EntityReference>("cmc_marketinglistid").Id;

            if (fetch == null)
            {
                query = new QueryExpression()
                {
                    EntityName = config.EntityLogicalName,
                    ColumnSet = new ColumnSet(new string[] { config.EntityLogicalName + "id" }),
                    Distinct = true,
                };

                var listMemberLink = new LinkEntity(config.EntityLogicalName, "listmember", listMemberEntityType + "id", "entityid", JoinOperator.Inner);
                listMemberLink.LinkCriteria.AddCondition(new ConditionExpression("listid", ConditionOperator.Equal, marketingListId));


                if (listMemberEntityType != config.EntityLogicalName)
                {
                    var parentLink = new LinkEntity(config.EntityLogicalName, listMemberEntityType, listMemberLookupField, listMemberEntityType + "id", JoinOperator.Inner);
                    listMemberLink.LinkFromEntityName = listMemberEntityType;
                    parentLink.LinkEntities.Add(listMemberLink);
                    query.LinkEntities.Add(parentLink);
                }
                else
                {
                    query.LinkEntities.Add(listMemberLink);
                }
            }
            else
            {
                try
                {
                    query = ((FetchXmlToQueryExpressionResponse)_orgService.Execute(new FetchXmlToQueryExpressionRequest()
                    {
                        FetchXml = fetch
                    })).Query;
                    query.Distinct = true;
                    if (config.EntityLogicalName == Opportunity.EntityLogicalName)
                    {
                        query = FlipContactBaseExpression(query, config);
                    }
                }

                catch (Exception e)
                {
                    LogError($"Error fetching query: {e.Message}", config, e);
                    return null;
                }
            }

            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));

            query?.Criteria.AddCondition("cmc_domstatus", ConditionOperator.Equal,
                    (int)cmc_domstatus.PendingAssignment);

            return query;
        }

        private QueryExpression FlipContactBaseExpression(QueryExpression contactBaseExpression, DOMEngineConfig config)
        {
            // Flip a query with contact as base and lifecycle as linked entity
            // Result should be a query with lifecycle base and contact as linked entity

            LinkEntity lifecycleLink = RetrieveOrBuildSubEntityExpression(contactBaseExpression, config);

            // Create new expression
            QueryExpression lifecycleBaseExpression = new QueryExpression(lifecycleLink.LinkToEntityName);

            lifecycleBaseExpression.Distinct = contactBaseExpression.Distinct;
            lifecycleBaseExpression.TopCount = contactBaseExpression.TopCount;
            lifecycleBaseExpression.PageInfo = contactBaseExpression.PageInfo;
            lifecycleBaseExpression.NoLock = contactBaseExpression.NoLock;

            lifecycleBaseExpression.ColumnSet.Columns.AddRange(lifecycleLink.Columns.Columns);
            lifecycleBaseExpression.Criteria = lifecycleLink.LinkCriteria;

            // Create contact link entity
            LinkEntity contactLink = new LinkEntity(lifecycleLink.LinkToEntityName, lifecycleLink.LinkFromEntityName, lifecycleLink.LinkToAttributeName, lifecycleLink.LinkFromAttributeName, lifecycleLink.JoinOperator);
            contactBaseExpression.Orders.AddRange(contactLink.Orders);

            contactLink.LinkCriteria = contactBaseExpression.Criteria;

            contactLink.Columns.Columns.AddRange(contactBaseExpression.ColumnSet.Columns);

            foreach (var linkEntity in contactBaseExpression.LinkEntities)
            {
                if (linkEntity != lifecycleLink)
                {
                    contactLink.LinkEntities.Add(linkEntity);
                }
            }

            lifecycleBaseExpression.LinkEntities.Add(contactLink);

            return lifecycleBaseExpression;
        }

        private QueryMultipleLookupConditions GetLinkEntityAndConditionExpressionForLookupCondition(string entityName, string attribute, string conditionValue, ConditionOperator conditionOperator,
            DOMEngineConfig config)
        {
            try
            {
                var targets = MetadataHelper.GetAttributeTargetEntity(_orgService, _tracer, entityName, attribute);
                List<LinkEntity> linkEntities = new List<LinkEntity>();
                List<ConditionExpression> conditionExpressions = new List<ConditionExpression>();
                foreach (var targetEntity in targets)
                {
                    string targetEntityPrimaryName = null;
                    try
                    {
                        targetEntityPrimaryName = MetadataHelper.GetEntityPrimaryName(_orgService, _tracer, targetEntity);
                    }
                    catch (Exception e)
                    {
                        LogError($"Error fetching metadata: {e.Message}", config, e);
                    }

                    if (string.IsNullOrEmpty(targetEntityPrimaryName))
                    {
                        return new QueryMultipleLookupConditions(null, null);
                    }

                    LinkEntity link = new LinkEntity(entityName, targetEntity, attribute, $"{targetEntity}id", JoinOperator.LeftOuter);

                    link.EntityAlias = $"{targetEntity}alias";
                    link.Columns.AddColumn($"{targetEntity}id");

                    link.LinkCriteria.AddCondition(targetEntityPrimaryName, conditionOperator, conditionValue + (conditionOperator == ConditionOperator.Like ? "%" : ""));

                    linkEntities.Add(link);

                    ConditionExpression conditionExpression = new ConditionExpression($"{targetEntity}alias", $"{targetEntity}id", ConditionOperator.NotNull);
                    conditionExpressions.Add(conditionExpression);
                }
                FilterExpression filterExpression = new FilterExpression(LogicalOperator.Or);
                filterExpression.Conditions.AddRange(conditionExpressions);

                return new QueryMultipleLookupConditions(linkEntities, filterExpression);
            }
            catch (Exception e)
            {
                LogError($"Error fetching metadata: {e.Message}", config, e);
                return new QueryMultipleLookupConditions(null, null);
            }
        }

        private class QueryMultipleLookupConditions
        {
            public List<LinkEntity> LinkEntities;
            public FilterExpression FilterExpression;

            public QueryMultipleLookupConditions(List<LinkEntity> le, FilterExpression fe)
            {
                LinkEntities = le;
                FilterExpression = fe;
            }
        }

        private ConditionExpression GetConditionExpressionForAttribute(string entityName, string attribute, string attributeType, string conditionValue, ConditionOperator conditionOperator, TimeZoneInfo timeZone,
            DOMEngineConfig config)
        {
            ConditionExpression conditionExpression = null;

            if (conditionOperator == ConditionOperator.Null || conditionOperator == ConditionOperator.NotNull)
            {
                return new ConditionExpression(attribute, conditionOperator);
            }

            if (attributeType == Constants.BooleanAttributeType)
            {
                if (bool.TryParse((string)conditionValue, out bool val))
                {
                    conditionValue = val ? "1" : "0";
                }
                else
                {
                    conditionValue = MetadataHelper.GetTwoOptionCodeForBoolValue(_orgService, _tracer, entityName, attribute, (string)conditionValue);
                }

                conditionExpression = new ConditionExpression(attribute, conditionOperator, conditionValue);
            }
            else if (attributeType == Constants.OptionSetAttributeType)
            {
                string optionSetLabel = (string)conditionValue;

                int? optionSetValue = null;
                try
                {
                    optionSetValue = MetadataHelper.GetOptionSetValueFromLabel(_orgService, _tracer, entityName, attribute, optionSetLabel);
                }
                catch (Exception e)
                {
                    LogError($"Error fetching metadata: {e.Message}", config, e);
                }
                if (optionSetValue == null)
                {
                    return null;
                }

                conditionExpression = new ConditionExpression(attribute, conditionOperator, optionSetValue.Value + (conditionOperator == ConditionOperator.Like ? "%" : ""));
            }
            else if (attributeType == Constants.DateTimeAttributeType)
            {

                var dateTimeBehavior = MetadataHelper.GetAttributeDateTimeBehavior(_orgService, _tracer, entityName, attribute);

                DateTime? utcDateTime = MetadataHelper.ParseStringDateTimeAndConvertToUTC((string)conditionValue, timeZone, dateTimeBehavior);

                if (utcDateTime == null)
                {
                    return null;
                }
                else
                {
                    ConditionOperator dateOperator = MetadataHelper.GetConditionOperatorForDateTimeString((string)conditionValue, false);
                    conditionExpression = new ConditionExpression(attribute, dateOperator, utcDateTime);
                }
            }
            else
            {
                conditionExpression = new ConditionExpression(attribute, conditionOperator, conditionValue + (conditionOperator == ConditionOperator.Like ? "%" : ""));
            }

            return conditionExpression;
        }

        private void CreatePosts(EntityReference assignedUser, List<bool> response, List<Entity> listMembers, string attributeNameToBeAssigned, string matchingAttribute,
            DOMEngineConfig config)
        {
            List<EntityReference> regardingEntityIds = new List<EntityReference>();

            if (config.CreatePosts)
            {

                LogInfo($"Creating posts for {response.Count} entities", config);

                List<OrganizationRequest> createRequests = new List<OrganizationRequest>();

                for (var i = 0; i < response.Count(); i++)
                {
                    var responseSucceeded = response[i];
                    EntityReference regardingEntity;
                    if (responseSucceeded)
                    {
                        Entity item = listMembers[i];
                        regardingEntity = new EntityReference(config.EntityLogicalName, RetrieveMemberId(item));
                    }
                    else
                    {
                        continue;
                    }

                    string attributeToBeAssignedDisplayName = null;
                    try
                    {
                        attributeToBeAssignedDisplayName = MetadataHelper.GetAttributeDisplayName(_orgService, _tracer, regardingEntity?.LogicalName, attributeNameToBeAssigned);
                    }
                    catch (Exception e)
                    {
                        LogError($"Error fetching metadata: {e.Message}", config, e);
                    }

                    if (config.UtcNow.Hour != DateTime.UtcNow.Hour)
                    {
                        config.UtcNow = DateTime.UtcNow;
                        config.UserLocalTime = DateHelper.RetrieveLocalTimeFromUtc(_orgService, config.UtcNow);
                    }

                    Entity post = new Entity("post");
                    post["regardingobjectid"] = regardingEntity;
                    post["text"] = !string.IsNullOrEmpty(matchingAttribute) ?
                        $"{attributeToBeAssignedDisplayName} set to {assignedUser.Name} on {config.UserLocalTime.ToLongDateString()} by matching on {matchingAttribute}" :
                        $"{attributeToBeAssignedDisplayName} set to {assignedUser.Name} on {config.UserLocalTime.ToLongDateString()} using the Fall Back User";

                    createRequests.Add(new CreateRequest()
                    {
                        Target = post
                    });
                    regardingEntityIds.Add(regardingEntity);
                }

                var responses = BatchExecute(createRequests, config);

                for (var i = 0; i < responses.Count(); i++)
                {
                    var responseSucceeded = responses[i];
                    if (!responseSucceeded)
                    {
                        var guid = regardingEntityIds[i].Id.ToString();
                        LogWarn($"Error creating post for entity with id = {guid}", config);
                    }
                }
            }
        }

        private Guid RetrieveMemberId(Entity member)
        {
            return member.Id;
        }

        private ListMemberTypeAndLookupField RetrieveListMemberEntityTypeAndLookupField(DOMEngineConfig config)
        {
            var listMemberTypeandLookupField = new ListMemberTypeAndLookupField()
            {
                ListMemberEntityType = config.EntityLogicalName
            };

            if (config.EntityLogicalName == Opportunity.EntityLogicalName)
            {
                listMemberTypeandLookupField.ListMemberEntityType = Contact.EntityLogicalName;
                listMemberTypeandLookupField.ListMemberLookupField = "cmc_contactid";
            }

            return listMemberTypeandLookupField;
        }

        private class ListMemberTypeAndLookupField
        {
            public string ListMemberEntityType { get; set; }
            public string ListMemberLookupField { get; set; }
        }

        private LinkEntity RetrieveOrBuildSubEntityExpression(QueryExpression query, DOMEngineConfig config)
        {
            LinkEntity subEntityExpression = null;
            if (config.EntityLogicalName == Opportunity.EntityLogicalName)
            {
                subEntityExpression = query.LinkEntities.FirstOrDefault(
                    le => le.LinkToEntityName == Opportunity.EntityLogicalName &&
                    (le.LinkToAttributeName == "customerid" || le.LinkToAttributeName == "cmc_contactid"));

                if (subEntityExpression == null)
                {
                    subEntityExpression = new LinkEntity()
                    {
                        LinkToEntityName = Opportunity.EntityLogicalName,
                        LinkFromEntityName = Contact.EntityLogicalName,
                        LinkToAttributeName = "cmc_contactid",
                        LinkFromAttributeName = "contactid",
                    };

                    if (subEntityExpression.Columns.Columns.Contains("opportunityid") == false)
                    {
                        subEntityExpression.Columns.AddColumn("opportunityid");
                    }
                }
            }

            if (subEntityExpression != null)
            {
                subEntityExpression.EntityAlias = config.EntityLogicalName;
            }

            return subEntityExpression;
        }

        private void LogInfo(string message, DOMEngineConfig config)
        {
            _tracer.Info($"{config.EntityLogicalName}: {message}");
        }

        private void LogError(string message, DOMEngineConfig config, Exception e = null)
        {
            if (e != null)
            {
                _tracer.Error(e, $"{config.EntityLogicalName}: {message}");
            }
            else
            {
                _tracer.Error($"{config.EntityLogicalName}: {message}");
            }
        }

        private void LogWarn(string message, DOMEngineConfig config)
        {
            _tracer.Warn($"{config.EntityLogicalName}: {message}");
        }		
	}
}

#endregion
