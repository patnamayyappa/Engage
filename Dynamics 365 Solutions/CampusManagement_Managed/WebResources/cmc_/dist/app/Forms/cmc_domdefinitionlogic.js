/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CMC;
(function (CMC) {
    var Engage;
    (function (Engage) {
        var cmc_domdefinitionlogic;
        (function (cmc_domdefinitionlogic) {
            var _formLoaded = false, _currentRunAssignmentForEntity = "", _missingRelatedDomDefinitionExecutionOrderId = "missing_domdefinitionexecutionorder_warning", _fieldNames = {
                attribute: "cmc_attribute",
                attributeSchema: "cmc_attributeschema",
                domDefinition: "cmc_domdefinitionid",
                conditionType: "cmc_conditiontype",
                value: "cmc_value",
                minimum: "cmc_minimum",
                maximum: "cmc_maximum"
            }, localizedStrings = {
                errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
                okButton: CampusManagement.localization.getResourceString("OkButton"),
                Missing_Valid_DOM_Definition_Execution_Order_for_DOM_Definition_Logic: CampusManagement.localization.getResourceString("Missing_Valid_DOM_Definition_Execution_Order_for_DOM_Definition_Logic"),
            };
            var StateCode;
            (function (StateCode) {
                StateCode[StateCode["Active"] = 0] = "Active";
            })(StateCode = cmc_domdefinitionlogic.StateCode || (cmc_domdefinitionlogic.StateCode = {}));
            var RunAssignmentForEntityType;
            (function (RunAssignmentForEntityType) {
                RunAssignmentForEntityType[RunAssignmentForEntityType["Account"] = 175490000] = "Account";
                RunAssignmentForEntityType[RunAssignmentForEntityType["Contact"] = 175490001] = "Contact";
                RunAssignmentForEntityType[RunAssignmentForEntityType["Lead"] = 175490002] = "Lead";
                RunAssignmentForEntityType[RunAssignmentForEntityType["Opportunity"] = 175490003] = "Opportunity";
            })(RunAssignmentForEntityType = cmc_domdefinitionlogic.RunAssignmentForEntityType || (cmc_domdefinitionlogic.RunAssignmentForEntityType = {}));
            function onLoad(executionContext) {
                var formContext = executionContext.getFormContext(), domDefinitionLookup = formContext.getAttribute(_fieldNames.domDefinition), attribute = formContext.getAttribute(_fieldNames.attribute);
                if (domDefinitionLookup && attribute) {
                    if (!_formLoaded && formContext.ui.getFormType() !== 1 /* Create */) {
                        _formLoaded = true;
                        validateHasMatchingDomDefinitionExecutionOrder(executionContext);
                        toggleRuleSection(executionContext);
                        domDefinitionLookup.addOnChange(validateHasMatchingDomDefinitionExecutionOrder);
                        attribute.addOnChange(validateHasMatchingDomDefinitionExecutionOrder);
                    }
                    attribute.addOnChange(toggleRuleSection);
                }
                var domDefinition = formContext.getAttribute("cmc_domdefinitionid");
                if (domDefinition) {
                    domDefinition.addOnChange(setDomMasterUi);
                    setDomMasterUi(executionContext);
                }
            }
            cmc_domdefinitionlogic.onLoad = onLoad;
            function setDomMasterUi(executionContext) {
                var formContext = executionContext.getFormContext();
                var domDefinitionId = formContext.getAttribute("cmc_domdefinitionid").getValue();
                var domUi = formContext.getControl("WebResource_AttributePicker");
                if (domDefinitionId != null && domDefinitionId.length > 0) {
                    domDefinitionId = domDefinitionId[0].id;
                    domUi.setVisible(true);
                }
                else {
                    domUi.setVisible(false);
                    return;
                }
                var nameField = "cmc_attribute";
                var schemaField = "cmc_attributeschema";
                var fetchXml = [
                    '<fetch top="1">',
                    '<entity name="cmc_dommaster">',
                    '<attribute name="cmc_runassignmentforentity" />',
                    '<link-entity name="cmc_domdefinition" from="cmc_dommasterid" to="cmc_dommasterid" link-type="inner">',
                    '<filter type="and">',
                    '<condition attribute="cmc_domdefinitionid" operator="eq" value="' + domDefinitionId + '" />',
                    '</filter>',
                    '</link-entity>',
                    '</entity>',
                    '</fetch>'
                ].join('');
                return Xrm.WebApi.retrieveMultipleRecords("cmc_dommaster", "?fetchXml=" + fetchXml)
                    .then(function success(result) {
                    if (result.entities.length > 0) {
                        var forEntityInt = result.entities[0].cmc_runassignmentforentity;
                        if (_currentRunAssignmentForEntity === forEntityInt)
                            return;
                        _currentRunAssignmentForEntity = forEntityInt;
                        var entity = "";
                        switch (forEntityInt) {
                            case RunAssignmentForEntityType.Account:
                                entity = "account";
                                break;
                            case RunAssignmentForEntityType.Contact:
                                entity = "contact";
                                break;
                            case RunAssignmentForEntityType.Lead:
                                entity = "lead";
                                break;
                            case RunAssignmentForEntityType.Opportunity:
                                entity = "opportunity";
                                break;
                        }
                        var input = '{"EntityLogicalNames": "' + entity + '"}';
                        domUi.setSrc("$webresources/cmc_/dist/attributePicker/index.html?data=" + encodeURIComponent("component=attributePicker&entity=" + entity + "&namefield=" + nameField + "&schemafield=" + schemaField + "&showLabel=true&retrieveMetadataJsonAction=cmc_retrieveattributesandrelationshipsforentities&retrieveMetadataJsonInput=" + input));
                    }
                }, function (error) {
                    CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.errorPrefix + error.message, localizedStrings.okButton);
                });
            }
            function toggleRuleSection(executionContext) {
                var formContext = executionContext.getFormContext();
                var attribute = formContext.getAttribute(_fieldNames.attribute);
                if (attribute && attribute.getValue()) {
                    toggleField(formContext.getControl(_fieldNames.attribute), true);
                    toggleField(formContext.getControl(_fieldNames.conditionType), true);
                    toggleField(formContext.getControl(_fieldNames.value), true);
                    toggleField(formContext.getControl(_fieldNames.maximum), true);
                    toggleField(formContext.getControl(_fieldNames.minimum), true);
                }
                else {
                    toggleField(formContext.getControl(_fieldNames.attribute), false);
                    toggleField(formContext.getControl(_fieldNames.conditionType), false);
                    toggleField(formContext.getControl(_fieldNames.value), false);
                    toggleField(formContext.getControl(_fieldNames.maximum), false);
                    toggleField(formContext.getControl(_fieldNames.minimum), false);
                    var source = executionContext.getEventSource();
                    // If the source is the Attribute field, clear the Conditon Type field when attribute
                    // changes. This will kick off a Business Rule to clear and disable the value fields.
                    if (source && source.getName && source.getName() === _fieldNames.attribute) {
                        var conditionType = formContext.getAttribute(_fieldNames.conditionType);
                        if (conditionType) {
                            conditionType.setValue(null);
                            conditionType.fireOnChange();
                        }
                    }
                }
            }
            function toggleField(field, visible) {
                if (field)
                    field.setVisible(visible);
            }
            function validateHasMatchingDomDefinitionExecutionOrder(executionContext) {
                var formContext = executionContext.getFormContext(), domDefinitionValue = formContext.getAttribute(_fieldNames.domDefinition).getValue(), attributeSchemaValue = formContext.getAttribute(_fieldNames.attributeSchema).getValue();
                if (attributeSchemaValue && domDefinitionValue && domDefinitionValue.length > 0) {
                    var fetchXml = [
                        '<fetch top="1">',
                        '<entity name="cmc_domdefinitionexecutionorder">',
                        '<attribute name="cmc_domdefinitionexecutionorderid" />',
                        '<filter type="and">',
                        '<condition attribute="cmc_attributeschema" operator="eq" value="' + attributeSchemaValue + '" />',
                        '<condition attribute="statecode" operator="eq" value="' + StateCode.Active + '"/>',
                        '</filter>',
                        '<link-entity name="cmc_dommaster" from="cmc_dommasterid" to="cmc_dommasterid" link-type="inner" alias="dommaster">',
                        '<link-entity name="cmc_domdefinition" from="cmc_dommasterid" to="cmc_dommasterid" link-type="inner" alias="domdefinition">',
                        '<filter type="and">',
                        '<condition attribute="cmc_domdefinitionid" operator="eq" value="' + domDefinitionValue[0].id + '" />',
                        '</filter>',
                        '</link-entity>',
                        '</link-entity>',
                        '</entity>',
                        '</fetch>'
                    ].join('');
                    return Xrm.WebApi.retrieveMultipleRecords("cmc_domdefinitionexecutionorder", "?fetchXml=" + fetchXml)
                        .then(function success(result) {
                        if (result.entities.length > 0) {
                            toggleWarningNotifications(true, formContext);
                        }
                        else {
                            toggleWarningNotifications(false, formContext);
                        }
                    }, function (error) {
                        CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.errorPrefix + error.message, localizedStrings.okButton);
                    });
                }
                else {
                    toggleWarningNotifications(true, formContext);
                }
            }
            function toggleWarningNotifications(isValid, formContext) {
                if (!isValid) {
                    formContext.ui.setFormNotification(localizedStrings.Missing_Valid_DOM_Definition_Execution_Order_for_DOM_Definition_Logic, "WARNING", _missingRelatedDomDefinitionExecutionOrderId);
                }
                else {
                    formContext.ui.clearFormNotification(_missingRelatedDomDefinitionExecutionOrderId);
                }
            }
        })(cmc_domdefinitionlogic = Engage.cmc_domdefinitionlogic || (Engage.cmc_domdefinitionlogic = {}));
    })(Engage = CMC.Engage || (CMC.Engage = {}));
})(CMC || (CMC = {}));
