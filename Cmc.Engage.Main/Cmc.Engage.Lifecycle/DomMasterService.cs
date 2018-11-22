using System;
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
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    public class DomMasterService : IDomMasterService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;
        private readonly ILanguageService _retrieveMultiLingualValues;

        private const int _defaultCreatedFromCode = -1;

        public DomMasterService(ILogger tracer, ILanguageService retrieveMultiLingualValues, IOrganizationService orgService)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _retrieveMultiLingualValues = retrieveMultiLingualValues ?? throw new ArgumentException(nameof(retrieveMultiLingualValues));         
            _orgService = orgService;
        }

        #region Validate Dom Master Service

        public void ValidateDomMasterService(IExecutionContext context)
        {

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateOrganizationServiceAsCurrentUser();
            _tracer.Trace($"Entered plugin: {nameof(ValidateDomMasterService)}");

            if (!pluginContext.IsValidCall(cmc_dommaster.EntityLogicalName))
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            }

            var target = pluginContext.GetTargetEntity<cmc_dommaster>();

            cmc_dommaster preImage = null;
            _tracer.Trace("Checking for PreImage");
            if (pluginContext.PreEntityImages.ContainsKey("Target"))
            {
                _tracer.Trace("Retrieving PreImage");
                preImage = pluginContext.GetPreEntityImage<cmc_dommaster>("Target");
            }

            ValidateDomMaster(target, preImage);
        }

        private void ValidateDomMaster(cmc_dommaster domMaster, cmc_dommaster preImage)
        {
            _tracer.Trace("Validating DOM Master");

            if (preImage == null ||
                domMaster.cmc_runassignmentforentity?.Value != preImage.cmc_runassignmentforentity?.Value)
            {
                _tracer.Trace("Valdiating Run Assignment For Entity");
                ValidateRunAssignmentForEntity(domMaster, preImage);
            }

            // Checks If Attribute Name To Be Assigned was just set to Other User Lookup or 
            // Attribute Name To Be Assigned was not changed and is still Other User Lookup
            if (domMaster.cmc_attributenametobeassigned?.Value == (int)cmc_attributenametobeassigned.OtherUserLookup ||
                domMaster.Contains("cmc_attributenametobeassigned") == false &&
                preImage?.cmc_attributenametobeassigned?.Value == (int)cmc_attributenametobeassigned.OtherUserLookup)
            {
                _tracer.Trace("Attribute Name To Be Assigned is Other User Lookup.");
                ValidateOtherUserLookup(domMaster, preImage);
            }
            // Checks if Attribute Name To Be Assigned was just changed 
            // or it previously was not Other User Lookup and Other User Lookup was set
            // or was not blank previously
            else if ((domMaster.Contains("cmc_attributenametobeassigned") == true ||
                preImage?.cmc_attributenametobeassigned?.Value != (int)cmc_attributenametobeassigned.OtherUserLookup) &&
                (domMaster.Contains("cmc_otheruserlookup") ||
                string.IsNullOrEmpty(preImage?.cmc_OtherUserLookup) == false))
            {
                _tracer.Trace("Clearing Other User Lookup as Attribute Name to Be Assigned is not Other User Lookup.");
                domMaster.cmc_OtherUserLookup = null;
            }
        }

        private void ValidateRunAssignmentForEntity(cmc_dommaster dommaster, cmc_dommaster preImage)
        {
            var marketingListId = dommaster.GetValueOrFallback<EntityReference>(preImage,
                "cmc_marketinglistid");
            var runAssignmentForEntity = dommaster.GetValueOrFallback<OptionSetValue>(preImage,
                "cmc_runassignmentforentity");

            if (marketingListId == null)
            {
                _tracer.Trace("Marketing List Id is null. Throwing error.");
                throw CreateInvalidRunAssignmentForException(runAssignmentForEntity?.Value,
                    _defaultCreatedFromCode);
            }

            var createdFromCode = RetrieveCreatedFromCode(marketingListId);

            _tracer.Trace($"Checking if {runAssignmentForEntity?.Value} is a valid Run Assignment For Entity for a Marketing List with Type Code {createdFromCode}.");
            bool validCode = false;
            switch (createdFromCode)
            {
                case Account.EntityTypeCode:
                    if (runAssignmentForEntity?.Value == (int)cmc_runassignmentforentity.Account)
                    {
                        validCode = true;
                    }
                    break;
                case Contact.EntityTypeCode:
                    if (runAssignmentForEntity?.Value == (int)cmc_runassignmentforentity.Contact ||
                        runAssignmentForEntity?.Value == (int)cmc_runassignmentforentity.Lifecycle)
                    {
                        validCode = true;
                    }
                    break;
                case Lead.EntityTypeCode:
                    if (runAssignmentForEntity?.Value == (int)cmc_runassignmentforentity.InboundInterest)
                    {
                        validCode = true;
                    }
                    break;
            }

            if (validCode == false)
            {
                _tracer.Trace("Run Assignment For Entity is not valid.");
                throw CreateInvalidRunAssignmentForException(runAssignmentForEntity?.Value,
                    createdFromCode);
            }

            if (preImage == null ||
                !dommaster.Attributes.Contains("cmc_runassignmentforentity") ||
                dommaster.cmc_runassignmentforentity?.Value == preImage.cmc_runassignmentforentity?.Value)
            {
                _tracer.Trace("Run Assignment For Entity was not updated. No need to check if records exist for this DOM Master");
                return;
            }

            if (IsDomMasterUsed(dommaster.cmc_dommasterId.Value))
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("Cannot_Change_Run_Assignment_For_Entity"));
            }

            _tracer.Trace("DOM Master is not currently in use. Run Assignment For Entity can change.");
        }

        private void ValidateOtherUserLookup(cmc_dommaster domMaster, cmc_dommaster preImage)
        {
            _tracer.Trace("Validating Other User Lookup is not blank.");
            var otherUserLookup = domMaster.GetValueOrFallback<string>(preImage,
                "cmc_otheruserlookup")?.ToLower();

            if (string.IsNullOrWhiteSpace(otherUserLookup))
            {
                throw new InvalidPluginExecutionException(
                    _retrieveMultiLingualValues.Get("Missing_Other_User_Lookup"));
            }

            DomAssignmentCommonService.ValidateAttributeFormat(otherUserLookup, _retrieveMultiLingualValues, "Invalid_Format_Other_User_Lookup");


            _tracer.Trace("Splitting Other User Lookup");
            var otherUserLookupParts = otherUserLookup.Split('.');
            var entityName = otherUserLookupParts[0].ToLower();
            var attributeName = otherUserLookupParts[1].ToLower();

            var runAssignmentForEntity = domMaster.GetValueOrFallback<OptionSetValue>(preImage,
                "cmc_runassignmentforentity");

            DomAssignmentCommonService.ValidateEntityNameMatchesRunAssignmentFor(entityName, runAssignmentForEntity?.Value,
                _retrieveMultiLingualValues, "Invalid_Entity_Other_User_Lookup", _orgService, _tracer);

            _tracer.Trace($"Retrieving Metadata for Entity {entityName} and attribute {attributeName}.");
            var metadataResponse = _orgService.Execute(new RetrieveMetadataChangesRequest()
            {
                Query = new EntityQueryExpression()
                {
                    Criteria = new MetadataFilterExpression()
                    {
                        Conditions =
                        {
                            new MetadataConditionExpression("LogicalName",
                                MetadataConditionOperator.Equals, entityName)
                        }
                    },
                    AttributeQuery = new AttributeQueryExpression()
                    {
                        Criteria = new MetadataFilterExpression()
                        {
                            Conditions =
                            {
                                new MetadataConditionExpression("LogicalName",
                                    MetadataConditionOperator.Equals, attributeName)
                            }
                        },
                        Properties = new MetadataPropertiesExpression("LogicalName", "AttributeType",
                            "AttributeTypeName", "Targets", "IsValidForUpdate")
                    },
                    LabelQuery = new LabelQueryExpression(),
                    Properties = new MetadataPropertiesExpression("Attributes")
                }
            }) as RetrieveMetadataChangesResponse;

            if (metadataResponse?.EntityMetadata.Count == 0 ||
                metadataResponse?.EntityMetadata.First().Attributes.Count() == 0)
            {
                _tracer.Trace($"{entityName} entity not found or had no attributes returned.");
                throw CreateInvalidOtherUserLookupException(attributeName, entityName,
                    _retrieveMultiLingualValues);
            }

            var attributeMetadata = metadataResponse.EntityMetadata.First().Attributes.FirstOrDefault(
                attribute => attribute.LogicalName == attributeName);

            if (attributeMetadata == null)
            {
                _tracer.Trace($"Attribute {attributeName} not found on Entity {entityName}");
                throw CreateInvalidOtherUserLookupException(attributeName, entityName,
                    _retrieveMultiLingualValues);
            }
            else if (attributeMetadata.AttributeType == AttributeTypeCode.Owner)
            {
                _tracer.Trace($"The type for attribute {attributeName} is Owner and can be used.");
                return;
            }
            else if (attributeMetadata.AttributeType != AttributeTypeCode.Lookup)
            {
                _tracer.Trace(
                    $"The type for attribute {attributeName} is {attributeMetadata.AttributeTypeName?.Value} and cannot be used.");
                throw CreateInvalidOtherUserLookupException(attributeName, entityName,
                    _retrieveMultiLingualValues);
            }
            else if ((attributeMetadata.IsValidForUpdate ?? false) == false)
            {
                _tracer.Trace($"Attribute {attributeName} cannot be updated and cannot be used.");
                throw CreateInvalidOtherUserLookupException(attributeName, entityName,
                    _retrieveMultiLingualValues);
            }

            var lookupAttribute = (LookupAttributeMetadata)attributeMetadata;
            if (lookupAttribute.Targets.Contains(SystemUser.EntityLogicalName) == false)
            {
                _tracer.Trace($"Attribute {attributeName} does not target User.");
                throw CreateInvalidOtherUserLookupException(attributeName, entityName,
                    _retrieveMultiLingualValues);
            }
        }

        private InvalidPluginExecutionException CreateInvalidOtherUserLookupException(
            string attributeName, string entityName,
            ILanguageService retrieveMultiLingualValues)
        {
            return new InvalidPluginExecutionException(string.Format(
                retrieveMultiLingualValues.Get("Invalid_Other_User_Lookup"), attributeName, entityName));
        }

        private InvalidPluginExecutionException CreateInvalidRunAssignmentForException(int? runAssignmentForEntityValue, int createdFromCode)
        {
            var metadataService = new MetadataService(_orgService);

            var runAssignmentForEntityLabel = runAssignmentForEntityValue.HasValue
                                              ? metadataService.GetStringValueFromPicklistInt(
                                                cmc_dommaster.EntityLogicalName,
                                                "cmc_runassignmentforentity",
                                                runAssignmentForEntityValue.Value)
                                              : _retrieveMultiLingualValues.Get("(null)");

            var entityName = createdFromCode != _defaultCreatedFromCode
                             ? metadataService.GetEntityNameFromTypeCode(createdFromCode, true)
                             : _retrieveMultiLingualValues.Get("(null)");

            return new InvalidPluginExecutionException(string.Format(
                    _retrieveMultiLingualValues.Get("Invalid_Run_Assignment_For"),
                    runAssignmentForEntityLabel, entityName));
        }

        private int RetrieveCreatedFromCode(EntityReference marketingListId)
        {
            _tracer.Trace($"Retrieving Created From Code for Marketing List {marketingListId?.Id}");
            if (marketingListId == null)
            {
                _tracer.Trace("Null Marketing List, using default code.");
                return _defaultCreatedFromCode;
            }

            return _orgService.Retrieve(marketingListId, new ColumnSet("createdfromcode")
                ).GetAttributeValue<OptionSetValue>("createdfromcode")?.Value ?? _defaultCreatedFromCode;
        }

        private bool IsDomMasterUsed(Guid domMasterId)
        {
            return _orgService.RetrieveMultiple(
                new FetchExpression(
                    $@"<fetch top='1'>
                         <entity name='cmc_dommaster'>
                           <attribute name='cmc_dommasterid' />
                           <link-entity name='cmc_domdefinition' from='cmc_dommasterid' to='cmc_dommasterid' link-type='outer' alias='cmc_domdefinition' />
                           <link-entity name='cmc_domdefinitionexecutionorder' from='cmc_dommasterid' to='cmc_dommasterid' link-type='outer' alias='cmc_domexecutinorder' />
                           <filter type='and'>
                             <condition attribute='cmc_dommasterid' operator='eq' value='{domMasterId}' />
                             <filter type='or'>
                               <condition entityname='cmc_domdefinition' attribute='cmc_dommasterid' operator='not-null' />
                               <condition entityname='cmc_domdefinitionexecutionorder' attribute='cmc_dommasterid' operator='not-null' />
                             </filter>
                           </filter>
                         </entity>
                       </fetch>")).Entities.Count > 0;
        }

        #endregion
    }
}
