using System;
using System.Linq;
using System.ServiceModel;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    public class DomDefinitionLogicService : IDomDefinitionLogicService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;
        private readonly ILanguageService _retrieveMultiLingualValues;
        private bool _isOnActivate;

        public DomDefinitionLogicService(ILogger tracer, ILanguageService retrieveMultiLingualValues, IOrganizationService orgService)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _retrieveMultiLingualValues = retrieveMultiLingualValues ?? throw new ArgumentException(nameof(retrieveMultiLingualValues));          
            _orgService = orgService;
        }

        #region Validate Dom Definition Logic Service

        public void ValidateDomDefinitionLogic(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _tracer.Trace($"Start {nameof(ValidateDomDefinitionLogic)}");

            if (!pluginContext.IsValidCall("cmc_domdefinitionlogic"))
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            }

            var target = pluginContext.GetTargetEntity<cmc_domdefinitionlogic>();

            cmc_domdefinitionlogic preImage = null;
            _tracer.Trace("Checking for PreImage");
            if (pluginContext.PreEntityImages.ContainsKey("PreImage"))
            {
                _tracer.Trace("Retrieving PreImage");
                preImage = pluginContext.GetPreEntityImage<cmc_domdefinitionlogic>("PreImage");
            }

            ValidateDomDefinitionLogic(target, preImage);

            _tracer.Trace($"End {nameof(ValidateDomDefinitionLogic)}");

        }
        
        private void ValidateDomDefinitionLogic(cmc_domdefinitionlogic target, cmc_domdefinitionlogic preImage)
        {
            _tracer.Trace($"Start {nameof(ValidateDomDefinitionLogic)}");
            AttributeMetadata attribute = null;
            var ignoreAttributeLogic = false;

            // On Create
            if (preImage == null)
            {
                HandleOnCreateOrPostActivation(target);
            }
            // On Update
            else
            {
                _isOnActivate = target.statecode == cmc_domdefinitionlogicState.Active;
                _tracer.Trace(_isOnActivate ? "Activating Record" : "Deactivating Record");

                if (target.Contains("cmc_conditiontype") ||
                    target.Contains("cmc_minimum") ||
                    target.Contains("cmc_maximum") ||
                    target.Contains("cmc_value") ||
                    target.Contains("cmc_attributeschema") ||
                    target.Contains("statecode") ||
                    target.Contains("cmc_domdefinitionid"))
                {
                    if (attribute == null)
                    {
                        attribute = ValidateDefinitionLogic(target, preImage);
                    }
                    // Ignore Attribute validation if Condition Type or Attribute didn't change
                    if (!(target.Contains("cmc_conditiontype") || target.Contains("cmc_attributeschema")))
                    {
                        ignoreAttributeLogic = true;
                    }
                    ValidateConditionType(target, preImage, attribute, ignoreAttributeLogic);
                    if ((target.Contains("cmc_value") && target.cmc_value != preImage.cmc_value) ||
                        (target.Contains("cmc_attributeschema") &&
                        target.cmc_attributeschema != preImage.cmc_attributeschema))
                    {
                        ValidateValueExists(target, preImage, attribute);
                    }

                    if (IsAttributeDateTime(attribute.AttributeType))
                    {
                        if ((target.Contains("cmc_attributeschema") &&
                           target.cmc_attributeschema != preImage.cmc_attributeschema))
                        {
                            ValidateDateTime(target, preImage, attribute);
                        }
                        else
                        {
                            var attributeName = target.GetValueOrFallback<string>(preImage, "cmc_attributeschema");
                            if ((target.Contains("cmc_value") && target.cmc_value != preImage.cmc_value))
                            {
                                ValidateDateTimeField(target.cmc_value,
                                    string.Format(_retrieveMultiLingualValues.Get("Invalid_DateTime_Value"), attributeName));
                            }
                            if ((target.Contains("cmc_minimum") && target.cmc_minimum != preImage.cmc_minimum))
                            {
                                ValidateDateTimeField(target.cmc_minimum,
                                    string.Format(_retrieveMultiLingualValues.Get("Invalid_DateTime_Min"), attributeName));
                            }
                            if ((target.Contains("cmc_maximum") && target.cmc_maximum != preImage.cmc_maximum))
                            {
                                ValidateDateTimeField(target.cmc_maximum,
                                    string.Format(_retrieveMultiLingualValues.Get("Invalid_DateTime_Max"), attributeName));
                            }
                        }
                    }
                    if (_isOnActivate)
                    {
                        var postImage = MergePreImageAndTarget(target, preImage);
                        HandleOnCreateOrPostActivation(postImage);
                    }
                }
            }
            _tracer.Trace($"End {nameof(ValidateDomDefinitionLogic)}");
        }

        private AttributeMetadata ValidateDefinitionLogic(cmc_domdefinitionlogic target, cmc_domdefinitionlogic preImage)
        {

            _tracer.Trace("Validating related DOM Definition is not blank.");

            var domDefinitionId = (Guid)target.GetValueOrFallback<EntityReference>(preImage, "cmc_domdefinitionid")?.Id;
            if (domDefinitionId == default(Guid))
            {
                ThrowMultilingualError(
                    _retrieveMultiLingualValues.Get("Missing_DOM_Definition_Lookup"));
            }

            var domDefinition = GetRelatedDomDefinitionAndDomMaster(domDefinitionId);
            var attributeSchema = target.GetValueOrFallback<string>(preImage, "cmc_attributeschema");
            var domMaster = domDefinition.cmc_dommasterid;

            if (domMaster == null)
            {
                var errorBase = string.Format(
                    _retrieveMultiLingualValues.Get("Missing_Related_DOM_Master"),
                    domDefinition.cmc_domdefinitionname ?? $"Id: {domDefinitionId} - (No name)");
                ThrowMultilingualError(errorBase);
            }

            _tracer.Trace("Validating related DOM Master's Run Assignment For Entity and Entity match.");

            DomAssignmentCommonService.ValidateAttributeFormat(attributeSchema, _retrieveMultiLingualValues, "Invalid_Format_Execution_Order_Attribute", CreateMutliLingualError);

            return DomAssignmentCommonService.ValidateAttributeString(attributeSchema, domMaster, _retrieveMultiLingualValues, _tracer, _orgService, CreateMutliLingualError);
        }

        private cmc_domdefinitionlogic MergePreImageAndTarget(cmc_domdefinitionlogic target, cmc_domdefinitionlogic preImage)
        {
            var postImage = new cmc_domdefinitionlogic();
            postImage.Id = target.Id;

            foreach (var attribute in target.Attributes)
            {
                postImage[attribute.Key] = target[attribute.Key];
            }

            foreach (var attribute in preImage.Attributes)
            {
                if (!target.Contains(attribute.Key))
                {
                    postImage[attribute.Key] = preImage[attribute.Key];
                }
            }

            var dumpPostImage = JsonConvert.SerializeObject(postImage, Formatting.Indented);
            _tracer.Trace($@"DOM Definition Logic Post-Update:
                {dumpPostImage}");
            return postImage;
        }

        private cmc_domdefinition GetRelatedDomDefinitionAndDomMaster(Guid domDefinitionId)
        {
            var results = _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch top='1'>
                  <entity name='cmc_domdefinition'>
                    <attribute name='cmc_domdefinitionid' />
                    <attribute name='cmc_domdefinitionname' />
                    <attribute name='cmc_dommasterid' />
                    <filter type='and'>
                       <condition attribute='cmc_domdefinitionid' operator='eq' value='{domDefinitionId}' />
                    </filter>
                    <link-entity name='cmc_dommaster' from='cmc_dommasterid' to='cmc_dommasterid' link-type='inner' alias='master' >
                        <attribute name='cmc_runassignmentforentity' />
                    </link-entity>
                  </entity>
                </fetch>")).Entities.FirstOrDefault()?.ToEntity<cmc_domdefinition>();

            return results;
        }

        private void ValidateConditionType(cmc_domdefinitionlogic target, cmc_domdefinitionlogic preImage, AttributeMetadata attribute, bool ignoreAttributeLogic)
        {
            _tracer.Trace($"Start {nameof(ValidateConditionType)}");

            _tracer.Trace("Validating Condition Type is not blank.");
            var conditionType = target.GetValueOrFallback<OptionSetValue>(preImage,
                "cmc_conditiontype").Value;
            var attributeType = attribute.AttributeType;
            var attributeName = attribute.LogicalName;

            if (conditionType == (int)cmc_domconditiontype.BeginsWith)
            {
                if (string.IsNullOrWhiteSpace(target.GetValueOrFallback<string>(preImage, "cmc_value")))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_Value_Format_Condition_Begins_With"));
                }
                if (!ignoreAttributeLogic && IsAttributeOptionSet(attributeType))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_BeginsWith_Condition_On_OptionSet_Attribute"));
                }
                if (!ignoreAttributeLogic && IsAttributeBoolean(attributeType))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_BeginsWith_Condition_On_Boolean_Attribute"));
                }
                if (!ignoreAttributeLogic && IsAttributeDateTime(attributeType))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_BeginsWith_Condition_On_DateTime_Attribute"));
                }
                if (target.cmc_minimum != null || target.cmc_maximum != null)
                {
                    target.cmc_minimum = null;
                    target.cmc_maximum = null;
                }
            }
            else if (conditionType == (int)cmc_domconditiontype.Equals)
            {
                if (string.IsNullOrWhiteSpace(target.GetValueOrFallback<string>(preImage, "cmc_value")))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_Value_Format_Condition_Equals"));
                }
                if (target.cmc_minimum != null || target.cmc_maximum != null)
                {
                    target.cmc_minimum = null;
                    target.cmc_maximum = null;
                }
            }
            else if (conditionType == (int)cmc_domconditiontype.Range)
            {
                if (target.GetValueOrFallback<string>(preImage, "cmc_minimum") == null ||
                    target.GetValueOrFallback<string>(preImage, "cmc_maximum") == null)
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_MinMax_Format_Condition_Range"));
                }
                if (!string.IsNullOrWhiteSpace(target.cmc_value))
                {
                    target.cmc_value = null;
                }
                if (!ignoreAttributeLogic && IsAttributeLookup(attributeType))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_Range_Condition_On_Lookup_Attribute"));
                }
                if (!ignoreAttributeLogic && IsAttributeOptionSet(attributeType))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_Range_Condition_On_OptionSet_Attribute"));
                }
                if (!ignoreAttributeLogic && IsAttributeBoolean(attributeType))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_Range_Condition_On_Boolean_Attribute"));
                }
            }
            else if (conditionType == (int)cmc_domconditiontype.RecordOwner)
            {
                if (!IsAttributeLookup(attributeType))
                {
                    ThrowMultilingualError(
                        _retrieveMultiLingualValues.Get("Invalid_RecordOwner_Condition_On_NonLookup_Attribute"));
                }
                if (!ignoreAttributeLogic && !ValidateAllRelatedLookupTargetsHaveOwner((LookupAttributeMetadata)attribute, out var targetName))
                {
                    var errorBase = string.Format(
                        _retrieveMultiLingualValues.Get("Invalid_RecordOwner_Condition_On_Attribute_Not_User_Owned"),
                        attributeName,
                        targetName);
                    ThrowMultilingualError(errorBase);
                }
                if (target.cmc_minimum != null ||
                    target.cmc_maximum != null ||
                    target.cmc_value != null)
                {
                    target.cmc_minimum = null;
                    target.cmc_maximum = null;
                    target.cmc_value = null;
                }
            }
            else if (conditionType == (int)cmc_domconditiontype.IsNull || conditionType == (int)cmc_domconditiontype.IsNotNull)
            {
                target.cmc_minimum = null;
                target.cmc_maximum = null;
                target.cmc_value = null;
            }
            _tracer.Trace($"End {nameof(ValidateConditionType)}");
        }

        private void ValidateDateTime(cmc_domdefinitionlogic target, cmc_domdefinitionlogic preImage, AttributeMetadata attribute)
        {
            _tracer.Trace($"Start {nameof(ValidateDateTime)}");

            if (IsAttributeDateTime(attribute.AttributeType))
            {
                _tracer.Trace("Validating Value/Min/Max are a valid Date/Time.");

                var value = target.GetValueOrFallback<string>(preImage, "cmc_value");
                var min = target.GetValueOrFallback<string>(preImage, "cmc_minimum");
                var max = target.GetValueOrFallback<string>(preImage, "cmc_maximum");
                var attributeName = target.GetValueOrFallback<string>(preImage, "cmc_attributeschema");

                ValidateDateTimeField(value,
                    string.Format(_retrieveMultiLingualValues.Get("Invalid_DateTime_Value"), attributeName));
                ValidateDateTimeField(min,
                    string.Format(_retrieveMultiLingualValues.Get("Invalid_DateTime_Min"), attributeName));
                ValidateDateTimeField(max,
                    string.Format(_retrieveMultiLingualValues.Get("Invalid_DateTime_Max"), attributeName));
            }

            _tracer.Trace($"End {nameof(ValidateDateTime)}");
        }

        private void ValidateDateTimeField(string value, string error)
        {
            if (!string.IsNullOrWhiteSpace(value) && !IsValidDateTime(value))
            {
                ThrowMultilingualError(error);
            }
        }

        private bool IsValidDateTime(string value)
        {
            DateTime dateValue;
            return (DateTime.TryParse(value, out dateValue));
        }

        private void ValidateValueExists(cmc_domdefinitionlogic target, cmc_domdefinitionlogic preImage, AttributeMetadata attribute)
        {
            _tracer.Trace($"Start {nameof(ValidateValueExists)}");

            _tracer.Trace("Validating Value is not blank.");
            var value = target.GetValueOrFallback<string>(preImage,
                "cmc_value");
            var attributeType = attribute.AttributeType;
            var attributeName = attribute.LogicalName.ToLower();
            var conditionType = target.GetValueOrFallback<OptionSetValue>(preImage, "cmc_conditiontype")?.Value;


            if (conditionType == (int)cmc_domconditiontype.IsNull || conditionType == (int)cmc_domconditiontype.IsNotNull) {
                _tracer.Trace($"Condition type {conditionType} does not require a value, validation complete.");
                return;
            }

            bool? containsValueAsLabel = null;

            if (attributeType == AttributeTypeCode.State)
            {
                containsValueAsLabel = OptionsetContainsValueAsLabel(value,
                    ((StateAttributeMetadata)attribute).OptionSet.Options);
            }
            if (attributeType == AttributeTypeCode.Status)
            {
                containsValueAsLabel = OptionsetContainsValueAsLabel(value,
                    ((StatusAttributeMetadata)attribute).OptionSet.Options);
            }
            if (attributeType == AttributeTypeCode.Picklist)
            {
                containsValueAsLabel = OptionsetContainsValueAsLabel(value,
                    ((PicklistAttributeMetadata)attribute).OptionSet.Options);
            }
            if (attributeType == AttributeTypeCode.Boolean)
            {
                containsValueAsLabel = BooleanContainsValueAsLabel(value,
                    ((BooleanAttributeMetadata)attribute).OptionSet);
            }

            if (containsValueAsLabel == false)
            {
                var errorBase = string.Format(_retrieveMultiLingualValues.Get("Missing_Value_As_Label"), attributeName);
                ThrowMultilingualError(errorBase);
            }
            _tracer.Trace($"End {nameof(ValidateValueExists)}");
        }

        private bool BooleanContainsValueAsLabel(string value, BooleanOptionSetMetadata boolean)
        {
            _tracer.Trace($"Validating Value is a valid boolean field.");

            if (bool.TryParse(value, out var result))
            {
                _tracer.Trace("Value is true or false and is considered valid.");
                return true;
            }

            var labelExists = DoesLabelExist(value, boolean.TrueOption.Label.LocalizedLabels);
            if (labelExists)
                return labelExists;

            return DoesLabelExist(value, boolean.FalseOption.Label.LocalizedLabels);
        }

        private bool OptionsetContainsValueAsLabel(string value, OptionMetadataCollection options)
        {
            _tracer.Trace($"Validating Value is a valid option in Option Set.");
            var containsValueAsLabel = options
                .FirstOrDefault(o => DoesLabelExist(value, o.Label.LocalizedLabels)) != null;
            return containsValueAsLabel;
        }

        private bool DoesLabelExist(string value, LocalizedLabelCollection labels)
        {
            return labels.Any(a => a.Label.Equals(value, StringComparison.InvariantCultureIgnoreCase));
        }

        private EntityMetadata GetEntity(string entityLogicalName)
        {
            _tracer.Trace($"Start {nameof(GetEntity)}");
            var metaDataRequest = new RetrieveEntityRequest();
            metaDataRequest.LogicalName = entityLogicalName;
            metaDataRequest.EntityFilters = EntityFilters.Attributes;
            EntityMetadata entity = null;

            try
            {
                var metaDataResponse = (RetrieveEntityResponse)_orgService.Execute(metaDataRequest);
                entity = metaDataResponse?.EntityMetadata;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                _tracer.Trace($"{ex.Message}");
            }
            catch (Exception ex)
            {
                _tracer.Trace($"{ex.Message}");
            }

            _tracer.Trace($"End {nameof(GetEntity)}");
            return entity;
        }

        private bool ValidateAllRelatedLookupTargetsHaveOwner(LookupAttributeMetadata lookupAttribute, out string targetName)
        {
            _tracer.Trace($"Start {nameof(ValidateAllRelatedLookupTargetsHaveOwner)}");
            foreach (var target in lookupAttribute.Targets)
            {
                var targetMetadata = GetEntity(target);
                var mustHaveAttributeMetadata = targetMetadata?.Attributes?.FirstOrDefault(a => a.AttributeType == AttributeTypeCode.Owner);

                if (mustHaveAttributeMetadata == default(AttributeMetadata))
                {
                    targetName = targetMetadata.LogicalName;
                    return false;
                }
            }

            targetName = "";
            _tracer.Trace($"End {nameof(ValidateAllRelatedLookupTargetsHaveOwner)}");
            return true;
        }

        private bool IsAttributeLookup(AttributeTypeCode? attributeType)
        {
            return attributeType == AttributeTypeCode.Lookup ||
                   attributeType == AttributeTypeCode.Owner ||
                   attributeType == AttributeTypeCode.Customer;
        }

        private bool IsAttributeOptionSet(AttributeTypeCode? attributeType)
        {
            return attributeType == AttributeTypeCode.State ||
                   attributeType == AttributeTypeCode.Status ||
                   attributeType == AttributeTypeCode.Picklist;
        }

        private bool IsAttributeBoolean(AttributeTypeCode? attributeType)
        {
            return attributeType == AttributeTypeCode.Boolean;
        }

        private bool IsAttributeDateTime(AttributeTypeCode? attributeType)
        {
            return attributeType == AttributeTypeCode.DateTime;
        }

        private void ThrowMultilingualError(string error)
        {
            throw CreateMutliLingualError(error);
        }

        private InvalidPluginExecutionException CreateMutliLingualError(string error)
        {
            if (_isOnActivate)
            {
                error = string.Format(
                    _retrieveMultiLingualValues.Get("Error_Prefix_On_Activate_of_DOM_Definition_Logic"), error);
            }
            return new InvalidPluginExecutionException(error);
        }

        private void HandleOnCreateOrPostActivation(cmc_domdefinitionlogic domdefinitionlogic)
        {
            _tracer.Trace($"Start {nameof(HandleOnCreateOrPostActivation)}");

            var attribute = ValidateDefinitionLogic(domdefinitionlogic, null);

            ValidateConditionType(domdefinitionlogic, null, attribute, false);
            ValidateValueExists(domdefinitionlogic, null, attribute);
            ValidateDateTime(domdefinitionlogic, null, attribute);

            _tracer.Trace($"End {nameof(HandleOnCreateOrPostActivation)}");
        }

        #endregion
    }
}
