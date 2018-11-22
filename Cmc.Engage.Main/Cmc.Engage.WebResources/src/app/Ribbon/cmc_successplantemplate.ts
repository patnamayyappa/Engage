/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
/// <reference path="../../../src/app/common/retrievemultilingualvalues.ts" />

module CampusManagement.ribbon.cmc_SuccessPlanTemplate {

    declare var SonomaCmc: any;

    var _ignoreSystemAttributes = [
        "createdon", "createdby", "createdonbehalfby",
        "modifiedby", "modifiedon", "modifiedonbehalfby",
        "ownerid", "owningbusinessunit", "owningteam", "owninguser",
        "overridencreatedon", "timezoneruleversionnumber",
        "utcconversiontimezonecode", "versionnumber"
    ],
        _ignoreCustomAttributes = ["cmc_copyfromsuccessplantemplateid", "cmc_successplantemplatename"];

    export function copy() {
        var recordId = SonomaCmc.Guid.decapsulate(Xrm.Page.data.entity.getId()),
            localizedStrings = {
                errorPrefix: CampusManagement.localization.getResourceString("ErrorPrefix"),
                okButton: CampusManagement.localization.getResourceString("OkButton"),
            },
            parameters = {};

        // Xrm.Page is used as there currently isn't a good way to get the execution context
        // or form context into a ribbon script
        if (Xrm.Page.data.entity.getIsDirty()) {
            CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("RecordFieldDirtyMessage"), localizedStrings.okButton);
            return;
        }

        if (!Xrm.Page.getAttribute("cmc_copyfromsuccessplantemplateid")) {
            CampusManagement.CommonUiControls.openAlertDialog(CampusManagement.localization.getResourceString("SuccessPlanTemplate_Copy_FormMissingCopySuccessPlanFieldMessage"), localizedStrings.okButton);
            return;
        }

        Xrm.Page.data.entity.attributes.forEach(function (attribute, index) {
            addAttributeToParameters(parameters, attribute, index);
        });

        parameters["cmc_copyfromsuccessplantemplateid"] = recordId;
        parameters["cmc_copyfromsuccessplantemplateidname"] = Xrm.Page.getAttribute("cmc_successplantemplatename").getValue();

        Xrm.Navigation.openForm({
            entityName: 'cmc_successplantemplate',
            openInNewWindow: true
        }, parameters);
    }

    function addAttributeToParameters(parameters, attribute, index) {
        var attName = attribute.getName(),
            attType = attribute.getAttributeType(),
            attValue = attribute.getValue();

        if (!attValue || _ignoreSystemAttributes.indexOf(attName) !== -1 || _ignoreCustomAttributes.indexOf(attName) !== -1) {
            return;
        }

        if (attType === "lookup" && attValue.length > 0) {
            parameters[attName] = SonomaCmc.Guid.decapsulate(attValue[0].id);
            parameters[attName + 'name'] = attValue[0].name;
        }
        else if (attType === "datetime") {
            parameters[attName] = attValue.toLocaleString();
        }
        else {
            parameters[attName] = attValue;
        }
    }
}