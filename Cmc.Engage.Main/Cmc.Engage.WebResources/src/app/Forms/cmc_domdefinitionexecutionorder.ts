/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
/// <reference path="../../../node_modules/@types/kendo-ui/index.d.ts" />
module CMC.Engage.cmc_domdefinitionexecutionorder {
    declare var CampusManagement: any;
    var localizedStrings = {
        errorPrefix: "",
        okButton: "",
        Missing_Valid_DOM_Definition_Logic_for_DOM_Definition_Execution_Order: ""
    }
    var _missingRelatedDomDefinitionId = "missing_domdefinition_warning",
        _fieldNames = {
            attribute: "cmc_attribute",
            attributeSchema: "cmc_attributeschema",
            domMaster: "cmc_dommasterid",
        };

    export enum StateCode {
        Active = 0
    }

    export function onLoad(executionContext) {
        var formContext: Xrm.FormContext = executionContext.getFormContext(),
            domMasterLookup = formContext.getAttribute(_fieldNames.domMaster),
            attribute = formContext.getAttribute(_fieldNames.attribute);
      InitializeDisplayStrings();
        if (domMasterLookup && attribute) {
            validateHasMatchingDomDefinitionLogicRecord(executionContext);

            domMasterLookup.addOnChange(clearNotificationsOnChange);
            attribute.addOnChange(clearNotificationsOnChange);
        }
    }
    
    function validateHasMatchingDomDefinitionLogicRecord(executionContext) {
        var formContext = executionContext.getFormContext(),
            domMasterValue = formContext.getAttribute(_fieldNames.domMaster).getValue(),
            attributeSchemaValue = formContext.getAttribute(_fieldNames.attributeSchema).getValue();

        if (attributeSchemaValue && domMasterValue && domMasterValue.length > 0) {
            var fetchXml = [
                '<fetch top="1">',
                '<entity name="cmc_domdefinitionlogic">',
                '<attribute name="cmc_domdefinitionlogicid" />',
                '<filter type="and">',
                '<condition attribute="cmc_attributeschema" operator="eq" value="' + attributeSchemaValue + '" />',
                '<condition attribute="statecode" operator="eq" value="' + StateCode.Active + '"/>',
                '</filter>',
                '<link-entity name="cmc_domdefinition" from="cmc_domdefinitionid" to="cmc_domdefinitionid" link-type="inner" alias="domdefinition">',
                '<filter type="and">',
                '<condition attribute="cmc_dommasterid" operator="eq" value="' + domMasterValue[0].id + '" />',
                '</filter>',
                '</link-entity>',
                '</entity>',
                '</fetch>'
            ].join('');

            return Xrm.WebApi.retrieveMultipleRecords("cmc_domdefinitionlogic", `?fetchXml=${fetchXml}`)
                .then(
                    function success(result) {
                        if (result.entities.length > 0) {
                            toggleWarningNotifications(true, formContext);
                        } else {
                            toggleWarningNotifications(false, formContext);
                        }
                    },
                    function(error) {
                        CampusManagement.CommonUiControls.openAlertDialog(localizedStrings.errorPrefix + error.message, localizedStrings.okButton);
                    }
                );
        }
        else {
            toggleWarningNotifications(false, formContext);
        }
    }

    function toggleWarningNotifications(isValid, formContext) {
        if (!isValid) {
            formContext.ui.setFormNotification(
                localizedStrings.Missing_Valid_DOM_Definition_Logic_for_DOM_Definition_Execution_Order,
                "WARNING",
                _missingRelatedDomDefinitionId
            );
        }
        else {
            formContext.ui.clearFormNotification(_missingRelatedDomDefinitionId);
        }
    }

    function clearNotificationsOnChange(executionContext) {
        var formContext = executionContext.getFormContext();
        formContext.ui.clearFormNotification(_missingRelatedDomDefinitionId);
  }
  function InitializeDisplayStrings() {
    localizedStrings.errorPrefix = CampusManagement.localization.getResourceString("ErrorPrefix");
    localizedStrings.okButton = CampusManagement.localization.getResourceString("OkButton");
    localizedStrings.Missing_Valid_DOM_Definition_Logic_for_DOM_Definition_Execution_Order = CampusManagement.localization.getResourceString("Missing_Valid_DOM_Definition_Logic_for_DOM_Definition_Execution_Order");
  }
}
